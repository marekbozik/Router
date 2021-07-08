using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Arp;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;

namespace Router
{
    class ArpPacket : GenericPacket
    {
        private IpV4Address sourceIp;
        private IpV4Address destinationIp;

        public IpV4Address SourceIp { get => sourceIp; set => sourceIp = value; }
        public IpV4Address DestinationIp { get => destinationIp; set => destinationIp = value; }

        public ArpPacket (Packet p) : base(p)
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

        public bool IsReply()
        {
            if (packet.Ethernet.Arp.Operation == PcapDotNet.Packets.Arp.ArpOperation.Reply)
                return true;
            return false;
        }

        public static Packet ArpPacketBuilder( ArpOperation operation,
                                        MacAddress srcMac,
                                        MacAddress dstMac,
                                        IpV4Address srcIp,
                                        IpV4Address dstIp
                                       )
        {
            EthernetLayer ethernetLayer =
                new EthernetLayer
                {
                    Source = srcMac,
                    Destination = dstMac,
                    EtherType = EthernetType.None, 
                };

            ArpLayer arpLayer =
                new ArpLayer
                {
                    ProtocolType = EthernetType.IpV4,
                    Operation = operation,
                    SenderHardwareAddress = new ReadOnlyCollection<byte>(srcMac.ToString().Split(':').Select(x => Convert.ToByte(x, 16)).ToArray()),
                    SenderProtocolAddress = new ReadOnlyCollection<byte>(IPAddress.Parse(srcIp.ToString()).GetAddressBytes()), 
                    TargetHardwareAddress = new ReadOnlyCollection<byte>(dstMac.ToString().Split(':').Select(x => Convert.ToByte(x, 16)).ToArray()), 
                    TargetProtocolAddress = new ReadOnlyCollection<byte>(IPAddress.Parse(dstIp.ToString()).GetAddressBytes()),
                };

            PacketBuilder builder = new PacketBuilder(ethernetLayer, arpLayer);

            return builder.Build(DateTime.Now);
        }

        public static bool IsArp(Packet p)
        {
            if (p.Ethernet.EtherType == PcapDotNet.Packets.Ethernet.EthernetType.Arp)
                return true;
            return false;
        }

    }
}
