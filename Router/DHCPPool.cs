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
        private Dictionary<IpV4Address, DHCPTransaction> usedIPs;

        private bool isPoolSet;
        private string subnetMask;

        public bool IsPoolSet { get => isPoolSet; set => isPoolSet = value; }
        public string SubnetMask { get => subnetMask; set => subnetMask = value; }
        internal Dictionary<IpV4Address, DHCPTransaction> UsedIPs { get => usedIPs; set => usedIPs = value; }
        internal Dictionary<MacAddress, IpV4Address> ManualAllocIPs { get => manualAllocIPs; set => manualAllocIPs = value; }

        public DHCPPool()
        {
            pool = new Stack<IpV4Address>();
            reservedIPs = new Dictionary<IpV4Address, MacAddress>();
            manualAllocIPs = new Dictionary<MacAddress, IpV4Address>();
            transactions = new Dictionary<byte[], DHCPTransaction>(new ByteEqualityComparer());
            usedIPs = new Dictionary<IpV4Address, DHCPTransaction>();
            isPoolSet = false;
        }

        public void Release(byte [] transId)
        {
            if (transactions.ContainsKey(transId))
            {
                var ipRem = transactions[transId].OfferedIP;
                usedIPs.Remove(ipRem);
                pool.Push(ipRem);
            }
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

        //step 1
        public void NewTransaction(byte[] id, MacAddress mac)
        {
            transactions[id] = new DHCPTransaction(id, mac);
        }

        //step 2
        public void NewIpOffer(byte[] transId, IpV4Address offerIp)
        {
            transactions[transId].OfferedIP = offerIp;
            usedIPs[offerIp] = transactions[transId];
        }

        //step 3
        public void AllocIP(byte[] transId)
        {
            transactions[transId].IsAllocated = true;
        }


        public void ManualAlloc(MacAddress mac, IpV4Address ip)
        {
            if (usedIPs.ContainsKey(ip) || reservedIPs.ContainsKey(ip))
            {
                throw new Exception();
            }
            if (isPoolSet)
            {
                if (manualAllocIPs.ContainsKey(mac))
                {
                    var remIp = new IpV4Address();
                    foreach (var i in reservedIPs)
                    {
                        if (i.Value == mac)
                        {
                            remIp = i.Key;
                        }
                    }
                    reservedIPs.Remove(remIp);
                    return;
                }
                reservedIPs[ip] = mac;
                manualAllocIPs[mac] = ip;
                //usedIPs[ip] = new DHCPTransaction(;
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

        public class ByteEqualityComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] x, byte[] y)
            {
                if (x.Length != y.Length)
                {
                    return false;
                }
                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(byte[] obj)
            {
                int result = 17;
                for (int i = 0; i < obj.Length; i++)
                {
                    unchecked
                    {
                        result = result * 23 + obj[i];
                    }
                }
                return result;
            }
        }
    }
}
