using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class IpV4Packet : GenericPacket
    {
        private IpV4Address srcIp;
        private IpV4Address dstIp;

        public IpV4Packet(Packet p) : base(p)
        {
            if (!IsIpV4Packet(p)) throw new Exception();
            if (p.Ethernet.EtherType == PcapDotNet.Packets.Ethernet.EthernetType.IpV4)
            {
                srcIp = p.Ethernet.IpV4.Source;
                dstIp = p.Ethernet.IpV4.Destination;
            }
            else if (p.Ethernet.EtherType == PcapDotNet.Packets.Ethernet.EthernetType.Arp)
            {
                srcIp = p.Ethernet.Arp.SenderProtocolIpV4Address;
                dstIp = p.Ethernet.Arp.TargetProtocolIpV4Address;
            }
            else throw new Exception();
        }

        public IpV4Address SrcIp { get => srcIp; }
        public IpV4Address DstIp { get => dstIp; }

        public static bool IsIpV4Packet(Packet p)
        {
            return (p.Ethernet.EtherType == PcapDotNet.Packets.Ethernet.EthernetType.IpV4 || p.Ethernet.EtherType == PcapDotNet.Packets.Ethernet.EthernetType.Arp);
        }

        public static bool IsIpV4Packet(GenericPacket p)
        {
            return IsIpV4Packet(p.Packet);
        }
    }
}
