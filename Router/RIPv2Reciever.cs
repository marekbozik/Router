using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
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
        private RIPv2Handler RIPHandler;
        private int port;

        public RIPv2Reciever(RouterPort rp, Router router, RIPv2Handler RIPHandler)
        {
            this.rp = rp;
            recieving = false;
            this.router = router;
            this.RIPHandler = RIPHandler;
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
                        //IpV4Address[] addedIps = new IpV4Address[16];
                        //int added = 0;
                        bool triggerSend = false;
                        foreach (var entry in rip.Entries.Table)
                        {
                            var rl = new RIPv2RoutingLog(rip, entry);
                            if (!router.RoutingTable.Contains(rl) && rl.Metric < 16)
                            {
                                router.RoutingTable.Add(rl);
                                triggerSend = true;

                                //addedIps[added++] = rl.Ip;
                                //if (RIPHandler.Sender1.Rp == rp)
                                //    RIPHandler.Sender2.TriggeredSend(rip.SrcIp);
                                //else if (RIPHandler.Sender2.Rp == rp)
                                //    RIPHandler.Sender1.TriggeredSend(rip.SrcIp);
                            }
                            else
                            {
                                try
                                {
                                    var l = router.RoutingTable.GetLog(entry.Ip, entry.Mask);
                                    if (l.Type == RoutingLog.typeRIPv2)
                                    {
                                        var r = (RIPv2RoutingLog)l;
                                        if ((rl.Metric) < r.Metric && !r.IsInvalid)
                                        {
                                            router.RoutingTable.Remove(l);
                                            router.RoutingTable.Add(rl);
                                            triggerSend = true;

                                            //addedIps[added++] = rl.Ip;

                                            //if (RIPHandler.Sender1.Rp == rp)
                                            //    RIPHandler.Sender2.TriggeredSend(rip.SrcIp);
                                            //else if (RIPHandler.Sender2.Rp == rp)
                                            //    RIPHandler.Sender1.TriggeredSend(rip.SrcIp);
                                        }
                                        else if ((rl.Metric - 1) == 16)
                                        {

                                            if (RIPHandler.Sender1.Rp == rp)
                                            {
                                                if (RIPHandler.Sender2.Sending)
                                                    RIPHandler.Sender2.SendRemovedInfo(rl);
                                            }
                                            else if (RIPHandler.Sender2.Rp == rp)
                                            {
                                                if (RIPHandler.Sender1.Sending)
                                                    RIPHandler.Sender1.SendRemovedInfo(rl);
                                            }
                                            if (!r.IsOnHoldDown)
                                            {
                                                router.RoutingTable.SetPossiblyDown(rl, RIPHandler.Timers);
                                            }

                                        }
                                        else
                                        {
                                            r.RegisterUpdate();
                                            r.IsOnHoldDown = false;
                                            r.IsInvalid = false;
                                            r.IsFlushed = false;
                                            
                                        }

                                    }
                                }
                                catch (Exception) { }
                            }

                        }
                        if (triggerSend)
                        {
                            if (RIPHandler.Sender1.Rp == rp)
                                RIPHandler.Sender2.TriggeredSend();
                            else if (RIPHandler.Sender2.Rp == rp)
                                RIPHandler.Sender1.TriggeredSend();
                        }
                    }
                    else if (rip.Command == RIPv2Packet.RIPv2CommandRequest)
                    {
                        if (RIPHandler.Sender1.Rp == rp)
                            RIPHandler.Sender1.TriggeredSend(rip.SrcIp);
                        else if (RIPHandler.Sender2.Rp == rp)
                            RIPHandler.Sender2.TriggeredSend(rip.SrcIp);
                    }
                }
            }
        }
    }
}
