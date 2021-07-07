﻿using System;
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
            }
        }

        private void ForwardHandler1(Packet p)
        {
            GenericPacket gp = new GenericPacket(p);
            //macTable.UpdateTable(gp, 1);
            if (ArpPacket.IsArp(p))
            {
                ArpPacket arp = new ArpPacket(p);
                if (arp.IsRequest())
                {
                    Packet send = arp.MakeReply(arp, arpTable, this);
                    sender1.SendPacket(send);
                    out1.Increment(send);
                }
            }

            

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
