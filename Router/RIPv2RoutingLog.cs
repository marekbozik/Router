using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class RIPv2RoutingLog : RoutingLog
    {
        protected int metric;
        protected DateTime lastUpdate;
        protected bool isInvalid;
        protected bool isOnHoldDown;
        protected bool isFlushed;

        public int Metric { get => metric; set => metric = value; }
        public DateTime LastUpdate { get => lastUpdate; set => lastUpdate = value; }
        public bool IsInvalid { get => isInvalid; set => isInvalid = value; }
        public bool IsFlushed { get => isFlushed; set => isFlushed = value; }

        public bool IsOnHoldDown { get => isOnHoldDown; set => isOnHoldDown = value; }

        public RIPv2RoutingLog(RIPv2Packet p, RIPv2Entry e)
        {
            type = RoutingLog.typeRIPv2;
            ip = new IpV4Address(e.Ip.ToString());
            mask = e.Mask;
            nextHop = p.SrcIp;
            metric = ++e.Metric;
            lastUpdate = DateTime.Now;
            isInvalid = false;
            isFlushed = false;
            isOnHoldDown = false;
        }

        public void RegisterUpdate()
        {
            lastUpdate = DateTime.Now;
        }



    }
}
