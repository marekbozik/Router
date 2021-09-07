using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class RIPv2Packet : IpV4Packet
    {
        public static readonly ushort RIPUdpPort = 520;
        public static readonly byte RIPv2CommandRequest = 1;
        public static readonly byte RIPv2CommandResponse = 2;

        private byte[] raw;
        private byte command;
        private RIPv2EntryTable entries;

        public RIPv2Packet(Packet p) : base(p)
        {
            if (!base.IsRIPv2()) throw new Exception();
            raw = p.Ethernet.IpV4.Udp.Payload.ToArray();
            command = raw[0];
            List<byte> l = new List<byte>(20);
            for (int i = 0; i < raw.Length; i++)
            {
                if (i < 4) continue;
                else
                    l.Add(raw[i]);
            }
            entries = new RIPv2EntryTable(l.ToArray());
        }

        public byte[] Raw { get => raw; set => raw = value; }
        public byte Command { get => command; set => command = value; }
        internal RIPv2EntryTable Entries { get => entries; set => entries = value; }

        protected static byte[] RIPv2Request()
        {
            byte[] arr = new byte[24];
            arr[0] = 1;
            arr[1] = 2;
            arr[23] = 16;
            return arr;
        }

        protected static byte[] RIPv2ResponseHeader()
        {
            byte[] arr = new byte[4];
            arr[0] = 2;
            arr[1] = 2;
            return arr;
        }

        public static Packet RIPv2ResponsePacketBuilder(RouterPort senderRp, List<byte[]> entries, MacAddress dstMac, IpV4Address dstIp)
        {

            

            EthernetLayer ethernetLayer = new EthernetLayer
            {
                Source = senderRp.Mac,
                Destination = dstMac
            };

            IpV4Layer ipV4Layer =
                new IpV4Layer
                {
                    Source = senderRp.Ip,
                    CurrentDestination = dstIp,
                    Fragmentation = IpV4Fragmentation.None,
                    HeaderChecksum = null,
                    Identification = 0,
                    Options = IpV4Options.None,
                    Protocol = null,
                    Ttl = 2,
                    TypeOfService = 0,
                };

            UdpLayer udpLayer =
                new UdpLayer
                {
                    SourcePort = 520,
                    DestinationPort = 520,
                    Checksum = null,
                    CalculateChecksumValue = true,
                };

            List<byte> r = new List<byte>(24);
            r.AddRange(RIPv2ResponseHeader());
            foreach (var entry in entries)
            {
                r.AddRange(entry);
            }

            PayloadLayer payloadLayer =
                new PayloadLayer
                {
                    Data = new Datagram(r.ToArray()),
                };

            PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, payloadLayer);

            return builder.Build(DateTime.Now);
        }

        public static Packet RIPv2ResponsePacketBuilder(RouterPort senderRp, List<byte[]>entries)
        {
            EthernetLayer ethernetLayer = new EthernetLayer
            {
                Source = senderRp.Mac,
                Destination = new MacAddress("01:00:5E:00:00:09")
            };

            IpV4Layer ipV4Layer =
                new IpV4Layer
                {
                    Source = senderRp.Ip,
                    CurrentDestination = new IpV4Address("224.0.0.9"),
                    Fragmentation = IpV4Fragmentation.None,
                    HeaderChecksum = null,
                    Identification = 0,
                    Options = IpV4Options.None,
                    Protocol = null,
                    Ttl = 2,
                    TypeOfService = 0,
                };

            UdpLayer udpLayer =
                new UdpLayer
                {
                    SourcePort = 520,
                    DestinationPort = 520,
                    Checksum = null,
                    CalculateChecksumValue = true,
                };

            List<byte> r = new List<byte>(24);
            r.AddRange(RIPv2ResponseHeader()); 
            foreach (var entry in entries)
            {
                r.AddRange(entry);
            }

            PayloadLayer payloadLayer =
                new PayloadLayer
                {
                    Data = new Datagram(r.ToArray()),
                };

            PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, payloadLayer);

            return builder.Build(DateTime.Now);
        }

        public static Packet RIPv2RequestPacketBuilder(RouterPort senderRp)
        {
            EthernetLayer ethernetLayer = new EthernetLayer
            {
                Source = senderRp.Mac,
                Destination = new MacAddress("01:00:5E:00:00:09")
            };


            IpV4Layer ipV4Layer =
                new IpV4Layer
                {
                    Source = senderRp.Ip,
                    CurrentDestination = new IpV4Address("224.0.0.9"),
                    Fragmentation = IpV4Fragmentation.None,
                    HeaderChecksum = null, 
                    Identification = 0,
                    Options = IpV4Options.None,
                    Protocol = null,
                    Ttl = 2,
                    TypeOfService = 0,
                };

            UdpLayer udpLayer =
                new UdpLayer
                {
                    SourcePort = 520,
                    DestinationPort = 520,
                    Checksum = null, 
                    CalculateChecksumValue = true,
                };

            PayloadLayer payloadLayer =
                new PayloadLayer
                {
                    Data = new Datagram(RIPv2Request()),
                };

            PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, payloadLayer);

            return builder.Build(DateTime.Now);
        }
    }
}
