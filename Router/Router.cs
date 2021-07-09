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

namespace Router
{
    class Router
    {
        private RouterPort port1;
        private RouterPort port2;
        private PacketCommunicator sender1, sender2;
        private static IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
        private ArpTable arpTable;

        private Stats out1;
        private Stats out2;
        
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

        public RouterPort Port1 { get => port1; set => port1 = value; }
        public RouterPort Port2 { get => port2; set => port2 = value; }
        internal ArpTable ArpTable { get => arpTable; }
        internal Stats Out1 { get => out1; }
        internal Stats Out2 { get => out2; }

        private void Initialize()
        {
            arpTable = new ArpTable(20, this);
            sender1 = port1.DeviceInterface.Open(65536, PacketDeviceOpenAttributes.Promiscuous | PacketDeviceOpenAttributes.NoCaptureLocal, 1000);
            sender2 = port2.DeviceInterface.Open(65536, PacketDeviceOpenAttributes.Promiscuous | PacketDeviceOpenAttributes.NoCaptureLocal, 1000);
            out1 = new Stats();
            out2 = new Stats();
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


        private void ForwardHandler1(Packet p)
        {
            if (IpV4Packet.IsIpV4Packet(p))
            {
                //IpV4Packet ip = new IpV4Packet(p);
                
                if (ArpPacket.IsArp(p)) { ArpHandle(p, 1); }

            }
        }

        private void ForwardHandler2(Packet p)
        {
            if (IpV4Packet.IsIpV4Packet(p))
            {
                //IpV4Packet ip = new IpV4Packet(p);

                if (ArpPacket.IsArp(p)) { ArpHandle(p, 2); }
            }
        }

        private void ArpHandle(Packet p, int port)
        {
            if (port == 1)
            {
                ArpPacket arp = new ArpPacket(p);
                if (arp.IsRequest())
                {
                    Packet send = MakeArpReply(arp, 1);
                    try { sender1.SendPacket(send); }
                    catch (Exception) { }

                    out1.Increment(send);
                }
                else if (arp.IsReply())
                {
                    ArpReply(arp, new IpV4Packet(p));
                }
            }
            else if (port == 2)
            {
                ArpPacket arp = new ArpPacket(p);
                if (arp.IsRequest())
                {
                    Packet send = MakeArpReply(arp, 2);
                    try { sender2.SendPacket(send); }
                    catch (Exception) { }

                    out2.Increment(send);
                }
                else if (arp.IsReply())
                {
                    ArpReply(arp, new IpV4Packet(p));
                }
            }

        }

        private void ArpReply(ArpPacket arp, IpV4Packet ip)
        {
            if (arpTable.IsExpectedReply(ip.SrcIp, 1))
            {
                arpTable.Add(arp, 1);
                new Thread(() => {
                    Packet pac = ArpPacket.ArpPacketBuilder(ArpOperation.Reply,
                                                            port2.Mac,
                                                            arpTable.GetRegistredArp(arp.SourceIp).SrcMac,
                                                            arp.SourceIp,
                                                            arpTable.GetRegistredArp(arp.SourceIp).SrcIp
                                                           );
                    arpTable.RegisterArpReply(ip.SrcIp);
                    sender2.SendPacket(pac);
                    out2.Increment(pac);
                }).Start();
            }
            if (arpTable.IsExpectedReply(ip.SrcIp, 2))
            {
                arpTable.Add(arp, 2);
                new Thread(() => {
                    Packet pac = ArpPacket.ArpPacketBuilder(ArpOperation.Reply,
                                                            port1.Mac,
                                                            arpTable.GetRegistredArp(arp.SourceIp).SrcMac,
                                                            arp.SourceIp,
                                                            arpTable.GetRegistredArp(arp.SourceIp).SrcIp
                                                           );
                    arpTable.RegisterArpReply(ip.SrcIp);
                    sender1.SendPacket(pac);
                    out1.Increment(pac);
                }).Start();
            }
        }

        private Packet ArpRouterReply(ArpPacket req, int incomePort)
        {
            RouterPort rp;
            if (incomePort == 1)
                rp = Port1;
            else
                rp = Port2;

            return ArpPacket.ArpPacketBuilder(ArpOperation.Reply,
                                     rp.Mac,
                                     req.SourceMacAddress,
                                     req.DestinationIp,
                                     req.SourceIp
                                    );
        }

        private Packet ArpTableReply(ArpPacket req, ArpLog l, int incomePort)
        {
            RouterPort rp;
            if (incomePort == 1)
                rp = Port1;
            else
                rp = Port2;
            return ArpPacket.ArpPacketBuilder(ArpOperation.Reply,
                                     rp.Mac,
                                     req.SourceMacAddress,
                                     req.DestinationIp,
                                     req.SourceIp
                                    );
        }

        private Packet MakeArpReply(ArpPacket req, int incomePort)
        {
            if (!req.IsRequest()) throw new Exception();

            if (req.DestinationIp == Port1.Ip || req.DestinationIp == Port2.Ip)
                return ArpRouterReply(req, incomePort);

            ArpLog l;
            if (arpTable.Contains(req.DestinationIp))
            {
                l = arpTable.GetLog(req.DestinationIp);
                if (l.Port != incomePort)
                {
                    return ArpTableReply(req, l, incomePort);
                }
            }
            else
            {
                if (incomePort == 1)
                {
                    if (IpV4.IsInSubnet(port2.Ip, port2.Mask, req.DestinationIp))
                    {
                        new Thread(() => {
                            Packet pac = ArpPacket.ArpPacketBuilder(ArpOperation.Request,
                                                                    port2.Mac,
                                                                    new MacAddress("FF:FF:FF:FF:FF:FF"),
                                                                    port2.Ip,
                                                                    req.DestinationIp
                                                                   );
                            arpTable.RegisterArpRequest(req.DestinationIp, req.SourceIp, req.SourceMacAddress, 2);
                            sender2.SendPacket(pac);
                            out2.Increment(pac);
                        }).Start();
                    }
                }
                else if (incomePort == 2)
                {
                    if (IpV4.IsInSubnet(port1.Ip, port1.Mask, req.DestinationIp))
                    {
                        new Thread(() => {
                            Packet pac = ArpPacket.ArpPacketBuilder(ArpOperation.Request,
                                                                    port1.Mac,
                                                                    new MacAddress("FF:FF:FF:FF:FF:FF"),
                                                                    port1.Ip,
                                                                    req.DestinationIp
                                                                   );
                            arpTable.RegisterArpRequest(req.DestinationIp, req.SourceIp, req.SourceMacAddress, 1);
                            sender1.SendPacket(pac);
                            out1.Increment(pac);
                        }).Start();
                    }
                }
            }


            return null;
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
