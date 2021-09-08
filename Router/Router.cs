using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Core;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Threading;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets;
using System.Collections.Concurrent;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Icmp;

namespace Router
{
    class Router
    {
        private RouterPort port1;
        private RouterPort port2;
        private static IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
        private ArpTable arpTable;
        private RoutingTable routingTable;
        private ConcurrentStack<ICMPPacket> pingStack;

        public Router()
        {
            try
            {
                string line;
                System.IO.StreamReader file = new System.IO.StreamReader("port1.txt");
                List<string> l = new List<string>();
                while ((line = file.ReadLine()) != null)
                {
                    l.Add(line);
                }
                file.Close();

                var devs = Router.GetPacketDevices();
                PacketDevice pd = null;
                foreach (var item in devs)
                {
                    if (item.Name == l[0])
                    {
                        pd = item;
                    }

                }

                if (pd == null)
                    throw new SerializeException();

                IpV4Address ip = new IpV4Address(l[1]);
                string mask = l[2];

                port1 = new RouterPort(pd, ip, mask, new MacAddress("00:00:00:00:01:01"));
            }
            catch (Exception)
            {
                throw new SerializeException();
            }

            try
            {
                string line;
                System.IO.StreamReader file = new System.IO.StreamReader("port2.txt");
                List<string> l = new List<string>();
                while ((line = file.ReadLine()) != null)
                {
                    l.Add(line);
                }
                file.Close();

                var devs = Router.GetPacketDevices();
                PacketDevice pd = null;
                foreach (var item in devs)
                {
                    if (item.Name == l[0])
                    {
                        pd = item;
                    }

                }

                if (pd == null)
                    throw new SerializeException();

                IpV4Address ip = new IpV4Address(l[1]);
                string mask = l[2];

                port2 = new RouterPort(pd, ip, mask, new MacAddress("00:00:00:00:02:02"));
            }
            catch (Exception)
            {
                throw new SerializeException();
            }

            Initialize();
        }

        public Router(RouterPort rp1, RouterPort rp2)
        {
            port1 = rp1;
            port2 = rp2;
            Initialize();
        }

        public RouterPort Port1 { get => port1; set { port1 = value; arpTable.UpdatePortsIp(this); } }
        public RouterPort Port2 { get => port2; set { port2 = value; arpTable.UpdatePortsIp(this); } }
        internal ArpTable ArpTable { get => arpTable; }

        internal RoutingTable RoutingTable { get => routingTable; }


        private void Initialize()
        {
            arpTable = new ArpTable(20, this);
            port1.Sender = port1.DeviceInterface.Open(65536, PacketDeviceOpenAttributes.Promiscuous | PacketDeviceOpenAttributes.NoCaptureLocal, 1000);
            port2.Sender = port2.DeviceInterface.Open(65536, PacketDeviceOpenAttributes.Promiscuous | PacketDeviceOpenAttributes.NoCaptureLocal, 1000);
            routingTable = new RoutingTable(this);
            pingStack = new ConcurrentStack<ICMPPacket>();
        }



