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
        protected byte[] raw;
        protected IpV4Address ip;
        protected string mask;
        protected byte metric;

        protected RIPv2Entry(RIPv2Entry e)
        {
            this.ip = e.Ip;
            this.mask =e.Mask;
            this.metric = e.Metric;
        }

        public RIPv2Entry(IpV4Address ip, string mask, byte metric)
        {
            this.ip = ip;
            this.mask = mask;
            this.metric = metric;
        }

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
