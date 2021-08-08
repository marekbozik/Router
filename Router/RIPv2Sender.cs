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

        public RIPv2Sender()
        {
            sending = false;
        }

        public bool Sending { get => sending; set => sending = value; }

        public void StartSending()
        {

        }
    }
}
