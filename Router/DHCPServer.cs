using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Router
{
    class DHCPServer
    {
        private bool isEnabled;
        private RouterPort rp;
        private DHCPPool dhcpIpPool;
        private uint leaseTime;
        private bool isLeasing;
        private System.Windows.Forms.RichTextBox rich;
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
        internal DHCPPool DhcpIpPool { get => dhcpIpPool; set => dhcpIpPool = value; }
        public RichTextBox Rich { get => rich; set => rich = value; }

        //public Stack<IpV4Address> Pool { get => pool; set => pool = value; }


        private void ProcessDiscover(DHCPPacket p)
        {
            dhcpIpPool.NewTransaction(p.TransactionID, p.SourceMacAddress);
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
                bool localLease = isLeasing;

                //ack re-lease
                if (dhcpIpPool.ManualAllocIPs.ContainsValue(p.SrcIp))
                {
                    dhcpIpPool.GetTransaction(p.TransactionID).AllocatedUntil = DateTime.MaxValue;
                    localLease = false;
                }
                else if (isLeasing)
                    dhcpIpPool.GetTransaction(p.TransactionID).AllocatedUntil = DateTime.Now + TimeSpan.FromSeconds(leaseTime);
                else
                    dhcpIpPool.GetTransaction(p.TransactionID).AllocatedUntil = DateTime.MaxValue;

                rp.Sender.SendPacket(
                    DHCPPacket.DHCPAckPacketBuilder(
                        rp,
                        p.TransactionID,
                        dhcpIpPool.GetTransaction(p.TransactionID).OfferedIP,
                        p.SourceMacAddress,
                        dhcpIpPool.SubnetMask,
                        leaseTime,
                        localLease
                    )
                );
            }
            else //ack
            {
                dhcpIpPool.AllocIP(p.TransactionID);
                bool localLease = isLeasing;

                //ack re-lease
                if (dhcpIpPool.ManualAllocIPs.ContainsValue(dhcpIpPool.GetTransaction(p.TransactionID).OfferedIP))
                {
                    dhcpIpPool.GetTransaction(p.TransactionID).AllocatedUntil = DateTime.MaxValue;
                    localLease = false;
                }
                else if(isLeasing)
                    dhcpIpPool.GetTransaction(p.TransactionID).AllocatedUntil = DateTime.Now + TimeSpan.FromSeconds(leaseTime);
                else
                    dhcpIpPool.GetTransaction(p.TransactionID).AllocatedUntil = DateTime.MaxValue;

                rp.Sender.SendPacket(
                    DHCPPacket.DHCPFirstAckPacketBuilder(
                        rp, 
                        p.TransactionID, 
                        dhcpIpPool.GetTransaction(p.TransactionID).OfferedIP, 
                        p.SourceMacAddress, 
                        dhcpIpPool.SubnetMask, 
                        leaseTime, 
                        localLease
                    )
                );
            }
        }

        private void ProcessRelease(DHCPPacket p)
        {
            dhcpIpPool.Release(p.TransactionID);
        }

        public void ProcessPacket(DHCPPacket p)
        {
            if (!isEnabled || !dhcpIpPool.IsPoolSet) return;

            if (p.MessageType == DHCPPacket.MessageTypeDiscover)
                ProcessDiscover(p);
            else if (p.MessageType == DHCPPacket.MessageTypeRequest)
                ProcessRequest(p);
            else if (p.MessageType == DHCPPacket.MessageTypeRelease)
                ProcessRelease(p);
            UpdateRich();
        }

        private void UpdateRich()
        {
            if (rich == null) return;
            rich.BeginInvoke(new Action(() =>
            {
                rich.Clear();
                foreach (var i in dhcpIpPool.UsedIPs)
                {
                    rich.AppendText(i.Value.OfferedIP.ToString() + "  |  " + i.Value.Mac.ToString() + "  |  " + "lease until " + i.Value.AllocatedUntil + "\n");
                }
                rich.AppendText("\n--------------------------------------------------------\nStatic ip entries:\n");
                foreach (var i in dhcpIpPool.ManualAllocIPs)
                {
                    rich.AppendText(i.Value.ToString() + "  |  " + i.Key.ToString() + "  |  " + "infinite lease\n");
                }
                //richTextBox2.AppendText(s1.GetStats());
            }));
        }



        
    }
}
