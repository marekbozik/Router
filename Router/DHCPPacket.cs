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
    class DHCPPacket : IpV4Packet
    {
        protected byte[] raw;
        protected byte[] transactionID;
        protected byte[] rawOptions;
        protected byte messageType;

        public static readonly byte MessageTypeDiscover = 1;

        public byte MessageType { get => messageType; set => messageType = value; }
        public byte[] TransactionID { get => transactionID; set => transactionID = value; }

        public DHCPPacket(Packet p) : base(p)
        {
            raw = p.Ethernet.IpV4.Udp.Payload.ToArray();
            TransactionID = new byte[4];
            for (int i = 0, j = 4; i < 4; i++, j++)
                TransactionID[i] = raw[j];
            List<byte> opts = new List<byte>(Math.Abs(raw.Length - 240));
            for (int i = 240; i < raw.Length; i++)
                opts.Add(raw[i]);
            rawOptions = opts.ToArray();
            FillOptions();
        }

        public static Packet DHCPOfferPacketBuilder(RouterPort rp, MacAddress dstMac, byte[] transactionID, IpV4Address newIp, string newSubnetMask)
        {
            EthernetLayer ethernetLayer = new EthernetLayer
            {
                Source = rp.Mac,
                Destination = new MacAddress("FF:FF:FF:FF:FF:FF")
            };

            IpV4Layer ipV4Layer =
                new IpV4Layer
                {
                    Source = rp.Ip,
                    CurrentDestination = new IpV4Address("255.255.255.255"),
                    Fragmentation = IpV4Fragmentation.None,
                    HeaderChecksum = null,
                    Identification = 0,
                    Options = IpV4Options.None,
                    Protocol = null,
                    Ttl = 255,
                    TypeOfService = 0,
                };

            UdpLayer udpLayer =
                new UdpLayer
                {
                    SourcePort = 67,
                    DestinationPort = 68,
                    Checksum = null,
                    CalculateChecksumValue = true,
                };

            List<byte> dhcp = new List<byte>(240);
            dhcp.Add(2);
            dhcp.Add(1);
            dhcp.Add(6);
            dhcp.Add(0);
            foreach (var semiID in transactionID)
                dhcp.Add(semiID);
            dhcp.Add(0);
            dhcp.Add(0);
            dhcp.Add(128);
            dhcp.Add(0);

            for (int i = 0; i < 4; i++)
                dhcp.Add(0);

            var ipArr = newIp.ToString().Split('.');
            for (int i = 0; i < 4; i++)
            {
                dhcp.Add(Byte.Parse(ipArr[i]));
            }
            for (int i = 0; i < 8; i++)
                dhcp.Add(0);

            var dstMacArr = dstMac.ToString().Split(':');
            for (int i = 0; i < 6; i++)
            {
                dhcp.Add(Byte.Parse(dstMacArr[i], System.Globalization.NumberStyles.HexNumber));
            }

            for (int i = 0; i < 202; i++)
                dhcp.Add(0);

            dhcp.Add(99);
            dhcp.Add(130);
            dhcp.Add(83);
            dhcp.Add(99);

            //options
            //offer
            dhcp.Add(53);
            dhcp.Add(1);
            dhcp.Add(2);
            //server ip
            dhcp.Add(54);
            dhcp.Add(4);
            var servIpArr = rp.Ip.ToString().Split('.');
            for (int i = 0; i < 4; i++)
            {
                dhcp.Add(Byte.Parse(servIpArr[i]));
            }
            //subnet mask
            dhcp.Add(1);
            dhcp.Add(4);
            var submaskArr = newSubnetMask.Split('.');
            for (int i = 0; i < 4; i++)
            {
                dhcp.Add(Byte.Parse(submaskArr[i]));
            }
            //end
            dhcp.Add(255);


            PayloadLayer payloadLayer =
                new PayloadLayer
                {
                    Data = new Datagram(dhcp.ToArray()),
                };

            PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, payloadLayer);

            return builder.Build(DateTime.Now);
        }

        private void FillOptions()
        {

            int i = 0;
            int offset = 0;
            byte option = 0;
            byte value = 0;
            while (rawOptions[i] != 255)
            {
                option = rawOptions[i];
                i++;
                offset = rawOptions[i];
                value = rawOptions[i + 1];

                if (option == 53) MessageType = value;

                for (int j = 0; j <= offset; j++)
                    i++;
            }
        }

    }
}
