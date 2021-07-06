﻿using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class ArpTable
    {
        private ConcurrentDictionary<IpV4Address, ArpLog> table;
        private int agingTime;

        public int AgingTime { get => agingTime; set => agingTime = value; }

        public ArpTable(int agingTime, Router r)
        {
            table = new ConcurrentDictionary<IpV4Address, ArpLog>();
            this.agingTime = agingTime;
            table[r.Port1.Ip] = new ArpLog(r.Port1.Ip, r.Port1.Mac, 1, DateTime.MaxValue);
            table[r.Port2.Ip] = new ArpLog(r.Port2.Ip, r.Port2.Mac, 2, DateTime.MaxValue);
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
        }

        public IpV4Address FindMinAgingTime()
        {
            double min = Double.MaxValue;
            IpV4Address ip = new IpV4Address();
            foreach (var i in table)
            {
                var x = (DateTime.Now - i.Value.Time).TotalMilliseconds;
                if (x <= min)
                {
                    min = x;
                    ip = new IpV4Address(i.Value.Ip.ToString());
                }
            }
            return ip;
        }

        public void Remove(IpV4Address ip)
        {
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
                for (int j = 0; j < 19 - len; j++)
                {
                    s += " ";
                }
                s += " | " + i.Value.Mac.ToString() + " |   " + i.Value.Port.ToString() + "  | " + Math.Abs((DateTime.Now - i.Value.Time).TotalSeconds).ToString() + "\n";
                l.Add(s);
            }
            return l;
        }
        /*
        //returns oldest mac or broadcast when table is empty
        public MacAddress CleanTable()
        {
            List<MacAddress> removeL = new List<MacAddress>();
            DateTime min = DateTime.Now;
            MacAddress last = new MacAddress("FF:FF:FF:FF:FF:FF");
            foreach (var i in table)
            {
                if ((DateTime.Now - i.Value.Time).TotalSeconds > agingTime)
                {
                    removeL.Add(new MacAddress(i.Key.ToString()));
                }
                else if (i.Value.Time < min)
                {
                    min = i.Value.Time;
                    last = new MacAddress(i.Value.Mac.ToString());
                }
            }

            foreach (var i in removeL)
            {
                var x = table[i];
                table.TryRemove(i, out x);
            }

            return last;
        }

        public double LogAgeMiliseconds(IpV4Address m)
        {
            double x = 0;
            try
            {
                x = (DateTime.Now - table[m].Time).TotalMilliseconds;
            }
            catch (Exception) { }

            return x;
        }
        */
    }
}
