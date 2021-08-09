using PcapDotNet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class RIPv2Sender
    {
        private bool sending;
        private RouterPort rp;
        private PacketCommunicator sender;
        private RIPv2Handler RIPv2Handler;

        public RIPv2Sender(RouterPort rp, RIPv2Handler handler, PacketCommunicator sender)
        {
            sending = false;
            this.RIPv2Handler = handler;
            this.rp = rp;
            this.sender = sender;
        }

        public bool Sending { get => sending; set => sending = SetSending(value); }

        private bool SetSending(bool b)
        {
            if (b && !sending && RIPv2Handler.Process.IsInProcess(rp.Ip, rp.Mask))
            {
                sender.SendPacket(RIPv2Packet.RIPv2RequestPacketBuilder(rp));
            }
            return b;
        }

        public void StartSending()
        {

        }
    }
}
