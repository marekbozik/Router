using PcapDotNet.Core;
using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class Sniffer
    {
        private PacketDevice deviceInterface;
        private Stats stats;

        public Sniffer(PacketDevice deviceInterface, Stats stats)
        {
            this.deviceInterface = deviceInterface;
            this.stats = stats;
        }

        public void SniffingIn()
        {
            using (PacketCommunicator communicator =
                    deviceInterface.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000))
            {
                communicator.ReceivePackets(0, Handler);
            }
        }

        public void SniffingOut()
        {
            using (PacketCommunicator communicator =
                    deviceInterface.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000))
            {
                communicator.ReceivePackets(0, Handler2);
            }
        }

        private void Handler(Packet p)
        {
            stats.IncrementIn(p);
        }

        private void Handler2(Packet p)
        {
            stats.IncrementOut(p);
        }

    }
}
