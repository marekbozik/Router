using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class IpV4
    {
        private static readonly HashSet<string> masks = new HashSet<string> { "0.0.0.0", "128.0.0.0", "192.0.0.0", "224.0.0.0", "240.0.0.0", "248.0.0.0", "252.0.0.0", "254.0.0.0", "255.0.0.0", "255.128.0.0", "255.192.0.0", "255.224.0.0", "255.240.0.0", "255.248.0.0", "255.252.0.0", "255.254.0.0", "255.255.0.0", "255.255.128.0", "255.255.192.0", "255.255.224.0", "255.255.240.0", "255.255.248.0", "255.255.252.0", "255.255.254.0", "255.255.255.0", "255.255.255.128", "255.255.255.192", "255.255.255.224", "255.255.255.240", "255.255.255.248", "255.255.255.252", "255.255.255.254", "255.255.255.255" };

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
    }
}
