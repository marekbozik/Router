using PcapDotNet.Core;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Router
{
    class RIPv2Sender
    {
        private bool sending;
        private RouterPort rp;
        private PacketCommunicator sender;
        private RIPv2Handler RIPHandler;
        private int port;

        public RIPv2Sender(RouterPort rp, RIPv2Handler handler, PacketCommunicator sender, int port)
        {
            sending = false;
            this.RIPHandler = handler;
            this.rp = rp;
            this.sender = sender;
            this.port = port;
        }

        public bool Sending { get => sending; set => sending = SetSending(value); }
        internal RouterPort Rp { get => rp; }

        private bool SetSending(bool b)
        {
            if (b && !sending && RIPHandler.Process.IsInProcess(rp.Ip, rp.Mask))
            {
                sender.SendPacket(RIPv2Packet.RIPv2RequestPacketBuilder(rp));
            }
            return b;
        }

        public void SendRIPv2Request()
        {
            if (RIPHandler.Process.IsInProcess(IpV4.ToNetworkAddress(rp.Ip, rp.Mask)))
            {
                sender.SendPacket(RIPv2Packet.RIPv2RequestPacketBuilder(rp));
            }
        }
        
        private void SendRIPv2()
        {
            var tableEntriesList = RIPHandler.Router.RoutingTable.GetRIPv2LogsFor(rp.Ip, rp.Mask);
            var added = RIPHandler.Process.GetAddedNetworks();
            List<RIPv2Entry> validAdded = new List<RIPv2Entry>(added.Count);
            RIPv2EntryOrdered res;
            bool allowed = false;
            while (added.TryDequeue(out res))
            {
                if (IpV4.ToNetworkAddress(rp.Ip, rp.Mask) == res.Ip) allowed = true;
                try
                {
                    if (RIPHandler.Router.RoutingTable.GetOutInt(res.Ip) != port)
                        validAdded.Add(res);
                }
                catch (Exception) {
                    if (res.Ip == new IpV4Address("0.0.0.0"))
                    {
                        validAdded.Add(res);
                    }
                }
            }

            if (allowed)
            {
                tableEntriesList.AddRange(validAdded);

                List<byte[]> entries = new List<byte[]>(tableEntriesList.Count);
                foreach (var en in tableEntriesList)
                {
                    entries.Add(en.ToBytes());
                }
                if (entries.Count > 0)
                {
                    if (RIPHandler.Process.IsInProcess(IpV4.ToNetworkAddress(rp.Ip, rp.Mask)))
                        sender.SendPacket(RIPv2Packet.RIPv2ResponsePacketBuilder(rp, entries));

                }
            }
        }

        public void SendRemovedInfo(RIPv2RoutingLog rl)
        {
            int metric = rl.Metric;
            rl.Metric = 16;
            List<byte[]> entries = new List<byte[]>(1);
            entries.Add(new RIPv2Entry(rl).ToBytes());
            rl.Metric = metric;
            if (RIPHandler.Process.IsInProcess(IpV4.ToNetworkAddress(rp.Ip, rp.Mask)))
                sender.SendPacket(RIPv2Packet.RIPv2ResponsePacketBuilder(rp, entries));

        }
        public void SendRemovedInfo(RIPv2Entry e)
        {
            byte metric = e.Metric;
            e.Metric = 16;
            List<byte[]> entries = new List<byte[]>(1);
            entries.Add(e.ToBytes());
            e.Metric = metric;
            if (RIPHandler.Process.IsInProcess(IpV4.ToNetworkAddress(rp.Ip, rp.Mask)))
                sender.SendPacket(RIPv2Packet.RIPv2ResponsePacketBuilder(rp, entries));
        }

        public void SendAddedInfo(RIPv2Entry e)
        {
            List<byte[]> entries = new List<byte[]>(1);
            entries.Add(e.ToBytes());
            if (RIPHandler.Process.IsInProcess(IpV4.ToNetworkAddress(rp.Ip, rp.Mask)))
                sender.SendPacket(RIPv2Packet.RIPv2ResponsePacketBuilder(rp, entries));
        }

        public void TriggeredSend()
        {
            var tableEntriesList = RIPHandler.Router.RoutingTable.GetRIPv2LogsFor(rp.Ip, rp.Mask);
            var added = RIPHandler.Process.GetAddedNetworks();
            List<RIPv2Entry> validAdded = new List<RIPv2Entry>(added.Count);
            RIPv2EntryOrdered res;
            bool allowed = false;
            while (added.TryDequeue(out res))
            {
                if (IpV4.ToNetworkAddress(rp.Ip, rp.Mask) == res.Ip) allowed = true;
                if (RIPHandler.Router.RoutingTable.GetOutInt(res.Ip) != port)
                    validAdded.Add(res);
            }

            if (allowed)
            {
                tableEntriesList.AddRange(validAdded);

                List<byte[]> entries = new List<byte[]>(tableEntriesList.Count);
                foreach (var en in tableEntriesList)
                {
                    entries.Add(en.ToBytes());
                }
                if (entries.Count > 0)
                    if (RIPHandler.Process.IsInProcess(IpV4.ToNetworkAddress(rp.Ip, rp.Mask)))
                        sender.SendPacket(RIPv2Packet.RIPv2ResponsePacketBuilder(rp, entries));
            }
        }

        public void TriggeredSend(IpV4Address dstIP)
        {
            var tableEntriesList = RIPHandler.Router.RoutingTable.GetRIPv2LogsFor(rp.Ip, rp.Mask);
            var added = RIPHandler.Process.GetAddedNetworks();
            List<RIPv2Entry> validAdded = new List<RIPv2Entry>(added.Count);
            RIPv2EntryOrdered res;
            bool allowed = false;
            while (added.TryDequeue(out res))
            {
                if (IpV4.ToNetworkAddress(rp.Ip, rp.Mask) == res.Ip) allowed = true;
                if (RIPHandler.Router.RoutingTable.GetOutInt(res.Ip) != port)
                    validAdded.Add(res);
            }

            if (allowed)
            {
                tableEntriesList.AddRange(validAdded);

                List<byte[]> entries = new List<byte[]>(tableEntriesList.Count);
                foreach (var en in tableEntriesList)
                {
                    entries.Add(en.ToBytes());
                }

                MacAddress dstMac = new MacAddress();
                if (RIPHandler.Router.ArpTable.Contains(dstIP))
                    dstMac = RIPHandler.Router.ArpTable.GetLog(dstIP).Mac;
                else
                {
                    sender.SendPacket(
                        ArpPacket.ArpPacketBuilder(PcapDotNet.Packets.Arp.ArpOperation.Request,
                        rp.Mac,
                        new MacAddress("FF:FF:FF:FF:FF:FF"),
                        rp.Ip,
                        dstIP));
                    bool success = false;
                    for (int i = 0; i < 20; i++)
                    {
                        if (RIPHandler.Router.ArpTable.Contains(dstIP))
                        {
                            dstMac = RIPHandler.Router.ArpTable.GetLog(dstIP).Mac;
                            success = true;
                            break;
                        }
                        Thread.Sleep(100);
                    }
                    if (!success) return;

                }

                if (entries.Count > 0)
                    if (RIPHandler.Process.IsInProcess(IpV4.ToNetworkAddress(rp.Ip, rp.Mask)))
                        sender.SendPacket(RIPv2Packet.RIPv2ResponsePacketBuilder(rp, entries, dstMac, dstIP));
            }
        }

        public void StartSending()
        {
            int sec = 0;
            while (true)
            {
                if (sec >= RIPHandler.Timers.Update && sending)
                {
                    SendRIPv2();
                    sec = 0;
                }

                Thread.Sleep(1000);
                sec++;
            }
        }
    }
}
