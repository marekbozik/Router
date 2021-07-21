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
		ConcurrentBag<RoutingLog> bag;
        SynchronizedCollection<RoutingLog> logs;

        public RoutingTable(Router r)
        {
            bag = new ConcurrentBag<RoutingLog>();
            logs = new SynchronizedCollection<RoutingLog>();
            SetConnected(r);
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

		public void Remove(int i)
        {

            try
            {
                if (logs[i].Type == RoutingLog.typeConnected)
                {
                    return;
                }
                logs.RemoveAt(i);
            }
            catch (Exception) { }

        }

		public int GetOutInt(IpV4Address ip)
        {
            IOrderedEnumerable<RoutingLog> coll = logs.OrderBy(b => b.Mask, new MaskComparer());

            foreach (var z in coll)
            {
				
                if (IpV4.IsInSubnet(z.Ip, z.Mask, ip))
                {
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
