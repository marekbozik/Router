using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class DHCPTransaction
    {
        private byte[] id;
        private IpV4Address offeredIP;
        private bool isAllocated;

        public DHCPTransaction(byte[] id)
        {
            this.id = id;
            isAllocated = false;
            offeredIP = new IpV4Address();
        }

        public byte[] Id { get => id; set => id = value; }
        public IpV4Address OfferedIP { get => offeredIP; set => offeredIP = value; }
        public bool IsAllocated { get => isAllocated; set => isAllocated = value; }
    }
}
