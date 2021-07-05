using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class GenericPacket
    {
        protected Packet packet;
        protected MacAddress sourceMacAddress;
        protected MacAddress destinationMacAddress;

        public GenericPacket(Packet p)
        {
            packet = p;
            sourceMacAddress = p.Ethernet.Source;
            destinationMacAddress = p.Ethernet.Destination;
        }

        public MacAddress SourceMacAddress { get => sourceMacAddress; set => sourceMacAddress = value; }
        public MacAddress DestinationMacAddress { get => destinationMacAddress; set => destinationMacAddress = value; }
    }
}
