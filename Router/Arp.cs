using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Ethernet;

namespace Router
{
    class Arp : GenericPacket
    {
        private IpV4Address sourceIp;
        private IpV4Address destinationIp;

        public IpV4Address SourceIp { get => sourceIp; set => sourceIp = value; }
        public IpV4Address DestinationIp { get => destinationIp; set => destinationIp = value; }

        public Arp (Packet p) : base(p)
        {            
            sourceIp = p.Ethernet.Arp.SenderProtocolIpV4Address;
            destinationIp = p.Ethernet.Arp.TargetProtocolIpV4Address;
        }

        public bool IsRequest()
        {
            if (packet.Ethernet.Arp.Operation == PcapDotNet.Packets.Arp.ArpOperation.Request)
                return true;
            return false;
        }

        public static bool IsArp(Packet p)
        {
            if (p.Ethernet.EtherType == PcapDotNet.Packets.Ethernet.EthernetType.Arp)
                return true;
            return false;
        }

    }
}
