using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;

namespace Router
{
    class RouterPort
    {
        private PacketCommunicator sender;
        private PacketDevice deviceInterface;
        private IpV4Address ipAddress;
        private MacAddress mac;
        private string mask;
        private bool forwarding;

        public RouterPort(PacketDevice DeviceInterface, IpV4Address ip, string Mask, MacAddress mac)
        {
            this.deviceInterface = DeviceInterface;
            this.ipAddress = ip;
            if (IpV4.IsMask(Mask))
                this.mask = Mask;
            else
                this.mask = "255.255.255.0";
            this.mac = mac;
            forwarding = false;
        }

        private string SetMask(string value)
        {
            if (IpV4.IsMask(value))
                return value;
            else
                return "255.255.255.0";
        }
        
        public string Mask { get => mask; set => mask = SetMask(value);  }
        public IpV4Address Ip { get => ipAddress; set => ipAddress = value; }
        public PacketDevice DeviceInterface { get => deviceInterface; set => deviceInterface = value; }
        public bool Forwarding { get => forwarding; set => forwarding = value; }
        public MacAddress Mac { get => mac; }
        public PacketCommunicator Sender { get => sender; set => sender = value; }
    }
}
