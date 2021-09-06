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
        private SynchronizedCollection<RoutingLog> logs;
        private Router router;

        public RoutingTable(Router r)
        {
            logs = new SynchronizedCollection<RoutingLog>();
            SetConnected(r);
            router = r;
        }

        public void SetConnected(Router r)
        {
            if (logs.Count == 0)
            {
                Add(RoutingLog.typeConnected, IpV4.ToNetworkAddress(r.Port1.Ip, r.Port1.Mask), r.Port1.Mask, 1, new IpV4Address());
                Add(RoutingLog.typeConnected, IpV4.ToNetworkAddress(r.Port2.Ip, r.Port2.Mask), r.Port2.Mask, 2, new IpV4Address());
            }
            else
            {
                logs[0] = new RoutingLog(RoutingLog.typeConnected, IpV4.ToNetworkAddress(r.Port1.Ip, r.Port1.Mask), r.Port1.Mask, 1);
                logs[1] = new RoutingLog(RoutingLog.typeConnected, IpV4.ToNetworkAddress(r.Port2.Ip, r.Port2.Mask), r.Port2.Mask, 2);
            }          
        }

        public List<RIPv2Entry> GetRIPv2LogsFor(IpV4Address netIp, string netMask)
        {
            int cap;
            if (logs.Count - 2 < 0)
                cap = 0;
            else cap = logs.Count - 2;

            List<RIPv2Entry> l = new List<RIPv2Entry>(cap);

            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
                if (log.Type == RoutingLog.typeRIPv2)
                {
                    //split horizon
                    if (!IpV4.IsInSubnet(netIp, netMask, log.NextHop))
                    {
                        var toAdd = new RIPv2Entry((RIPv2RoutingLog)log);
                        if (((RIPv2RoutingLog)log).IsOnHoldDown)
                            toAdd.Metric = 16;
                        l.Add(toAdd);
                    }
                }
            }
            return l;
        }

        public string GetMask(IpV4Address netIp)
        {
            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
                if (log.Ip == netIp && log.Type != RoutingLog.typeRIPv2)
                {
                    return log.Mask;
                }
            }
            throw new Exception();
        }

        public bool Contains(IpV4Address netIp)
        {
            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
                if (log.Ip == netIp && log.Type != RoutingLog.typeRIPv2)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(RoutingLog rl)
        {
            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
                if (log == rl) return true;
            }
            return false;
        }

        public RoutingLog GetLog(IpV4Address ip, string mask)
        {
            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
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
            ip = IpV4.ToNetworkAddress(ip, mask);
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

        public void SetPossiblyDown(RIPv2RoutingLog rl, RIPv2Timer timers)
        {
            int j = 0;
            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
                if (log == rl)
                {
                    ((RIPv2RoutingLog)logs[i]).LastUpdate = DateTime.Now - TimeSpan.FromSeconds(timers.Flush);
                    ((RIPv2RoutingLog)logs[i]).IsInvalid = true;
                    ((RIPv2RoutingLog)logs[i]).IsOnHoldDown = true;
                    ((RIPv2RoutingLog)logs[i]).IsFlushed = true;
                }
                j++;
            }
        }

        public void Remove(RoutingLog rl)
        {
            int j = 0;
            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
                if (log == rl)
                {
                    break;
                }
                j++;
            }
            logs.RemoveAt(j);
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
            int j = 0;
            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
                if (log.Type == RoutingLog.typeRIPv2)
                {
                    var r = (RIPv2RoutingLog)log;

                    if (r.IsOnHoldDown)
                    {
                        if (Math.Abs((DateTime.Now - r.LastUpdate).TotalSeconds) > (t.Holddown + t.Invalid))
                            r.IsOnHoldDown = false;
                    }

                    if (r.IsFlushed)
                    {
                        if (Math.Abs((DateTime.Now - r.LastUpdate).TotalSeconds) > (t.Holddown + t.Invalid))
                            toRemove.Add(j);
                    }
                    else if (r.IsInvalid)
                    {
                        if (Math.Abs((DateTime.Now - r.LastUpdate).TotalSeconds) > t.Flush)
                        {
                            r.IsFlushed = true;
                            if (Math.Abs((DateTime.Now - r.LastUpdate).TotalSeconds) > (t.Holddown + t.Invalid))
                                toRemove.Add(j);
                        }
                    }
                    else
                    {
                        if (Math.Abs((DateTime.Now - r.LastUpdate).TotalSeconds) > t.Invalid)
                        {
                            r.IsInvalid = true;
                            r.IsOnHoldDown = true;
                            if (Math.Abs((DateTime.Now - r.LastUpdate).TotalSeconds) > t.Flush)
                            {
                                r.IsFlushed = true;
                                if (Math.Abs((DateTime.Now - r.LastUpdate).TotalSeconds) > (t.Holddown + t.Invalid))
                                    toRemove.Add(j);
                            }
                        }
                    }
                }
                j++;
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
            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
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
