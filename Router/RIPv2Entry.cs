using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class RIPv2Entry
    {
        private byte[] raw;
        private IpV4Address ip;
        private string mask;
        private byte metric;

        public RIPv2Entry(byte[] raw)
        {
            this.raw = raw;
            ip = ToIp(raw[4], raw[5], raw[6], raw[7]);
            mask = ToIp(raw[8], raw[9], raw[10], raw[11]).ToString();
            metric = raw[19];
        }

        private IpV4Address ToIp(byte a, byte b, byte c, byte d)
        {
            return new IpV4Address(a.ToString() + "." + b.ToString() + "." + c.ToString() + "." + d.ToString());
        }

        public IpV4Address Ip { get => ip; set => ip = value; }
        public string Mask { get => mask; set => mask = value; }
        public byte Metric { get => metric; set => metric = value; }
    }
}
