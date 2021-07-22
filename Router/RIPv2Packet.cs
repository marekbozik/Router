using PcapDotNet.Packets;
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
    }
}
