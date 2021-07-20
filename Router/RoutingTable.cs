using PcapDotNet.Packets.IpV4;
using System;
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

        public RoutingTable(Router r)
        {
            bag = new ConcurrentBag<RoutingLog>();
            SetConnected(r);
        }

        public void SetConnected(Router r)
        {
            int c = 0;
            foreach (var z in bag)
            {
                if (z.Type == RoutingLog.typeConnected)
                {
                    z.Removed = true;
                    c++;
                }
                if (c == 2) break;
            }

            Add(RoutingLog.typeConnected, r.Port1.Ip, r.Port1.Mask, 1, new IpV4Address());
            Add(RoutingLog.typeConnected, r.Port2.Ip, r.Port2.Mask, 2, new IpV4Address());
        }

        public void Add(int type, IpV4Address ip, string mask, int outInt, IpV4Address nextHop)
        {
			if ((outInt == 1 || outInt == 2) && nextHop != new IpV4Address())
				bag.Add(new RoutingLog(type, ip, mask, outInt, nextHop));
			else if ((outInt == 1 || outInt == 2))
				bag.Add(new RoutingLog(type, ip, mask, outInt));
			else if (nextHop != new IpV4Address())
				bag.Add(new RoutingLog(type, ip, mask, nextHop));

			Sort();
		}

		public void Remove(int i)
        {
			int j = 0;
            foreach (var z in bag)
            {
                if (j == i)
                {
					z.Removed = true;
                }
				j++;
            }
        }

		public int GetOutInt(IpV4Address ip)
        {
            foreach (var z in bag)
            {
				if (z.Removed) continue;
                
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
                            return RecursiveInt(z.NextHop);
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

		private int RecursiveInt(IpV4Address nextHop)
        {
            foreach (var z in bag)
            {
				if (z.Removed) continue;

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
		

		private IOrderedEnumerable Sort()
        {
			return bag.OrderBy(b => b.Mask, new MaskComparer());
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
            foreach (var z in bag)
            {
                if (!z.Removed)
                    l.Add(z.ToString());
            }
            return l;
        }

        //      private static int MaskSort(string mask1, string mask2)
        //{
        //	var m1 = mask1.Split('.');
        //	var m2 = mask2.Split('.');

        //	int x = 1000;
        //	int hash1 = 0;
        //	for (int i = 0; i < m1.Length; i++)
        //	{
        //		hash1 += Int32.Parse(m1[i]) * x;
        //		x = x / 10;
        //	}

        //	x = 1000;
        //	int hash2 = 0;
        //	for (int i = 0; i < m2.Length; i++)
        //	{
        //		hash2 += Int32.Parse(m2[i]) * x;
        //		x = x / 10;
        //	}

        //	if (hash1 > hash2)
        //		return -1;
        //	else if (hash1 < hash2)
        //		return 1;
        //	return 0;
        //}




    }
}
