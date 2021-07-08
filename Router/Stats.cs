using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    class Stats
    {
        private int ethernet;
        private int arp;
        private int ip;
        private int tcp;
        private int udp;
        private int icmp;
        private int http;
        private MacAddress [] macs;


        public Stats(Router r)
        {
            MacAddress[] mac = { r.Port1.Mac, r.Port2.Mac };
            macs = mac;
            this.ethernet = 0;
            this.arp = 0;
            this.ip = 0;
            this.tcp = 0;
            this.udp = 0;
            this.icmp = 0;
            this.http = 0;
        }

        public Stats()
        {
            macs = null;
            this.ethernet = 0;
            this.arp = 0;
            this.ip = 0;
            this.tcp = 0;
            this.udp = 0;
            this.icmp = 0;
            this.http = 0;
        }

        public void ResetStats()
        {
            this.ethernet = 0;
            this.arp = 0;
            this.ip = 0;
            this.tcp = 0;
            this.udp = 0;
            this.icmp = 0;
            this.http = 0;
        }

        public int Ethernet { get => ethernet; set => ethernet = value; }
        public int Arp { get => arp; set => arp = value; }
        public int Ip { get => ip; set => ip = value; }
        public int Tcp { get => tcp; set => tcp = value; }
        public int Udp { get => udp; set => udp = value; }
        public int Icmp { get => icmp; set => icmp = value; }
        public int Http { get => http; set => http = value; }

        private int GetEthType(byte[] pBytes)
        {
            byte[] subBytes = new byte[2];
            subBytes[0] = pBytes[12];
            subBytes[1] = pBytes[13];
            string s = BitConverter.ToString(subBytes);
            string vys = "";
            foreach (char c in s)
            {
                if (c != '-')
                {
                    vys += c;
                }
            }

            return Int32.Parse(vys, System.Globalization.NumberStyles.HexNumber);
        }

        public string GetStats()
        {
            string s = "";
            s += "Ethernet: " + this.ethernet + "\n";
            s += "ARP: " + this.arp + "\n";
            s += "IPv4: " + this.ip + "\n";
            s += "TCP: " + this.tcp + "\n";
            s += "UDP: " + this.udp + "\n";
            s += "ICMP: " + this.icmp + "\n";
            s += "HTTP: " + this.http + "\n";

            return s;
        }

        public void Increment(Packet p)
        {
            if (p == null) return;

            if (macs != null)
                foreach (var mac in macs)
                {
                    if (p.Ethernet.Source == mac)
                        return;
                }
            
            int ethType = GetEthType(p.Buffer);
            if (ethType >= 2048)
            {
                this.ethernet++;
                if (p.Ethernet.EtherType == PcapDotNet.Packets.Ethernet.EthernetType.Arp)
                {
                    this.arp++;
                }
                else if (p.Ethernet.EtherType == PcapDotNet.Packets.Ethernet.EthernetType.IpV4)
                {
                    this.ip++;
                    if (p.Ethernet.IpV4.Protocol == PcapDotNet.Packets.IpV4.IpV4Protocol.Udp)
                    {
                        this.udp++;
                    }
                    else if (p.Ethernet.IpV4.Protocol == PcapDotNet.Packets.IpV4.IpV4Protocol.Tcp)
                    {
                        this.tcp++;
                        if (p.Ethernet.IpV4.Tcp.DestinationPort == 80 || p.Ethernet.IpV4.Tcp.SourcePort == 80)
                        {
                            this.http++;
                        }
                    }
                    else if (p.Ethernet.IpV4.Protocol == PcapDotNet.Packets.IpV4.IpV4Protocol.InternetControlMessageProtocol)
                    {
                        this.icmp++;
                    }
                }
            }
        }
    }
}
