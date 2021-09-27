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
        protected IpV4Address srcIp;
        protected IpV4Address dstIp;

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

        public bool IsIcmp()
        {
            if (packet.Ethernet.IpV4.Protocol == IpV4Protocol.InternetControlMessageProtocol)
                return true;
            return false;
        }

        public bool IsDHCP()
        {
            if (packet.Ethernet.IpV4.Protocol == IpV4Protocol.Udp)
                if (packet.Ethernet.IpV4.Udp.SourcePort == 68 && packet.Ethernet.IpV4.Udp.DestinationPort == 67)
                    return true;
            return false;
        }

        public bool IsRIPv2()
        {
            if (packet.Ethernet.IpV4.Protocol == IpV4Protocol.Udp)
            {
                if (packet.Ethernet.IpV4.Udp.SourcePort == RIPv2Packet.RIPUdpPort && packet.Ethernet.IpV4.Udp.DestinationPort == RIPv2Packet.RIPUdpPort)
                {
                    return true;
                    //if (dstIp == new IpV4Address("224.0.0.9")) return true;
                }
            }
            return false;
        }
    }
}
