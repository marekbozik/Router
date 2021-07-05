using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class ArpLog
    {
        private IpV4Address ip;
        private MacAddress mac;
        private int port;
        private DateTime time;

        public ArpLog(IpV4Address ip, MacAddress mac, int port, DateTime time)
        {
            this.ip = ip;
            this.mac = mac;
            this.port = port;
            this.time = time;
        }

        public IpV4Address Ip { get => ip; set => ip = value; }
        public MacAddress Mac { get => mac; set => mac = value; }
        public int Port { get => port; set => port = value; }
        public DateTime Time { get => time; set => time = value; }
    }
}
