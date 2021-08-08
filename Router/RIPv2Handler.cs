namespace Router
{
    class RIPv2Handler
    {
        private RIPv2Sender sender1, sender2;
        private RIPv2Reciever reciever1, reciever2;
        private RIPv2Timer timers;
        private Router router;

        internal RIPv2Sender Sender1 { get => sender1; set => sender1 = value; }
        internal RIPv2Sender Sender2 { get => sender2; set => sender2 = value; }
        internal RIPv2Reciever Reciever1 { get => reciever1; set => reciever1 = value; }
        internal RIPv2Reciever Reciever2 { get => reciever2; set => reciever2 = value; }
        internal RIPv2Timer Timers { get => timers; set => timers = value; }

        public RIPv2Handler(Router router)
        {
            sender1 = new RIPv2Sender();
            sender2 = new RIPv2Sender();
            reciever1 = new RIPv2Reciever(router.Port1, router, 1);
            reciever2 = new RIPv2Reciever(router.Port2, router, 2);
            this.router = router;
        }

        public void TimersHandle()
        {
            router.RoutingTable.RIPv2TimersHandle(timers);
        }

    }
}
