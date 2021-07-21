using PcapDotNet.Packets.IpV4;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class IpV4
    {
        private static readonly HashSet<string> masks = new HashSet<string> { "0.0.0.0", "128.0.0.0", "192.0.0.0", "224.0.0.0", "240.0.0.0", "248.0.0.0", "252.0.0.0", "254.0.0.0", "255.0.0.0", "255.128.0.0", "255.192.0.0", "255.224.0.0", "255.240.0.0", "255.248.0.0", "255.252.0.0", "255.254.0.0", "255.255.0.0", "255.255.128.0", "255.255.192.0", "255.255.224.0", "255.255.240.0", "255.255.248.0", "255.255.252.0", "255.255.254.0", "255.255.255.0", "255.255.255.128", "255.255.255.192", "255.255.255.224", "255.255.255.240", "255.255.255.248", "255.255.255.252", "255.255.255.254", "255.255.255.255" };

        public static bool IsInSubnet(IpV4Address routerIp, string mask, IpV4Address other)
        {
            IpMatcher.Matcher m = new IpMatcher.Matcher();
            m.Add(routerIp.ToString(), mask);
            return m.MatchExists(other.ToString());
        }

        public static bool IsConflict(IpV4Address ip1, string mask1, IpV4Address ip2, string mask2)
        {
            IpMatcher.Matcher m = new IpMatcher.Matcher();
            m.Add(ip1.ToString(), mask1);
            IpMatcher.Matcher mm = new IpMatcher.Matcher();
            mm.Add(ip2.ToString(), mask2);

            return m.MatchExists(ip2.ToString()) || mm.MatchExists(ip1.ToString());
        }

        public static bool IsMask(string s)
        {
            return masks.Contains(s);
        }

        public static IpV4Address ToNetworkAdress(IpV4Address ip, string mask)
        {

			string ip1 = ip.ToString();
			var x = ip1.Split('.');
			List<byte> l = new List<byte>(4);
			foreach (var ii in x)
			{
				l.Add(Byte.Parse(ii));
			}

			BitArray bits = new BitArray(l.ToArray());


			var xx = mask.Split('.');
			List<byte> ll = new List<byte>(4);
			foreach (var ii in xx)
			{
				ll.Add(Byte.Parse(ii));
			}

			bits = bits.And(new BitArray(ll.ToArray()));

			string s = "";

			double num = 0;
			for (int i = 0; i < 8; i++)
			{
				char c = bits[i] ? '1' : '0';
				if (c == '1')
				{
					num += Math.Pow(2, i);
				}

			}

			s += num + ".";
			//Console.Write(num + ".");

			num = 0;
			for (int i = 0; i < 8; i++)
			{
				char c = bits[i + 8] ? '1' : '0';
				if (c == '1')
				{
					num += Math.Pow(2, i);
				}

			}

			s += num + ".";


			num = 0;
			for (int i = 0; i < 8; i++)
			{
				char c = bits[i + 8 + 8] ? '1' : '0';
				if (c == '1')
				{
					num += Math.Pow(2, i);
				}

			}

			s += num + ".";


			num = 0;
			for (int i = 0; i < 8; i++)
			{
				char c = bits[i + 8 + 8 + 8] ? '1' : '0';
				if (c == '1')
				{
					num += Math.Pow(2, i);
				}

			}

			s += num;

			return new IpV4Address(s);
		}
	}
}
