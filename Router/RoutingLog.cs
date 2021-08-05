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

        protected int type;
        protected IpV4Address ip;
        protected string mask;
        protected int outInt;
        protected IpV4Address nextHop;
        


        public RoutingLog(int type, IpV4Address ip, string mask, int outInt)
        {
            this.type = type;
            this.ip = ip;
            this.mask = mask;
            this.outInt = outInt;

        }

        public RoutingLog(int type, IpV4Address ip, string mask, IpV4Address nextHop)
        {
            this.type = type;
            this.ip = ip;
            this.mask = mask;
            this.nextHop = nextHop;
            
        }

        public RoutingLog(int type, IpV4Address ip, string mask, int outInt, IpV4Address nextHop) : this(type, ip, mask, outInt)
        {
            this.nextHop = nextHop;
        }

        protected RoutingLog()
        {
        }

        public override string ToString()
        {
            char c = '-';
            if (type == RoutingLog.typeConnected) c = 'C';
            else if (type == RoutingLog.typeRIPv2) c = 'R';
            else if (type == RoutingLog.typeStatic) c = 'S';

            string ipp = ip.ToString();
            int len = ipp.Length;
            for (int j = 0; j < 15 - len; j++)
            {
                ipp += " ";
            }

            string maskk = mask;
            len = mask.Length;
            for (int j = 0; j < 15 - len; j++)
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

        public static bool operator ==(RoutingLog obj1, RoutingLog obj2)
        {
            if (obj1.Type == obj2.Type &&
                obj1.Ip == obj2.Ip &&
                obj1.Mask == obj2.Mask &&
                (obj1.OutInt == obj2.OutInt || obj1.NextHop == obj2.NextHop)
                )
                return true;
            return false;
        }

        public static bool operator !=(RoutingLog obj1, RoutingLog obj2)
        {
            if (obj1.Type == obj2.Type &&
                obj1.Ip == obj2.Ip &&
                obj1.Mask == obj2.Mask &&
                (obj1.OutInt == obj2.OutInt || obj1.NextHop == obj2.NextHop)
                )
                return false;
            return true;
        }


        public string Mask { get => mask; set => mask = value; }
        public int Type { get => type; set => type = value; }
        public IpV4Address Ip { get => ip; set => ip = value; }
        public int OutInt { get => outInt; set => outInt = value; }
        public IpV4Address NextHop { get => nextHop; set => nextHop = value; }

    }
}
