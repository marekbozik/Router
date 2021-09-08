using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Router
{
    class ArpTable
    {
        private ConcurrentDictionary<IpV4Address, ArpLog> table;
        private ConcurrentDictionary<IpV4Address, ArpRequestLog> requestsTable;

        private int agingTime;

        public int AgingTime { get => agingTime; set => agingTime = value; }
        internal ConcurrentDictionary<IpV4Address, ArpLog> Table { get => table; set => table = value; }

        public ArpTable(int agingTime, Router r)
        {                                                        //concurrent level, initial size
            table = new ConcurrentDictionary<IpV4Address, ArpLog>(4, 67);
            requestsTable = new ConcurrentDictionary<IpV4Address, ArpRequestLog>(4, 19);

            this.agingTime = agingTime;
            table[r.Port1.Ip] = new ArpLog(r.Port1.Ip, r.Port1.Mac, 1, DateTime.MaxValue);
            table[r.Port2.Ip] = new ArpLog(r.Port2.Ip, r.Port2.Mac, 2, DateTime.MaxValue);
            new Thread(() => { AutoRemove(); }).Start();
        }

        private void AutoRemove()
        {
            IpV4Address nextD = new IpV4Address();
            while (true)
            {
                try
                {
                    nextD = FindNextDelete();
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                int ttd = TimeToDelete(GetLog(nextD).Time);
                if (ttd < 0)
                {
                    try
                    {
                        Remove(nextD);
                    }
                    catch (Exception) { }
                    continue;
                }

                if (ttd < 1000)
                {
                    Thread.Sleep(ttd);
                    try
                    {
                        Remove(nextD);
                    }
                    catch (Exception) { }
                    
                }
                else
                {
                    bool change = false;
                    int oldTimer = agingTime;
                    for (int i = 0; i < ttd/1000; i++)
                    {
                        if (oldTimer != agingTime)
                        {
                            change = true;
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                    if (change)
                    {
                        continue;
                    }

                    try
                    {
                        Remove(nextD);
                    }
                    catch (Exception) { }
                    
                }



            }

        }

        private int TimeToDelete(DateTime dt)
        {
            if ((DateTime.Now - dt).TotalSeconds > agingTime)
            {
                return -1;
            }
            return (int)( (agingTime - (DateTime.Now- dt).TotalSeconds) * 1000);
        }

        public void RegisterArpRequest(IpV4Address ip, IpV4Address srcIp, MacAddress srcMac, int port)
        {
            requestsTable[ip] = new ArpRequestLog(port, srcMac, srcIp);
        }

        public ArpRequestLog GetRegistredArp(IpV4Address ip)
        {
            if (requestsTable.ContainsKey(ip))
                return requestsTable[ip];
            else
                throw new Exception();
        }

        public bool IsExpectedReply(IpV4Address ip, int port)
        {
            if (requestsTable.ContainsKey(ip))
                if (requestsTable[ip].Port == port)
                    return true;
            return false;
        }

        public void RegisterArpReply(IpV4Address ip)
        {
            requestsTable.TryRemove(ip, out _);
        }

        public void UpdatePortsIp(Router r)
        {
            IpV4Address ip = new IpV4Address();
            ArpLog log;
            for (int i = 0; i < 2; i++)
            {
                foreach (var j in table)
                {
                    if (j.Value.Time == DateTime.MaxValue)
                    {
                        ip = new IpV4Address(j.Value.Ip.ToString());
                    }
                }
                table.TryRemove(ip, out log);
            }

            table[r.Port1.Ip] = new ArpLog(r.Port1.Ip, r.Port1.Mac, 1, DateTime.MaxValue);
            table[r.Port2.Ip] = new ArpLog(r.Port2.Ip, r.Port2.Mac, 2, DateTime.MaxValue);

            List<IpV4Address> toRemoveList = new List<IpV4Address>();
            foreach (var a in table)
            {
                if (a.Value.Ip == r.Port1.Ip || a.Value.Ip == r.Port2.Ip) continue;
                else
                {
                    if (IpV4.IsInSubnet(r.Port1.Ip, r.Port1.Mask, a.Value.Ip) || IpV4.IsInSubnet(r.Port2.Ip, r.Port2.Mask, a.Value.Ip))
                    {
                        continue;
                    }
                    else
                    {
                        toRemoveList.Add(new IpV4Address(a.Value.Ip.ToString()));
                    }
                }
            }
            foreach (var rem in toRemoveList)
            {
                table.TryRemove(rem, out log);
            }
        }

        public IpV4Address FindNextDelete()
        {
            DateTime min = DateTime.MaxValue;
            IpV4Address ip = new IpV4Address("255.255.255.255");
            foreach (var i in table)
            {
                if (i.Value.Time == DateTime.MaxValue) continue;

                if (i.Value.Time < min)
                {
                    min = i.Value.Time;
                    ip = new IpV4Address(i.Value.Ip.ToString());
                }
            }
            //MessageBox.Show(ip.ToString());
            //Console.WriteLine();
            if (min == DateTime.MaxValue)
                throw new Exception();
            if (ip != new IpV4Address("255.255.255.255"))
                return ip;
            else
                throw new Exception();
        }

        public void Remove(IpV4Address ip)
        {
            if (table[ip].Time == DateTime.MaxValue)
                return;
            ArpLog log;
            table.TryRemove(ip, out log);
        }

        public void Remove(int port)
        {
            List<IpV4Address> toRemove = new List<IpV4Address>();
            foreach (var i in table)
            {
                if (i.Value.Port == port)
                {
                    toRemove.Add(new IpV4Address(i.Value.Ip.ToString()));
                }
            }
            ArpLog log;
            foreach (var i in toRemove)
            {
                table.TryRemove(i, out log);
            }
        }

        public void Add(ArpPacket arpP, int port)
        {
            if (arpP.IsReply())
            {
                table[arpP.SourceIp] = new ArpLog(arpP.SourceIp, arpP.SourceMacAddress, port, DateTime.Now);
            }
        }

        public void ClearTable() { table.Clear(); }

        public bool Contains(IpV4Address ip)
        {
            return table.ContainsKey(ip);
        }

        public int GetPort(IpV4Address ip)
        {
            if (this.Contains(ip))
            {
                return table[ip].Port;
            }
            else
                throw new Exception();
        }

        public ArpLog GetLog(IpV4Address ip)
        {
            if (this.Contains(ip))
            {
                var x = table[ip];
                return new ArpLog(x.Ip, x.Mac, x.Port, x.Time);
            }
            else
                throw new Exception();
        }

        public override string ToString()
        {               //255.255.255.255.255 | 00:00:00:00:02:02 | 1 
            string s =   "     IP             |        Mac        | Port | Timer \n";
            foreach (var i in table)
            {
                int len = i.Value.Ip.ToString().Length;
                s += i.Value.Ip.ToString();
                for (int j = 0; j < 19 - len; j++)
                {
                    s += " ";
                }
                s +=  " | " + i.Value.Mac.ToString() + " |   " + i.Value.Port.ToString() + "  | " + Math.Abs((DateTime.Now - i.Value.Time).TotalSeconds).ToString() + "\n";
            }
            return s;
        }

        public List<string> GetTable()
        {
            List<string> l = new List<string>();
            //l.Add("     IP             |        Mac        | Port | Timer \n");
            foreach (var i in table)
            {
                string s = "";
                int len = i.Value.Ip.ToString().Length;
                s += i.Value.Ip.ToString();

                string time = null;
                if (i.Value.Time != DateTime.MaxValue)
                    time = Math.Abs((DateTime.Now - i.Value.Time).TotalSeconds).ToString();
                else
                    time = "-";

                for (int j = 0; j < 19 - len; j++)
                {
                    s += " ";
                }
                s += " | " + i.Value.Mac.ToString() + " |   " + i.Value.Port.ToString() + "  | " + time + "\n";
                l.Add(s);
            }
            return l;
        }
       
    }
}
