using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Router
{
    class RIPv2Reciever
    {
        private bool recieving;
        private RouterPort rp;

        public RIPv2Reciever(RouterPort rp)
        {
            this.rp = rp;
            recieving = false;
            StartRecieving();
        }

        private void StartRecieving()
        {
            new Thread(() =>
            {
                while (true)
                {
                    if (recieving)
                    {

                    }

                    Thread.Sleep(250);
                }
            }).Start();
        }
    }
}
