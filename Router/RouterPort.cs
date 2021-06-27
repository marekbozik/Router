using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Core;
using PcapDotNet.Packets.IpV4;

namespace Router
{
    class RouterPort
    {
        private PacketDevice DeviceInterface;
        private IpV4Address IpAddress;
        private string Mask;

        public RouterPort(PacketDevice DeviceInterface, IpV4Address ip, string Mask)
        {
            this.DeviceInterface = DeviceInterface;
            this.IpAddress = ip;
            this.Mask = Mask;
        }

        public string Mask1 { get => Mask; set => Mask = value; }
        public IpV4Address IpAddress1 { get => IpAddress; set => IpAddress = value; }
        public PacketDevice DeviceInterface1 { get => DeviceInterface; set => DeviceInterface = value; }
    }
}
