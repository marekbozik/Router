using PcapDotNet.Packets.IpV4;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class RoutingTable
    {
		//ConcurrentBag<RoutingLog> bag;
        private SynchronizedCollection<RoutingLog> logs;
        private Router router;

        public RoutingTable(Router r)
        {
            //bag = new ConcurrentBag<RoutingLog>();
            logs = new SynchronizedCollection<RoutingLog>();
            SetConnected(r);
            router = r;
        }

        public void SetConnected(Router r)
        {
            int[] arr = new int[2];
            int c = 0;
            int i = 0;
            foreach (var log in logs)
            {
                if (log.Type == RoutingLog.typeConnected)
                {
                    arr[c] = i;
                    c++;
                }
                if (c == 2) break;
                i++;
            }


            try
            {
                for (int j = 1; j >= 0; j--)
                {
                    logs.RemoveAt(arr[j]);
                }
            }
            catch (Exception) { }


            Add(RoutingLog.typeConnected, IpV4.ToNetworkAdress(r.Port1.Ip, r.Port1.Mask), r.Port1.Mask, 1, new IpV4Address());
            Add(RoutingLog.typeConnected, IpV4.ToNetworkAdress(r.Port2.Ip, r.Port2.Mask), r.Port2.Mask, 2, new IpV4Address());

        }

        public List<RIPv2Entry> GetRIPv2LogsFor(IpV4Address netIp, string netMask)
        {
            int cap;
            if (logs.Count - 2 < 0)
                cap = 0;
            else cap = logs.Count - 2;

            List<RIPv2Entry> l = new List<RIPv2Entry>(cap);

            foreach (var log in logs)
            {
                if (log.Type == RoutingLog.typeRIPv2)
                {
                    if (IpV4.IsInSubnet(netIp, netMask, log.NextHop))
                        l.Add(new RIPv2Entry((RIPv2RoutingLog)log));
                }
            }
            return l;
        }

        public string GetMask(IpV4Address netIp)
        {
            foreach (var log in logs)
            {
                if (log.Ip == netIp && log.Type != RoutingLog.typeRIPv2)
                {
                    return log.Mask;
                }
            }
            throw new Exception();
        }

        public bool Contains(IpV4Address netIp)
        {
            foreach (var log in logs)
            {
                if (log.Ip == netIp && log.Type != RoutingLog.typeRIPv2)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(RoutingLog rl)
        {
            foreach (var i in logs)
            {
                if (i == rl) return true;
            }
            return false;
        }

        public RoutingLog GetLog(IpV4Address ip, string mask)
        {
            foreach (var log in logs)
            {
                if (log.Ip == ip && log.Mask == mask)
                    return log;
            }
            throw new Exception();
        }

        public void Add (RoutingLog rl)
        {
            logs.Add(rl);
        }

        public void Add(int type, IpV4Address ip, string mask, int outInt, IpV4Address nextHop)
        {
            ip = IpV4.ToNetworkAdress(ip, mask);
			if ((outInt == 1 || outInt == 2) && nextHop != new IpV4Address())
				logs.Add(new RoutingLog(type, ip, mask, outInt, nextHop));
			else if ((outInt == 1 || outInt == 2))
                logs.Add(new RoutingLog(type, ip, mask, outInt));
			else if (nextHop != new IpV4Address())
                logs.Add(new RoutingLog(type, ip, mask, nextHop));

		}

        public void Add(RIPv2Packet p, RIPv2Entry e)
        {
            logs.Add(new RIPv2RoutingLog(p, e));
        }

        public void Remove(RoutingLog rl)
        {
            int i = 0;
            foreach (var log in logs)
            {
                if (log == rl)
                {
                    break;
                }
                i++;
            }
            logs.RemoveAt(i);
        }

		public void Remove(int i)
        {

            try
            {
                if (logs[i].Type == RoutingLog.typeConnected || logs[i].Type == RoutingLog.typeRIPv2)
                {
                    return;
                }
                List<IpV4Address> toRemove = new List<IpV4Address>();
                foreach (var x in router.ArpTable.Table)
                {
                    if (x.Value.Ip == router.Port1.Ip || x.Value.Ip == router.Port2.Ip) continue;

                    if (IpV4.IsInSubnet(logs[i].Ip, logs[i].Mask, x.Value.Ip))
                    {
                        toRemove.Add(x.Value.Ip);
                    }
                }
                foreach (var x in toRemove)
                {
                    try { router.ArpTable.Remove(x); }
                    catch (Exception) { }
                }
                logs.RemoveAt(i);

            }
            catch (Exception) { }

        }

        public void RIPv2TimersHandle(RIPv2Timer t)
        {
            List<int> toRemove = new List<int>();
            int i = 0;
            foreach (var log in logs)
            {
                if (log.Type == RoutingLog.typeRIPv2)
                {
                    var r = (RIPv2RoutingLog)log;
                    if (r.IsFlushed)
                    {
                        if (Math.Abs((DateTime.Now - r.LastUpdate).TotalSeconds) > (t.Holddown + t.Invalid))
                            toRemove.Add(i);
                    }
                    else if (r.IsInvalid)
                    {
                        if (Math.Abs((DateTime.Now - r.LastUpdate).TotalSeconds) > t.Flush)
                        {
                            r.IsFlushed = true;
                            if (Math.Abs((DateTime.Now - r.LastUpdate).TotalSeconds) > (t.Holddown + t.Invalid))
                                toRemove.Add(i);
                        }
                    }
                    else
                    {
                        if (Math.Abs((DateTime.Now - r.LastUpdate).TotalSeconds) > t.Invalid)
                        {
                            r.IsInvalid = true;
                            if (Math.Abs((DateTime.Now - r.LastUpdate).TotalSeconds) > t.Flush)
                            {
                                r.IsFlushed = true;
                                if (Math.Abs((DateTime.Now - r.LastUpdate).TotalSeconds) > (t.Holddown + t.Invalid))
                                    toRemove.Add(i);
                            }
                        }
                    }
                }
                i++;
            }

            if (toRemove.Count > 0)
            {
                toRemove.Reverse();
                foreach (var rem in toRemove)
                {
                    logs.RemoveAt(rem);
                }
            }

        }

		public int GetOutInt(IpV4Address ip)
        {
            IOrderedEnumerable<RoutingLog> coll = logs.OrderBy(b => b.Mask, new MaskComparer());

            foreach (var z in coll)
            {
				
                if (IpV4.IsInSubnet(z.Ip, z.Mask, ip))
                {
                    if (z.Type == RoutingLog.typeRIPv2)
                    {
                        var log = (RIPv2RoutingLog)z;
                        if (log.IsFlushed) throw new Exception(); 
                    }
                    if (z.OutInt == 1 || z.OutInt == 2)
                    {
						return z.OutInt;
                    }
					else
                    {
                        try
                        {
                            return RecursiveInt(z.NextHop, coll);
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }
                }
            }
			throw new Exception();

        }

		private int RecursiveInt(IpV4Address nextHop, IOrderedEnumerable<RoutingLog> coll)
        {
            foreach (var z in coll)
            {
				if (IpV4.IsInSubnet(z.Ip, z.Mask, nextHop))
                {
					if (z.OutInt == 1 || z.OutInt == 2)
					{
						return z.OutInt;
					}
					else continue;
				}
			}

			throw new Exception();
        }


        private void Sort()
        {
            logs.OrderBy(b => b.Mask, new MaskComparer());
        }

        class MaskComparer : IComparer<string>
        {
            public int Compare(string mask1, string mask2)
            {
				var m1 = mask1.Split('.');
				var m2 = mask2.Split('.');

				int x = 1000;
				int hash1 = 0;
				for (int i = 0; i < m1.Length; i++)
				{
					hash1 += Int32.Parse(m1[i]) * x;
					x = x / 10;
				}

				x = 1000;
				int hash2 = 0;
				for (int i = 0; i < m2.Length; i++)
				{
					hash2 += Int32.Parse(m2[i]) * x;
					x = x / 10;
				}

				if (hash1 > hash2)
					return -1;
				else if (hash1 < hash2)
					return 1;
				return 0;
			}
        }

        public List<string> GetTable()
        {
            List<string> l = new List<string>();
            //l.Add("Type |        Ip       |       Mask      | Out int | NextHop");
            // "  C  | 255.255.255.255 | 255.255.255.255 |    1    | "
            foreach (var log in logs)
            {
                if (log.Type == RoutingLog.typeRIPv2)
                {
                    if (((RIPv2RoutingLog)log).IsFlushed) continue;
                }
                l.Add(log.ToString());
            }
            return l;
        }

        //private static int MaskSort(string mask1, string mask2)
        //{
        //    var m1 = mask1.Split('.');
        //    var m2 = mask2.Split('.');

        //    int x = 1000;
        //    int hash1 = 0;
        //    for (int i = 0; i < m1.Length; i++)
        //    {
        //        hash1 += Int32.Parse(m1[i]) * x;
        //        x = x / 10;
        //    }

        //    x = 1000;
        //    int hash2 = 0;
        //    for (int i = 0; i < m2.Length; i++)
        //    {
        //        hash2 += Int32.Parse(m2[i]) * x;
        //        x = x / 10;
        //    }

        //    if (hash1 > hash2)
        //        return -1;
        //    else if (hash1 < hash2)
        //        return 1;
        //    return 0;
        //}




    }
}
