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
        private bool forwarding;
        private static readonly HashSet<string> masks = new HashSet<string> {"0.0.0.0", "128.0.0.0", "192.0.0.0", "224.0.0.0", "240.0.0.0", "248.0.0.0", "252.0.0.0", "254.0.0.0", "255.0.0.0", "255.128.0.0", "255.192.0.0", "255.224.0.0", "255.240.0.0", "255.248.0.0", "255.252.0.0", "255.254.0.0", "255.255.0.0", "255.255.128.0", "255.255.192.0", "255.255.224.0", "255.255.240.0", "255.255.248.0", "255.255.252.0", "255.255.254.0", "255.255.255.0", "255.255.255.128", "255.255.255.192", "255.255.255.224", "255.255.255.240", "255.255.255.248", "255.255.255.252", "255.255.255.254", "255.255.255.255" };


        public RouterPort(PacketDevice DeviceInterface, IpV4Address ip, string Mask)
        {
            this.DeviceInterface = DeviceInterface;
            this.IpAddress = ip;
            if (masks.Contains(Mask))
                this.Mask = Mask;
            else
                this.Mask = "255.255.255.0";
            forwarding = false;
        }

        private string SetMask(string value)
        {
            if (masks.Contains(value))
                return value;
            else
                return "255.255.255.0";
        }

        public string Mask1 { get => Mask; set => Mask = SetMask(value);  }
        public IpV4Address IpAddress1 { get => IpAddress; set => IpAddress = value; }
        public PacketDevice DeviceInterface1 { get => DeviceInterface; set => DeviceInterface = value; }
        public bool Forwarding { get => forwarding; set => forwarding = value; }
    }
}
