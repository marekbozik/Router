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


        private bool isEnabled;
        private RouterPort rp;
        private DHCPPool dhcpIpPool;
        private uint leaseTime;
        private bool isLeasing;
        public DHCPServer(RouterPort rp)
        {
            isEnabled = false;
            this.rp = rp;
            dhcpIpPool = new DHCPPool();
            leaseTime = 0;
            isLeasing = false;
        }

        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }
        public uint LeaseTime { get => leaseTime; set => leaseTime = value; }
        public bool IsLeasing { get => isLeasing; set => isLeasing = value; }

        //public Stack<IpV4Address> Pool { get => pool; set => pool = value; }


        private void ProcessDiscover(DHCPPacket p)
        {
            dhcpIpPool.NewTransaction(p.TransactionID);
            IpV4Address nextIp;
            try
            {
                nextIp = dhcpIpPool.GetNextIP(p.SourceMacAddress, rp);
            }
            catch (Exception)
            {
                return;
            }
            var offer = DHCPPacket.DHCPOfferPacketBuilder(rp, p.SourceMacAddress, p.TransactionID, nextIp, dhcpIpPool.SubnetMask);
            dhcpIpPool.NewIpOffer(p.TransactionID, nextIp);
            rp.Sender.SendPacket(offer);
        }

        private void ProcessRequest(DHCPPacket p)
        {
            if (dhcpIpPool.HasAllocatedIP(p.TransactionID))
            {
                //ack re-lease
            }
            else //ack
            {
                dhcpIpPool.AllocIP(p.TransactionID);
                rp.Sender.SendPacket(
                    DHCPPacket.DHCPFirstAckPacketBuilder(
                        rp, 
                        p.TransactionID, 
                        dhcpIpPool.GetTransaction(p.TransactionID).OfferedIP, 
                        p.SourceMacAddress, 
                        dhcpIpPool.SubnetMask, 
                        leaseTime, 
                        isLeasing
                    )
                );
            }
        }

        public void ProcessPacket(DHCPPacket p)
        {
            if (!isEnabled) return;

            if (p.MessageType == DHCPPacket.MessageTypeDiscover)
                ProcessDiscover(p);
            else if (p.MessageType == DHCPPacket.MessageTypeRequest)
                ProcessRequest(p);
        }




        
    }
}
