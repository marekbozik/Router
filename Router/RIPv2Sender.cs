using PcapDotNet.Core;
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

        private bool SetSending(bool b)
        {
            if (b && !sending && RIPHandler.Process.IsInProcess(rp.Ip, rp.Mask))
            {
                sender.SendPacket(RIPv2Packet.RIPv2RequestPacketBuilder(rp));
            }
            return b;
        }

        public void StartSending()
        {
            int sec = 0;
            while (true)
            {
                if (sec >= RIPHandler.Timers.Update && sending)
                {
                    var tableEntriesList = RIPHandler.Router.RoutingTable.GetRIPv2LogsFor(rp.Ip, rp.Mask);
                    var added = RIPHandler.Process.GetAddedNetworks();
                    List<RIPv2Entry> validAdded = new List<RIPv2Entry>(added.Count);
                    RIPv2EntryOrdered res;
                    while (added.TryDequeue(out res))
                    {
                        if (RIPHandler.Router.RoutingTable.GetOutInt(res.Ip) != port)
                            validAdded.Add(res);
                    }
                    tableEntriesList.AddRange(validAdded);

                    List<byte[]> entries = new List<byte[]>(tableEntriesList.Count);
                    foreach (var en in tableEntriesList)
                    {
                        entries.Add(en.ToBytes());
                    }
                    sender.SendPacket(RIPv2Packet.RIPv2ResponsePacketBuilder(rp, entries));
                    sec = 0;
                }

                Thread.Sleep(1000);
                sec++;
            }
        }
    }
}
