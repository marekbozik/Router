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
            recieving = false;
        }

        public bool Recieving { get => recieving; set => recieving = value; }

        public void StartRecieving()
        {

            using (PacketCommunicator communicator =
                rp.DeviceInterface.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000))
            {
                communicator.ReceivePackets(0, Handler);
            }

        }

        private void Handler(Packet p)
        {
            if (!recieving) return;

            if (p.Ethernet.Source == router.Port1.Mac || p.Ethernet.Source == router.Port2.Mac) return;

            if (IpV4Packet.IsIpV4Packet(p))
            {
                IpV4Packet ipp = new IpV4Packet(p);

                if (ipp.IsRIPv2())
                {
                    RIPv2Packet rip;
                    rip = new RIPv2Packet(ipp.Packet);
                    if (rip.Command == RIPv2Packet.RIPv2CommandResponse)
                    {
                        foreach (var entry in rip.Entries.Table)
                        {
                            //var rl = new RoutingLog(RoutingLog.typeRIPv2, entry.Ip, entry.Mask, port);
                            var rl = new RIPv2RoutingLog(rip, entry);
                            if (!router.RoutingTable.Contains(rl))
                            {
                                router.RoutingTable.Add(rl);
                            }
                            else
                            {
                                var l = router.RoutingTable.GetLog(entry.Ip, entry.Mask);
                                if (l.Type == RoutingLog.typeRIPv2)
                                {
                                    var r = (RIPv2RoutingLog)l;
                                    if ((r.Metric + 1) < r.Metric && !r.IsInvalid)
                                    {
                                        router.RoutingTable.Remove(l);
                                        router.RoutingTable.Add(rl);
                                    }

                                }
                            }

                        }
                    }
                }
            }
        }
    }
}
