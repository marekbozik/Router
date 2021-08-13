using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Icmp;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class ICMPPacket : IpV4Packet
    {
        public ICMPPacket(Packet p) : base(p)
        {
        }

        public bool IsReply()
        {
            return packet.Ethernet.IpV4.Icmp.MessageType == PcapDotNet.Packets.Icmp.IcmpMessageType.EchoReply;
        }

        public bool IsRequest()
        {
            return packet.Ethernet.IpV4.Icmp.MessageType == PcapDotNet.Packets.Icmp.IcmpMessageType.Echo;

        }

        public static Packet ICMPReplyPacketBuilder(ICMPPacket req, MacAddress srcMac, IpV4Address srcIp)
        {
            EthernetLayer ethernetLayer = new EthernetLayer
            {
                Source = srcMac,
                Destination = req.SourceMacAddress
            };

            // IPv4 Layer
            IpV4Layer ipV4Layer = new IpV4Layer
            {
                Source = srcIp,
                CurrentDestination = req.SrcIp,
                Ttl = 255,

                // The rest of the important parameters will be set for each packet
            };

            // ICMP Layer
            IcmpEchoReplyLayer icmpLayer = new IcmpEchoReplyLayer();

            icmpLayer.Identifier = 3;//req.packet.Ethernet.IpV4.Icmp.Payload. [1];
            icmpLayer.SequenceNumber = 0;//req.packet.Ethernet.IpV4.Icmp.Payload[3];

            // Create the builder that will build our packets
            PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, icmpLayer);
            return builder.Build(DateTime.Now);
        }
    }
}
