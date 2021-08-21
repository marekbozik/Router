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
        public static ushort identifier = 0;

        private ushort seq;
        private ushort id;
        private byte[] data;

        public ushort Seq { get => seq; set => seq = value; }
        public ushort Id { get => id; set => id = value; }
        public byte[] Data { get => data; set => data = value; }

        public ICMPPacket(Packet p) : base(p)
        {
            List<byte> l = new List<byte>(128);
            for (int i = 42; i < p.Buffer.Length; i++)
            {
                l.Add(p.Buffer[i]);
            }
            data = l.ToArray();
            byte[] iden = { p.Ethernet.IpV4.Icmp[3 + 1], p.Ethernet.IpV4.Icmp[3 + 2] };
            string hex = BitConverter.ToString(iden).Replace("-", string.Empty);
            id = ushort.Parse(hex, System.Globalization.NumberStyles.HexNumber);

            byte[] sq = { p.Ethernet.IpV4.Icmp[5 + 1], p.Ethernet.IpV4.Icmp[5 + 2] };
            hex = BitConverter.ToString(sq).Replace("-", string.Empty);
            seq =  ushort.Parse(hex, System.Globalization.NumberStyles.HexNumber);

        }

        public bool IsReply()
        {
            return packet.Ethernet.IpV4.Icmp.MessageType == PcapDotNet.Packets.Icmp.IcmpMessageType.EchoReply;
        }

        public bool IsRequest()
        {
            return packet.Ethernet.IpV4.Icmp.MessageType == PcapDotNet.Packets.Icmp.IcmpMessageType.Echo;

        }

        public static void NewPingSequence() { identifier++; }

        public static ICMPRequestPacket ICMPRequestPacketBuilder(MacAddress srcMac, IpV4Address srcIp, MacAddress dstMac, IpV4Address dstIp, ushort sequenceNumber)
        {
            EthernetLayer ethernetLayer = new EthernetLayer
            {
                Source = srcMac,
                Destination = dstMac
            };

            // IPv4 Layer
            IpV4Layer ipV4Layer = new IpV4Layer
            {
                Source = srcIp,
                CurrentDestination = dstIp,
                Ttl = 255,
            };

            // ICMP Layer
            IcmpEchoLayer icmpEchoLayer = new IcmpEchoLayer();
            icmpEchoLayer.Identifier = identifier;
            icmpEchoLayer.SequenceNumber = sequenceNumber;

            byte[] data = new byte[64];
            new Random().NextBytes(data);
            
            PayloadLayer payloadLayer = new PayloadLayer();
            payloadLayer.Data = new Datagram(data);

            PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, icmpEchoLayer, payloadLayer);

            return new ICMPRequestPacket
            {
                data = data,
                identifier = identifier,
                requestPacket = builder.Build(DateTime.Now),
                sequenceNumber = sequenceNumber
            };   
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
