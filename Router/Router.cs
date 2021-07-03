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

namespace Router
{
    class Router
    {
        private RouterPort port1;
        private RouterPort port2;
        private static IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
        private MacTable macTable;
        
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

                port1 = new RouterPort(pd, ip, mask);
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

                port2 = new RouterPort(pd, ip, mask);
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

        private void Initialize()
        {
            macTable = new MacTable();
        }

        public void Forward(RouterPort rp)
        {
            if (rp == port1 || rp == port2)
            {
                if (rp.Forwarding) return;
                else rp.Forwarding = true;
                
                using (PacketCommunicator communicator =
                rp.DeviceInterface1.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000))
                {
                    communicator.ReceivePackets(0, ForwardHandler);
                }
            }
        }

        private void ForwardHandler(Packet p)
        {
            if (Arp.IsArp(p))
            {
                p.Ethernet.Arp.
            }

        }

        public void Serialize()
        {
            new Thread(() =>
            {
                string[] lines =
                {
                    port1.DeviceInterface1.Name.ToString(), port1.IpAddress1.ToString(), port1.Mask1.ToString()
                };

                File.WriteAllLines("port1.txt", lines);
            }).Start();

            new Thread(() =>
            {
                string[] lines =
                {
                    port2.DeviceInterface1.Name.ToString(), port2.IpAddress1.ToString(), port2.Mask1.ToString()
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
