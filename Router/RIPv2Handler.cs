namespace Router
{
    struct RIPv2Handler
    {
        private RIPv2Sender sender;
        private RIPv2Reciever reciever;

        internal RIPv2Sender Sender { get => sender; set => sender = value; }
        internal RIPv2Reciever Reciever { get => reciever; set => reciever = value; }

        public RIPv2Handler(RouterPort rp, Router router, int port)
        {
            sender = new RIPv2Sender();
            reciever = new RIPv2Reciever(rp, router, port);
        }
    }
}
