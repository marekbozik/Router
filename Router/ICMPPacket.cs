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
            };

            // ICMP Layer
            IcmpEchoReplyLayer icmpLayer = new IcmpEchoReplyLayer();

            byte[] iden = { req.Packet.Ethernet.IpV4.Icmp[3 + 1], req.Packet.Ethernet.IpV4.Icmp[3 + 2] };
            string hex = BitConverter.ToString(iden).Replace("-", string.Empty);
            icmpLayer.Identifier = ushort.Parse(hex, System.Globalization.NumberStyles.HexNumber);

            byte[] seq = { req.Packet.Ethernet.IpV4.Icmp[5 + 1], req.Packet.Ethernet.IpV4.Icmp[5 + 2] };
            hex = BitConverter.ToString(seq).Replace("-", string.Empty);
            icmpLayer.SequenceNumber = ushort.Parse(hex, System.Globalization.NumberStyles.HexNumber);


            List<byte> data = new List<byte>(req.Packet.Buffer.Length - 42);

            for (int i = 42; i < req.Packet.Buffer.Length; i++)
            {
                data.Add(req.Packet.Buffer[i]);
            }

            PayloadLayer payloadLayer = new PayloadLayer();
            payloadLayer.Data = new Datagram(data.ToArray());

            PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, icmpLayer, payloadLayer);
            return builder.Build(DateTime.Now);
        }
    }
}
