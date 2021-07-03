using PcapDotNet.Packets.Ethernet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class MacLog
    {
        private MacAddress mac;
        private int port;
        private DateTime time;

        public MacLog(MacAddress mac, int port, DateTime time)
        {
            this.mac = mac;
            this.port = port;
            this.time = time;
        }

        public MacAddress Mac { get => mac; set => mac = value; }
        public int Port { get => port; set => port = value; }
        public DateTime Time { get => time; set => time = value; }
    }
}
