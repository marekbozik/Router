using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class ArpRequestLog
    {
        private int port;
        private MacAddress srcMac;
        private IpV4Address srcIp;

        public ArpRequestLog(int port, MacAddress srcMac, IpV4Address srcIp)
        {
            this.port = port;
            this.srcMac = srcMac;
            this.srcIp = srcIp;
        }

        public int Port { get => port; }
        public MacAddress SrcMac { get => srcMac; }
        public IpV4Address SrcIp { get => srcIp;  }
    }
}
