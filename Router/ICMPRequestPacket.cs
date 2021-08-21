using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    struct ICMPRequestPacket
    {
        public Packet requestPacket;
        public byte[] data;
        public ushort sequenceNumber;
        public ushort identifier;
    }
}
