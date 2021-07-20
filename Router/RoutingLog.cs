using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class RoutingLog
    {
        public static readonly int typeConnected = 0;
        public static readonly int typeStatic = 1;
        public static readonly int typeRIPv2 = 2;

        private int type;
        private IpV4Address ip;
        private string mask;
        private int outInt;
        private IpV4Address nextHop;
        private bool removed;

        public RoutingLog(int type, IpV4Address ip, string mask, int outInt)
        {
            this.type = type;
            this.ip = ip;
            this.mask = mask;
            this.outInt = outInt;
            removed = false;
        }

        public RoutingLog(int type, IpV4Address ip, string mask, IpV4Address nextHop)
        {
            this.type = type;
            this.ip = ip;
            this.mask = mask;
            this.nextHop = nextHop;
            removed = false;
        }

        public RoutingLog(int type, IpV4Address ip, string mask, int outInt, IpV4Address nextHop) : this(type, ip, mask, outInt)
        {
            this.nextHop = nextHop;
        }

        public override string ToString()
        {
            char c = '-';
            if (type == RoutingLog.typeConnected) c = 'C';
            else if (type == RoutingLog.typeRIPv2) c = 'R';
            else if (type == RoutingLog.typeStatic) c = 'S';

            string ipp = ip.ToString();
            for (int j = 0; j < 19 - ipp.Length; j++)
            {
                ipp += " ";
            }

            string maskk = mask;
            for (int j = 0; j < 19 - mask.Length; j++)
            {
                maskk += " ";
            }
            if ((outInt == 1 || outInt == 2) && nextHop != new IpV4Address())
                return "  " + c + "  | " + ipp + " | " + maskk + " |     " + outInt + "       | " + nextHop.ToString();
            else if (outInt == 1 || outInt == 2)
                return "  " + c + "  | " + ipp + " | " + maskk + " |     " + outInt + "     | - ";
            else
                return "  " + c + "  | " + ipp + " | " + maskk + " |     " + "-     | " + nextHop.ToString();

        }

        public string Mask { get => mask; set => mask = value; }
        public int Type { get => type; set => type = value; }
        public IpV4Address Ip { get => ip; set => ip = value; }
        public int OutInt { get => outInt; set => outInt = value; }
        public IpV4Address NextHop { get => nextHop; set => nextHop = value; }
        public bool Removed { get => removed; set => removed = value; }
    }
}
