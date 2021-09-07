using System.Threading;

namespace Router
{
    class RIPv2Handler
    {
        private RIPv2Sender sender1, sender2;
        private RIPv2Reciever reciever1, reciever2;
        private RIPv2Timer timers;
        private Router router;
        private bool isRIPv2Enabled;
        private RIPv2Process process;

        internal RIPv2Sender Sender1 { get => sender1; set => sender1 = value; }
        internal RIPv2Sender Sender2 { get => sender2; set => sender2 = value; }
        internal RIPv2Reciever Reciever1 { get => reciever1; set => reciever1 = value; }
        internal RIPv2Reciever Reciever2 { get => reciever2; set => reciever2 = value; }
        internal RIPv2Timer Timers { get => timers; set => timers = value; }
        internal RIPv2Process Process { get => process; set => process = value; }
        public bool IsRIPv2Enabled { get => isRIPv2Enabled; set => isRIPv2Enabled = SetIsEnabled(value); }
        internal Router Router { get => router; set => router = value; }

        public RIPv2Handler(Router router)
        {
            sender1 = new RIPv2Sender(router.Port1, this, router.Sender1, 1);
            sender2 = new RIPv2Sender(router.Port2, this, router.Sender2, 2);
            reciever1 = new RIPv2Reciever(router.Port1, router, this);
            reciever2 = new RIPv2Reciever(router.Port2, router, this);
            timers = new RIPv2Timer();
            timers.Update = 15;//30;
            timers.Invalid = 30;//60; //180
            timers.Holddown = 60;//30; //180
            timers.Flush = 60;//180; //240
            this.router = router;
            isRIPv2Enabled = false;
            process = new RIPv2Process(this);
        }

        private bool SetIsEnabled(bool enabled)
        {
            if (!enabled)
            {
                reciever1.Recieving = false;
                reciever2.Recieving = false;
                sender1.Sending = false;
                sender2.Sending = false;
            }
            //new Thread(() => { 
            //    router.Sender1.SendPacket(RIPv2Packet.RIPv2RequestPacketBuilder(router.Port1));
            //    router.Sender2.SendPacket(RIPv2Packet.RIPv2RequestPacketBuilder(router.Port2));
            //}).Start();


            return enabled;
        }

        public void TimersHandle()
        {
            router.RoutingTable.RIPv2TimersHandle(timers);
        }

    }
}
