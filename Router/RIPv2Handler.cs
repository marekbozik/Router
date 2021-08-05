namespace Router
{
    class RIPv2Handler
    {
        private RIPv2Sender sender;
        private RIPv2Reciever reciever;
        private RIPv2Timer timers;
        private Router router;

        internal RIPv2Sender Sender { get => sender; set => sender = value; }
        internal RIPv2Reciever Reciever { get => reciever; set => reciever = value; }
        internal RIPv2Timer Timers { get => timers; set => timers = value; }

        public RIPv2Handler(RouterPort rp, Router router, int port)
        {
            sender = new RIPv2Sender();
            reciever = new RIPv2Reciever(rp, router, port);
            this.router = router;
        }

        public void TimersHandle()
        {
            //router.RoutingTable.
        }

    }
}
