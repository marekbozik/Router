using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class DHCPServer
    {
        private Stack<IpV4Address> pool;
        private Dictionary<IpV4Address, MacAddress> reservedIPs;
        private Dictionary<MacAddress, IpV4Address> manualAllocIPs;
        private Dictionary<byte[], IpV4Address> transactions;

        private bool isEnabled;
        private RouterPort rp;
        public DHCPServer(RouterPort rp) 
        {
            pool = new Stack<IpV4Address>();
            reservedIPs = new Dictionary<IpV4Address, MacAddress>();
            manualAllocIPs = new Dictionary<MacAddress, IpV4Address>();
            transactions = new Dictionary<byte[], IpV4Address>();

            isEnabled = false;
            this.rp = rp;
        }

        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }
        public Stack<IpV4Address> Pool { get => pool; set => pool = value; }

        public void ManualAlloc(MacAddress mac, IpV4Address ip)
        {
            reservedIPs[ip] = mac;
            manualAllocIPs[mac] = ip;
        }

        private IpV4Address GetNextIP(MacAddress srcMac)
        {
            if (manualAllocIPs.ContainsKey(srcMac))
            {
                return manualAllocIPs[srcMac];
            }
            try
            {
                IpV4Address nextIp = new IpV4Address();
                while (true)
                {
                    nextIp = pool.Pop();
                    if (nextIp == rp.Ip || reservedIPs.ContainsKey(nextIp))
                    {
                        continue;
                    }
                    else break;
                }
                return nextIp;
            }
            catch (Exception)
            {

                throw;
            }

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
                else ip[3]++;
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
                    pool.Push(new IpV4Address(ip[0].ToString() + "." + ip[1].ToString() + "." + ip[2].ToString() + "." + ip[3].ToString()));
                }
            }

            try
            {
                pool.Pop();
            }
            catch (Exception)
            {

            }

        }
    }
}
