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

            icmpLayer.Identifier = (ushort)req.Packet.Ethernet.IpV4.Icmp[3+2];// 3;//req.packet.Ethernet.IpV4.Icmp.Payload. [1];
            icmpLayer.SequenceNumber = (ushort)req.Packet.Ethernet.IpV4.Icmp[5+2];//req.packet.Ethernet.IpV4.Icmp.Payload[3];

 

            // Create the builder that will build our packets
            //PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, icmpLayer);
            //var p = builder.Build(DateTime.Now);
            //List<byte> load = new List<byte>(p.Buffer);
            List<byte> load = new List<byte>();

            for (int i = 42; i < req.Packet.Buffer.Length; i++)
            {
                load.Add(req.Packet.Buffer[i]);
            }
            //load[17] = 100;
            //string hexS = BitConverter.ToString(load.ToArray()).Replace("-", string.Empty);

            //return Packet.FromHexadecimalString(hexS, DateTime.Now, DataLinkKind.Ethernet);

            PayloadLayer payloadLayer = new PayloadLayer();
            payloadLayer.Data = new Datagram(load.ToArray());
            //builder = new PacketBuilder(load.ToArray());

            PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, icmpLayer, payloadLayer);
            return builder.Build(DateTime.Now);
        }
    }
}