        public void Forward(RouterPort rp)
        {
            if (rp == port1 || rp == port2)
            {
                if (rp.Forwarding) return;
                else rp.Forwarding = true;

                if (rp == port1)
                {
                    using (PacketCommunicator communicator =
                    rp.DeviceInterface.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000))
                    {
                        communicator.ReceivePackets(0, ForwardHandler1);
                    }
                }
                else
                {
                    using (PacketCommunicator communicator =
                    rp.DeviceInterface.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000))
                    {
                        communicator.ReceivePackets(0, ForwardHandler2);
                    }
                }
            }
        }

        private void Forwarder(Packet p, RouterPort port, RouterPort mirrorPort)
        {
            if (p.Ethernet.Source == port1.Mac || p.Ethernet.Source == port2.Mac)
                return;
            new Thread(() => {
                if (IpV4Packet.IsIpV4Packet(p))
                {
                    IpV4Packet ipp = new IpV4Packet(p);

                    if (ArpPacket.IsArp(ipp.Packet)) 
                    { 
                        ArpHandle(ipp.Packet, port); 
                        return; 
                    }
                    else if (ipp.IsIcmp() && (ipp.DstIp == port1.Ip || ipp.DstIp == port2.Ip))
                        PingHandler(ipp, port);
                    else if (ipp.DstIp == port.Ip)
                        return;
                    else if (IpV4.IsInSubnet(mirrorPort.Ip, mirrorPort.Mask, ipp.DstIp))
                        ForwardTo(ipp, mirrorPort);
                    else if (arpTable.Contains(ipp.DstIp))
                    {
                        ForwardTo(ipp, arpTable.GetPort(ipp.DstIp) == 1 ? port1 : port2);
                    }
                    else
                    {
                        int pr = 0;
                        try
                        {
                            pr = routingTable.GetOutInt(ipp.DstIp);
                        }
                        catch (Exception) { return; }
                        ForwardTo(ipp, pr == 1 ? port1 : port2);
                    }
                }
            
            
            }).Start();

        }

        private void ForwardHandler1(Packet p)
        {
            Forwarder(p, port1, port2);
        }
        private void ForwardHandler2(Packet p)
        {
            Forwarder(p, port2, port1);
        }

        public void Ping(IpV4Address dstIp, ProgressBar bar, TextBox textBox, Label summaryLabel, Button pingButton)
        {
            pingStack.Clear();
            int port = 0;
            try
            {
                port = routingTable.GetOutInt(dstIp);
            }
            catch (Exception)
            {
                pingButton.BeginInvoke(new Action(() => pingButton.Enabled = true));
                return;
            }
            bar.Invoke(new Action(() => bar.Maximum = 5));
            summaryLabel.Invoke(new Action(() => summaryLabel.Text = "0/0"));
            textBox.Invoke(new Action(() => textBox.Text = ""));

            RouterPort rp = null;
            PacketCommunicator sender = null;
            if (port == 1)
            {
                rp = port1;
                sender = port1.Sender;
            }
            else if (port == 2)
            {
                rp = port2;
                sender = port2.Sender;
            }

            int replies = 0;
            for (int i = 0; i < 5; i++)
            {
                bool found = false;
                if (!arpTable.Contains(dstIp))
                {
                    bool end = false;
                    IpV4Address nxtH = new IpV4Address();
                    try
                    {
                        nxtH = routingTable.GetNextHop(dstIp);
                    }
                    catch (Exception)
                    {
                        end = true;
                        sender.SendPacket(ArpPacket.ArpPacketBuilder(ArpOperation.Request, rp.Mac, new MacAddress("FF:FF:FF:FF:FF:FF"), rp.Ip, dstIp));
                        Thread.Sleep(2000);
                    }
                    if (!end)
                    {
                        if (arpTable.Contains(nxtH))
                        {
                            var log = arpTable.GetLog(nxtH);
                            var x = ICMPPacket.ICMPRequestPacketBuilder(rp.Mac, rp.Ip, log.Mac, dstIp, (ushort)i);
                            sender.SendPacket(x.requestPacket);

                            //2s timeout

                            for (int j = 0; j < 40; j++)
                            {
                                ICMPPacket icmpP;
                                while (pingStack.TryPop(out icmpP))
                                {
                                    if (icmpP.Id == x.identifier && icmpP.Seq == x.sequenceNumber && icmpP.Data.Length == x.data.Length)
                                    {
                                        bool br = false;
                                        for (int k = 0; k < icmpP.Data.Length; k++)
                                        {
                                            if (icmpP.Data[k] != x.data[k])
                                            {
                                                br = true;
                                                break;
                                            }
                                        }
                                        if (!br)
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                                if (found)
                                {
                                    replies++;
                                    break;
                                }
                                Thread.Sleep(50);
                            }
                        }
                        else
                        {
                            sender.SendPacket(ArpPacket.ArpPacketBuilder(ArpOperation.Request, rp.Mac, new MacAddress("FF:FF:FF:FF:FF:FF"), rp.Ip, nxtH));
                            Thread.Sleep(2000);
                        }
                    }
                    
                }
                else
                {
                    var log = arpTable.GetLog(dstIp);
                    var x = ICMPPacket.ICMPRequestPacketBuilder(rp.Mac, rp.Ip, log.Mac, dstIp, (ushort)i);
                    sender.SendPacket(x.requestPacket);

                    //2s timeout
                    
                    for (int j = 0; j < 40; j++)
                    {
                        ICMPPacket icmpP;
                        while (pingStack.TryPop(out icmpP))
                        {
                            if (icmpP.Id == x.identifier && icmpP.Seq == x.sequenceNumber && icmpP.Data.Length == x.data.Length)
                            {
                                bool br = false;
                                for (int k = 0; k < icmpP.Data.Length; k++)
                                {
                                    if (icmpP.Data[k] != x.data[k])
                                    {
                                        br = true;
                                        break;
                                    }
                                }
                                if (!br)
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (found)
                        {
                            replies++;
                            break;
                        }
                        Thread.Sleep(50);
                    }
                }

                bar.Invoke(new Action(() => bar.Value = i + 1));
                summaryLabel.Invoke(new Action(() => summaryLabel.Text = replies + "/" + (i + 1) ));
                if (found)
                {
                    textBox.Invoke(new Action(() => textBox.AppendText("!")));
                }
                else
                {
                    textBox.Invoke(new Action(() => textBox.AppendText(".")));
                }

            }
            ICMPPacket.NewPingSequence();
            pingButton.BeginInvoke(new Action(() => pingButton.Enabled = true));
            Thread.Sleep(10000);
            bar.Invoke(new Action(() =>
            {
                if (bar.Value != 5) return;
                else bar.Value = 0;
            }));
        }

        private void PingRepliesHandler(ICMPPacket icmp)
        {
            pingStack.Push(icmp);
        }

        private void PingAutoReply(IpV4Packet ipp, int port)
        {
            if (port == 1 && ipp.DstIp == port1.Ip)
                port1.Sender.SendPacket(ICMPPacket.ICMPReplyPacketBuilder(new ICMPPacket(ipp.Packet), port1.Mac, port1.Ip));
            else if (port == 1 && ipp.DstIp == port2.Ip)
                port1.Sender.SendPacket(ICMPPacket.ICMPReplyPacketBuilder(new ICMPPacket(ipp.Packet), port1.Mac, port2.Ip));
            else if (port == 2 && ipp.DstIp == port1.Ip)
                port2.Sender.SendPacket(ICMPPacket.ICMPReplyPacketBuilder(new ICMPPacket(ipp.Packet), port2.Mac, port1.Ip));
            else if (port == 2 && ipp.DstIp == port2.Ip)
                port2.Sender.SendPacket(ICMPPacket.ICMPReplyPacketBuilder(new ICMPPacket(ipp.Packet), port2.Mac, port2.Ip));
        }



        private void PingHandler(IpV4Packet ipp, RouterPort port)
        {
            if (new ICMPPacket(ipp.Packet).IsRequest())
            {
                if (!arpTable.Contains(ipp.SrcIp))
                {
                    new Thread(() =>
                    {
                        port.Sender.SendPacket(ArpPacket.ArpPacketBuilder(ArpOperation.Request, port.Mac, new MacAddress("FF:FF:FF:FF:FF:FF"), port.Ip, ipp.SrcIp));

                        //Try to get arp response in max 1s 
                        for (int i = 0; i < 20; i++)
                        {
                            if (arpTable.Contains(ipp.SrcIp))
                            {
                                int pport = port.Mac == port1.Mac ? 1 : 2;
                                PingAutoReply(ipp, pport);
                                return;
                            }
                            Thread.Sleep(50);
                        }
                    }).Start();
                }
                else
                {
                    int pport = port.Mac == port1.Mac ? 1 : 2;
                    PingAutoReply(ipp, pport);
                }
            }
            else if (new ICMPPacket(ipp.Packet).IsReply())
            {
                PingRepliesHandler(new ICMPPacket(ipp.Packet));
            }

        }

        private void ForwardTo(IpV4Packet ipp, RouterPort port)
        {
            if (arpTable.Contains(ipp.DstIp))
            {
                ContainsForward(ipp, port);
            }
            else
            {
                IpV4Address nextHop = new IpV4Address();
                try
                {
                    nextHop = routingTable.GetNextHop(ipp.DstIp);
                }
                catch (Exception)
                {
                    port.Sender.SendPacket(ArpPacket.ArpPacketBuilder(ArpOperation.Request, port.Mac, new MacAddress("FF:FF:FF:FF:FF:FF"), port.Ip, ipp.DstIp));

                    new Thread(() =>
                    {
                        //Try to get arp response in max 1s 
                        for (int i = 0; i < 20; i++)
                        {
                            if (arpTable.Contains(ipp.DstIp))
                            {
                                ContainsForward(ipp, port);
                                return;
                            }
                            Thread.Sleep(50);
                        }
                    }).Start();
                }
                if (nextHop == IpV4.ToNetworkAddress(port1.Ip, port1.Mask) || nextHop == IpV4.ToNetworkAddress(port2.Ip, port2.Mask))
                {
                    port.Sender.SendPacket(ArpPacket.ArpPacketBuilder(ArpOperation.Request, port.Mac, new MacAddress("FF:FF:FF:FF:FF:FF"), port.Ip, ipp.DstIp));

                    new Thread(() =>
                    {
                        //Try to get arp response in max 1s 
                        for (int i = 0; i < 20; i++)
                        {
                            if (arpTable.Contains(ipp.DstIp))
                            {
                                ContainsForward(ipp, port);
                                return;
                            }
                            Thread.Sleep(50);
                        }
                    }).Start();
                }
                else
                {
                    if (arpTable.Contains(nextHop))
                    {
                        ContainsForward(ipp, port, nextHop);
                    }
                    else
                    {
                        int p = routingTable.GetOutInt(nextHop);
                        if (p == 1)
                            port1.Sender.SendPacket(ArpPacket.ArpPacketBuilder(ArpOperation.Request, port1.Mac, new MacAddress("FF:FF:FF:FF:FF:FF"), port1.Ip, nextHop));
                        else
                            port2.Sender.SendPacket(ArpPacket.ArpPacketBuilder(ArpOperation.Request, port2.Mac, new MacAddress("FF:FF:FF:FF:FF:FF"), port2.Ip, nextHop));

                        new Thread(() =>
                        {
                            //Try to get arp response in max 1s 
                            for (int i = 0; i < 20; i++)
                            {
                                if (arpTable.Contains(nextHop))
                                {
                                    ContainsForward(ipp, port, nextHop);
                                    return;
                                }
                                Thread.Sleep(50);
                            }
                        }).Start();
                    }
                }


            }


        }

        private void ContainsForward(IpV4Packet ipp, RouterPort port, IpV4Address nextHop)
        {
            EthernetLayer ethernetLayer = new EthernetLayer
            {
                Source = port.Mac,
                Destination = new MacAddress(arpTable.GetLog(nextHop).Mac.ToString()),
                EtherType = EthernetType.IpV4
            };

            PacketBuilder pb = new PacketBuilder(ethernetLayer);
            Packet pE = pb.Build(DateTime.Now);
            Packet pI = ipp.Packet;

            var eBytes = pE.Buffer;
            var iBytes = pI.Buffer;

            for (int i = 0; i < eBytes.Length; i++)
                iBytes[i] = eBytes[i];

            string hexS = BitConverter.ToString(iBytes).Replace("-", string.Empty);

            Packet p = Packet.FromHexadecimalString(hexS, DateTime.Now, DataLinkKind.Ethernet);

            port.Sender.SendPacket(p);
        }

        private void ContainsForward(IpV4Packet ipp, RouterPort port)
        {
            EthernetLayer ethernetLayer = new EthernetLayer
            {
                Source = port.Mac,
                Destination = new MacAddress(arpTable.GetLog(ipp.DstIp).Mac.ToString()),
                EtherType = EthernetType.IpV4
            };

            PacketBuilder pb = new PacketBuilder(ethernetLayer);
            Packet pE = pb.Build(DateTime.Now);
            Packet pI = ipp.Packet;

            var eBytes = pE.Buffer;
            var iBytes = pI.Buffer;

            for (int i = 0; i < eBytes.Length; i++)
                iBytes[i] = eBytes[i];

            string hexS = BitConverter.ToString(iBytes).Replace("-", string.Empty); 

            Packet p = Packet.FromHexadecimalString(hexS, DateTime.Now, DataLinkKind.Ethernet);

            port.Sender.SendPacket(p);
        }

        private void ArpHandle(Packet p, RouterPort port)
        {
            ArpPacket arp = new ArpPacket(p);
            if (arp.IsRequest())
            {
                Packet send = MakeArpReply(arp, port);
                try { port.Sender.SendPacket(send); }
                catch (Exception) { }
            }
            else if (arp.IsReply())
            {
                ArpReply(arp, new IpV4Packet(p));
            }

        }

        private void ArpReply(ArpPacket arp, IpV4Packet ip)
        {
            if (arp.DestinationIp == port1.Ip)
            {
                arpTable.Add(arp, 1);
                return;
            }
            else if (arp.DestinationIp == port2.Ip)
            {
                arpTable.Add(arp, 2);
                return;
            }   

            if (arpTable.IsExpectedReply(ip.SrcIp, 1))
            {
                arpTable.Add(arp, 1);
                new Thread(() =>
                {
                    Packet pac = null;
                    try
                    {
                        pac = ArpPacket.ArpPacketBuilder(ArpOperation.Reply,
                                                        port2.Mac,
                                                        arpTable.GetRegistredArp(arp.SourceIp).SrcMac,
                                                        arp.SourceIp,
                                                        arpTable.GetRegistredArp(arp.SourceIp).SrcIp
                                                       );
                        arpTable.RegisterArpReply(ip.SrcIp);
                    }
                    catch (Exception)
                    {

                        return;
                    }
                    port2.Sender.SendPacket(pac);

                }).Start();
            }
            if (arpTable.IsExpectedReply(ip.SrcIp, 2))
            {
                arpTable.Add(arp, 2);
                new Thread(() =>
                {
                    Packet pac =  null;
                    try
                    { 
                        pac = ArpPacket.ArpPacketBuilder(ArpOperation.Reply,
                                                            port1.Mac,
                                                            arpTable.GetRegistredArp(arp.SourceIp).SrcMac,
                                                            arp.SourceIp,
                                                            arpTable.GetRegistredArp(arp.SourceIp).SrcIp
                                                           );
                    
                        arpTable.RegisterArpReply(ip.SrcIp);
                    }
                    catch (Exception)
                    {

                        return;
                    }
                    port1.Sender.SendPacket(pac);
                }).Start();
            }
        }

        private Packet ArpRouterReply(ArpPacket req, RouterPort incomePort)
        {
            return ArpPacket.ArpPacketBuilder(ArpOperation.Reply,
                                     incomePort.Mac,
                                     req.SourceMacAddress,
                                     req.DestinationIp,
                                     req.SourceIp
                                    );
        }

        private Packet ArpTableReply(ArpPacket req, RouterPort port)
        {
            return ArpPacket.ArpPacketBuilder(ArpOperation.Reply,
                                     port.Mac,
                                     req.SourceMacAddress,
                                     req.DestinationIp,
                                     req.SourceIp
                                    );
        }

        private Packet MakeArpReply(ArpPacket req, RouterPort port)
        {
            if (!req.IsRequest()) throw new Exception();

            if (req.DestinationIp == Port1.Ip || req.DestinationIp == Port2.Ip)
                return ArpRouterReply(req, port);

            ArpLog l;
            if (arpTable.Contains(req.DestinationIp))
            {
                l = arpTable.GetLog(req.DestinationIp);
                if (l.Mac != port.Mac)
                {
                    return ArpTableReply(req, port);
                }
            }
            else
            {
                //bool stop = false;
                //int pp = 0;
                //try
                //{
                //    pp = routingTable.GetOutInt(req.DestinationIp);
                //}
                //catch (Exception)
                //{

                //    stop = true;
                //}

                //if (!stop)
                //{
                //    new Thread(() =>
                //    {
                //        Packet pac = ArpPacket.ArpPacketBuilder(ArpOperation.Reply,
                //                                                port.Mac,
                //                                                req.SourceMacAddress,
                //                                                req.DestinationIp,
                //                                                req.SourceIp
                //                                                );
                //        port.Sender.SendPacket(pac);

                //    }).Start();
                //}
                if (port.Mac == port1.Mac)
                {
                    if (IpV4.IsInSubnet(port2.Ip, port2.Mask, req.DestinationIp))
                    {
                        new Thread(() =>
                        {
                            Packet pac = ArpPacket.ArpPacketBuilder(ArpOperation.Request,
                                                                    port2.Mac,
                                                                    new MacAddress("FF:FF:FF:FF:FF:FF"),
                                                                    port2.Ip,
                                                                    req.DestinationIp
                                                                   );
                            arpTable.RegisterArpRequest(req.DestinationIp, req.SourceIp, req.SourceMacAddress, 2);
                            port2.Sender.SendPacket(pac);

                        }).Start();
                    }
                }
                else if (port.Mac == port2.Mac)
                {
                    if (IpV4.IsInSubnet(port1.Ip, port1.Mask, req.DestinationIp))
                    {
                        new Thread(() =>
                        {
                            Packet pac = ArpPacket.ArpPacketBuilder(ArpOperation.Request,
                                                                    port1.Mac,
                                                                    new MacAddress("FF:FF:FF:FF:FF:FF"),
                                                                    port1.Ip,
                                                                    req.DestinationIp
                                                                   );
                            arpTable.RegisterArpRequest(req.DestinationIp, req.SourceIp, req.SourceMacAddress, 1);
                            port1.Sender.SendPacket(pac);
                        }).Start();
                    }
                }
            }

            return null;
        }

        public bool TryAddStaticRoute(string ip, string mask, string outInt, string nextHop)
        {
            IpV4Address ipp, nextHopp;
            int intf = -1;
            try
            {
                ipp = new IpV4Address(ip);
                if (!IpV4.IsMask(mask)) return false;
            }
            catch (Exception)
            {

                return false;
            }

            bool b = false;
            try
            {
                intf = Int32.Parse(outInt);
            }
            catch (Exception)
            {
                b = true;
            }

            if (b)
            {
                try
                {
                    nextHopp = new IpV4Address(nextHop);
                }
                catch (Exception)
                {

                    return false;
                }
            }
            else
            {
                try
                {
                    nextHopp = new IpV4Address(nextHop);
                }
                catch (Exception)
                {
                    nextHopp = new IpV4Address();
                }

            }

            routingTable.Add(RoutingLog.typeStatic, ipp, mask, intf, nextHopp);

            return true;
            
        }

        public void Serialize()
        {
            new Thread(() =>
            {
                string[] lines =
                {
                    port1.DeviceInterface.Name.ToString(), port1.Ip.ToString(), port1.Mask.ToString()
                };

                File.WriteAllLines("port1.txt", lines);
            }).Start();

            new Thread(() =>
            {
                string[] lines =
                {
                    port2.DeviceInterface.Name.ToString(), port2.Ip.ToString(), port2.Mask.ToString()
                };

                File.WriteAllLines("port2.txt", lines);

            }).Start();
        }

        public static IList<LivePacketDevice> GetPacketDevices()
        {
            return allDevices;
        }
    }
}
