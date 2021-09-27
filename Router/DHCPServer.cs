using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    public class DHCPServer
    {
        private Queue<IpV4Address> pool;
        
        public DHCPServer() 
        {
            pool = new Queue<IpV4Address>();
        }

        public void SetPool(IpV4Address netIp, string subMask)
        {
            var ipArr = netIp.ToString().Split('.');
            var maskArr = subMask.Split('.');

            byte[] mask = new byte[4];
            for (int i = 0; i < 4; i++)
                mask[i] = Byte.Parse(maskArr[i]);

            byte[] ip = new byte[4];
            for (int i = 0; i < 4; i++)
                ip[i] = Byte.Parse(ipArr[i]);

            byte[] baseIp = new byte[4];
            for (int i = 0; i < 4; i++)
                baseIp[i] = ip[i];

            while (true)
            {
                if (ip[3] == 255)
                {
                    ip[3] = 0;
                    ip[2]++;
                    if (ip[2] == 255)
                    {
                        ip[2] = 0;
                        ip[3] = 0;
                        ip[1]++;
                        if (ip[1] == 255)
                        {
                            ip[1] = 0;
                            ip[2] = 0;
                            ip[3] = 0;
                            ip[0]++;
                        }
                    }
                }
                bool end = false;
                for (int i = 0; i < 4; i++)
                {
                    byte b = (byte)(ip[i] & mask[i]);
                    if (b != baseIp[i])
                    {
                        end = true;
                        break;
                    }
                }
                if (end)
                {
                    break;
                }
                else
                {
                    pool.Enqueue(new IpV4Address(ip[0].ToString() + "." + ip[1].ToString() + "." + ip[2].ToString() + "." + ip[3].ToString()));
                }
            }

        }
    }
}
