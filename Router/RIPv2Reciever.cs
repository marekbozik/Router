using PcapDotNet.Core;
using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Router
{
    class RIPv2Reciever
    {
        private bool recieving;
        private RouterPort rp;
        private Router router;
        private int port;

        public RIPv2Reciever(RouterPort rp, Router router, int port)
        {
            this.rp = rp;
            recieving = false;
            this.router = router;
            this.port = port;
            //StartRecieving();
        }

        public void StartRecieving()
        {

            using (PacketCommunicator communicator =
                rp.DeviceInterface.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000))
            {
                communicator.ReceivePackets(0, Handler);
            }
            //new Thread(() =>
            //{

            //    while (true)
            //    {
            //        if (recieving)
            //        {

            //        }

            //        Thread.Sleep(250);
            //    }
            //}).Start();
        }

        private void Handler(Packet p)
        {
            if (p.Ethernet.Source == router.Port1.Mac || p.Ethernet.Source == router.Port2.Mac) return;

            if (IpV4Packet.IsIpV4Packet(p))
            {
                IpV4Packet ipp = new IpV4Packet(p);

                if (ipp.IsRIPv2())
                {
                    RIPv2Packet rip;
                    rip = new RIPv2Packet(ipp.Packet);
                    foreach (var entry in rip.Entries.Table)
                    {
                        var rl = new RoutingLog(RoutingLog.typeRIPv2, entry.Ip, entry.Mask, port);
                        if (!router.RoutingTable.Contains(rl.GetHashCode()))
                        {
                            router.RoutingTable.Add(rl);
                        }

                    }
                }
            }
        }
    }
}
