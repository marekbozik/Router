using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class DHCPPool
    {
        private Stack<IpV4Address> pool;
        private Dictionary<IpV4Address, MacAddress> reservedIPs;
        private Dictionary<MacAddress, IpV4Address> manualAllocIPs;
        private Dictionary<byte[], DHCPTransaction> transactions;
        private bool isPoolSet;
        private string subnetMask;

        public bool IsPoolSet { get => isPoolSet; set => isPoolSet = value; }
        public string SubnetMask { get => subnetMask; set => subnetMask = value; }

        public DHCPPool()
        {
            pool = new Stack<IpV4Address>();
            reservedIPs = new Dictionary<IpV4Address, MacAddress>();
            manualAllocIPs = new Dictionary<MacAddress, IpV4Address>();
            transactions = new Dictionary<byte[], DHCPTransaction>();
            isPoolSet = false;
        }

        public bool HasAllocatedIP(byte[] transId)
        {
            try
            {
                if (transactions[transId].IsAllocated)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public DHCPTransaction GetTransaction(byte[] id)
        {
            return transactions[id];
        }

        public void NewTransaction(byte[] id)
        {
            transactions[id] = new DHCPTransaction(id);
        }

        public void NewIpOffer(byte[] transId, IpV4Address offerIp)
        {
            transactions[transId].OfferedIP = offerIp;
        }

        public IpV4Address AllocIP(byte[] transId)
        {
            transactions[transId].IsAllocated = true;
            return transactions[transId].OfferedIP;
        }


        public void ManualAlloc(MacAddress mac, IpV4Address ip)
        {
            if (isPoolSet)
            {
                reservedIPs[ip] = mac;
                manualAllocIPs[mac] = ip;
            }
        }

        public IpV4Address GetNextIP(MacAddress srcMac, RouterPort rp)
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
            subnetMask = subMask;
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
            isPoolSet = true;
        }

    }
}
