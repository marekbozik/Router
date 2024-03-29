﻿using PcapDotNet.Core;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using System;
using System.Threading;
using System.Windows.Forms;


namespace Router
{
    public partial class RouterGui : Form
    {
        private Router router;
        private RIPv2Handler ripHandler;
        private Stats in1, in2, out1, out2;
        private bool arpViewFocus;
        private bool routingTableViewFocus;

        public RouterGui()
        {
            InitializeComponent();
            
            //design 
            {
                tabs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                tableLayoutPanel4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                tableLayoutPanel3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                richTextBox2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                richTextBox3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                richTextBox4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                richTextBox5.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                tableLayoutPanel2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            }
            RIPv2Port1StateButton.Enabled = false;
            RIPv2Port2StateButton.Enabled = false;

            router = null;
            try
            {
                router = new Router();
                port1IpTextBox.Text = router.Port1.Ip.ToString();
                port1MaskTextBox.Text = router.Port1.Mask.ToString();
                port2IpTextBox.Text = router.Port2.Ip.ToString();
                port2MaskTextBox.Text = router.Port2.Mask.ToString();
            }
            catch (SerializeException)
            {
                DialogResult res = MessageBox.Show("No loopback interface configured", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabs.TabPages.Remove(routerTab);

            }

            var devs = Router.GetPacketDevices();
            for (int i = 0; i != devs.Count; ++i)
            {
                LivePacketDevice device = devs[i];

                richTextBox1.AppendText((i + 1) + ". " + device.Name);
                    
                if (device.Description != null)
                {
                    richTextBox1.AppendText(" (" + device.Description + ")\n");
                }
                else
                {
                    richTextBox1.AppendText(" (No description available)");
                    richTextBox1.AppendText("\n");
                }
            }
            
            Stats();

            arpListView.View = View.Details;
            foreach (var i in router.ArpTable.GetTable())
            {
                arpListView.Items.Add(i);
            }

            if (router != null)
                arpAgingTimerBox.Text = router.ArpTable.AgingTime.ToString();

            if (router != null)
            {
                router.Port1.DhcpServer.Rich = dhcpPort1RichTextBox;
                router.Port2.DhcpServer.Rich = dhcpPort2RichTextBox;
            }

            new Thread(() => { router.Forward(router.Port1); }).Start();
            new Thread(() => { router.Forward(router.Port2); }).Start();
            /* RIPv2 */
            ripHandler = new RIPv2Handler(router);
            new Thread(() => { ripHandler.Reciever1.StartRecieving(); }).Start();
            new Thread(() => { ripHandler.Reciever2.StartRecieving(); }).Start();
            new Thread(() => { ripHandler.Sender1.StartSending(); }).Start();
            new Thread(() => { ripHandler.Sender2.StartSending(); }).Start();
            new Thread(() => {
                while (true)
                {
                    ripHandler.TimersHandle();
                    Thread.Sleep(1000);
                }
            }).Start();



            arpViewFocus = false;
            routingTableViewFocus = false;
            routingTableListView.Scrollable = false;

            routingTableListView.Items.Clear();
            routingTableListView.View = View.Details;


            tabs.TabPages.Remove(appSettingTab);
            tabs.TabPages.Add(appSettingTab);

            ripUpdateTimerTextBox.Text = ripHandler.Timers.Update.ToString();
            ripInvalidTimerTextBox.Text = ripHandler.Timers.Invalid.ToString();
            ripHolddownTimerTextBox.Text = ripHandler.Timers.Holddown.ToString();
            ripFlushTimerTextBox.Text = ripHandler.Timers.Flush.ToString();


        }

        private void Stats()
        {
            if (router != null)
            {
                Stats s1 = new Stats(router);
                in1 = s1;
                Stats s2 = new Stats(router);
                in2 = s2;
                Stats s3 = new Stats(router);
                Stats s4 = new Stats(router);
                out1 = s3; out2 = s4;

                new Thread(() => { new Sniffer(router.Port1.DeviceInterface, s1).SniffingIn(); }).Start();
                new Thread(() => { new Sniffer(router.Port2.DeviceInterface, s2).SniffingIn(); }).Start();
                new Thread(() => { new Sniffer(router.Port1.DeviceInterface, s3).SniffingOut(); }).Start();
                new Thread(() => { new Sniffer(router.Port2.DeviceInterface, s4).SniffingOut(); }).Start();

                new Thread(() =>
                {
                    while (true)
                    {
                        System.Threading.Thread.Sleep(1000);
                        richTextBox2.BeginInvoke(new Action(() =>
                        {
                            richTextBox2.Clear();
                            richTextBox2.AppendText(s1.GetStats());
                        }));
                        richTextBox3.BeginInvoke(new Action(() =>
                        {
                            richTextBox3.Clear();
                            richTextBox3.AppendText(s2.GetStats());
                        }));
                        richTextBox5.BeginInvoke(new Action(() =>
                        {
                            richTextBox5.Clear();
                            richTextBox5.AppendText(s3.GetStats());
                        }));
                        richTextBox4.BeginInvoke(new Action(() =>
                        {
                            richTextBox4.Clear();
                            richTextBox4.AppendText(s4.GetStats());
                        }));
                        
                        arpListView.BeginInvoke(new Action(() =>
                        {
                            if (tabs.SelectedTab == tabPage1)
                            {
                                int q = 0;
                                try
                                {
                                    var x = arpListView.SelectedItems[0];
                                    q = x.Index;
                                }
                                catch (Exception) { }

                                arpListView.Items.Clear();
                                arpListView.View = View.Details;
                                //int j = 0, q = 0;
                                foreach (var i in router.ArpTable.GetTable())
                                {
                                    arpListView.Items.Add(i);
                                }
                                if (arpViewFocus)
                                {
                                    try
                                    {
                                        arpListView.Items[q].Selected = true;
                                        arpListView.Select();
                                    }
                                    catch (Exception) { }
                                }
                            }
                                
                        }
                        ));
                        routingTableListView.BeginInvoke(new Action(() =>
                        {
                            if (tabs.SelectedTab == tabPage1)
                            {
                                int q = 0;
                                try
                                {
                                    var x = routingTableListView.SelectedItems[0];
                                    q = x.Index;
                                }
                                catch (Exception) { }

                                routingTableListView.Items.Clear();
                                routingTableListView.View = View.Details;

                                foreach (var i in router.RoutingTable.GetTable())
                                {
                                    routingTableListView.Items.Add(i);
                                }
                                if (routingTableViewFocus)
                                {
                                    try
                                    {
                                        routingTableListView.Items[q].Selected = true;
                                        routingTableListView.Select();
                                    }
                                    catch (Exception) { }
                                }
                                //routingTableListView.Scrollable = true;
                                //routingTableListView.Scrollable = false;
                            }
                        }));

                        

                    }
                }).Start();
            }
        }

        private void interfaceSaveButton_Click(object sender, EventArgs e)
        {
            var devs = Router.GetPacketDevices();
            if (router == null)
            {
                int indx1 = Int32.Parse(intPort1.Text) - 1;
                int indx2 = Int32.Parse(intPort2.Text) - 1;
                if (indx1 == indx2)
                {
                    MessageBox.Show("Error occured", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    RouterPort rp1 = new RouterPort(devs[indx1], new IpV4Address("10.0.0.1"), "255.255.255.0", new MacAddress("00:00:00:00:01:01"));
                    RouterPort rp2 = new RouterPort(devs[indx2], new IpV4Address("10.0.0.2"), "255.255.255.0", new MacAddress("00:00:00:00:02:02"));
                    router = new Router(rp1, rp2);
                }
                catch (Exception)
                {
                    router = null;
                    MessageBox.Show("Error occured", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                router.Serialize();
                port1IpTextBox.Text = router.Port1.Ip.ToString();
                port1MaskTextBox.Text = router.Port1.Mask.ToString();
                port2IpTextBox.Text = router.Port2.Ip.ToString();
                port2MaskTextBox.Text = router.Port2.Mask.ToString();
                interfaceInfoRich.AppendText(DateTime.Now.ToString() + " Settings changed \n");
                router.ArpTable.UpdatePortsIp(router);
                router.RoutingTable.SetConnected(router);
                
                tabs.TabPages.Remove(appSettingTab);
                tabs.TabPages.Add(routerTab);
                tabs.TabPages.Add(appSettingTab);
                Stats();
            }
            else
            {
                int indx1 = Int32.Parse(intPort1.Text) - 1;
                int indx2 = Int32.Parse(intPort2.Text) - 1;
                if (indx1 == indx2)
                {
                    MessageBox.Show("Error occured", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    var x = devs[indx1];
                    x = devs[indx2];
                }
                catch (Exception)
                {
                    router = null;
                    MessageBox.Show("Error occured", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                router.Port1 = new RouterPort(devs[indx1], router.Port1.Ip, router.Port1.Mask, new MacAddress("00:00:00:00:01:01"));
                router.Port2 = new RouterPort(devs[indx2], router.Port2.Ip, router.Port2.Mask, new MacAddress("00:00:00:00:02:02"));

                router.Serialize();
                port1IpTextBox.Text = router.Port1.Ip.ToString();
                port1MaskTextBox.Text = router.Port1.Mask.ToString();
                port2IpTextBox.Text = router.Port2.Ip.ToString();
                port2MaskTextBox.Text = router.Port2.Mask.ToString();
                router.ArpTable.UpdatePortsIp(router);
                router.RoutingTable.SetConnected(router);

                interfaceInfoRich.AppendText(DateTime.Now.ToString() + " Settings changed \n");
            }
        }

        private void ipAddSetButton_Click(object sender, EventArgs e)
        {
            IpV4Address ip1, ip2;
            try
            {
                ip1 = new IpV4Address(port1IpTextBox.Text);
                ip2 = new IpV4Address(port2IpTextBox.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Error occured", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (IpV4.IsConflict(ip1, port1MaskTextBox.Text, ip2, port2MaskTextBox.Text))
            {
                MessageBox.Show("Ip adress conflict!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!IpV4.IsMask(port1MaskTextBox.Text) | !IpV4.IsMask(port2MaskTextBox.Text))
            {
                MessageBox.Show("Mask error", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            ripHandler.Process.DeleteConnected(router);
            router.Port1.Ip = ip1;
            router.Port1.Mask = port1MaskTextBox.Text;
            router.Port2.Ip = ip2;
            router.Port2.Mask = port2MaskTextBox.Text;

            port1IpTextBox.Text = router.Port1.Ip.ToString();
            port1MaskTextBox.Text = router.Port1.Mask.ToString();
            port2IpTextBox.Text = router.Port2.Ip.ToString();
            port2MaskTextBox.Text = router.Port2.Mask.ToString();
            //
            router.ArpTable.UpdatePortsIp(router);
            router.RoutingTable.SetConnected(router);

            
            RIPv2NetworksListView.Items.Clear();
            RIPv2NetworksListView.View = View.Details;
            var en = ripHandler.Process.GetAddedNetworks();
            RIPv2EntryOrdered res;
            while (en.TryDequeue(out res))
            {
                RIPv2NetworksListView.Items.Add(res.Id.ToString() + " " + res.Ip.ToString());
            }


            routerStatusBar.AppendText(DateTime.Now.ToString() + " IP address changed\n");
            router.Serialize();
        }

        private void arpTableRemove_Click(object sender, EventArgs e)
        {
            try
            {
                var x = arpListView.SelectedItems[0];
                var s = x.Text.Split(' ');
                router.ArpTable.Remove(new IpV4Address(s[0]));

            }
            catch (Exception)
            {

                
            }
            
        }

        private void arpListView_Enter(object sender, EventArgs e)
        {
            arpViewFocus = true;
        }

        private void arpListView_Leave(object sender, EventArgs e)
        {
            arpViewFocus = false;
        }

        private void arpAgingTimeSetButton_Click(object sender, EventArgs e)
        {
            uint x;
            try
            {
                x = UInt32.Parse(arpAgingTimerBox.Text);
            }
            catch (Exception)
            {
                arpAgingTimerBox.Text = router.ArpTable.AgingTime.ToString();
                return;
            }
            router.ArpTable.AgingTime = (int)x;
            tablesRich.AppendText("\n" + DateTime.Now + " Arp table remove timer set to " + x + "s");
            tablesRich.SelectionStart = tablesRich.Text.Length;
            tablesRich.ScrollToCaret();
        }

        private void setStaticRouteButton_Click(object sender, EventArgs e)
        {
            if (router.TryAddStaticRoute(staticRouteDestIpTextBox.Text, staticRouteMaskTextBox.Text, staticRouteInterfaceTextBox.Text, staticRouteNextHopTextBox.Text))
                tablesRich.AppendText("\n" + DateTime.Now + " static ip route set");
            else
                tablesRich.AppendText("\n" + DateTime.Now + " [FAIL] static ip route set");
            tablesRich.SelectionStart = tablesRich.Text.Length;
            tablesRich.ScrollToCaret();
        }

        private void routingTableListView_MouseEnter(object sender, EventArgs e)
        {
        }

        private void routingTableListView_MouseLeave(object sender, EventArgs e)
        {
        }

        private void RouterGui_SizeChanged(object sender, EventArgs e)
        {
            routingTableListView.Scrollable = true;
            routingTableListView.Scrollable = false;

        }

        private void routingTableListView_Enter(object sender, EventArgs e)
        {
            routingTableViewFocus = true;

        }

        private void routingTableListView_Leave(object sender, EventArgs e)
        {
            routingTableViewFocus = false;

        }

        private void staticRouteRemoveButton_Click(object sender, EventArgs e)
        {

            try
            {
                router.RoutingTable.Remove(routingTableListView.SelectedItems[0].Index);
            }
            catch (Exception) { }
            
        }

        private void port1RipStateButton_Click(object sender, EventArgs e)
        {
            if (ripHandler.Reciever1.Recieving)
            {
                ripHandler.Reciever1.Recieving = false;
                ripHandler.Sender1.Sending = false;
                label21.Text = "Off";
            }
            else if (!ripHandler.Reciever1.Recieving)
            {
                ripHandler.Reciever1.Recieving = true;
                ripHandler.Sender1.Sending = true;
                label21.Text = "On";
            }
        }

        private void port2RipStateButton_Click(object sender, EventArgs e)
        {
            if (ripHandler.Reciever2.Recieving)
            {
                ripHandler.Reciever2.Recieving = false;
                ripHandler.Sender2.Sending = false;
                label22.Text = "Off";
            }
            else if (!ripHandler.Reciever2.Recieving)
            {
                ripHandler.Reciever2.Recieving = true;
                ripHandler.Sender2.Sending = true;
                label22.Text = "On";
            }
        }

        private void SetRIPv2TimerButton_Click(object sender, EventArgs e)
        {
            uint u, i, h, f;
            try
            {
                u = UInt32.Parse(ripUpdateTimerTextBox.Text);
                i = UInt32.Parse(ripInvalidTimerTextBox.Text);
                h = UInt32.Parse(ripHolddownTimerTextBox.Text);
                f = UInt32.Parse(ripFlushTimerTextBox.Text);

            }
            catch (Exception)
            {
                ripUpdateTimerTextBox.Text = ripHandler.Timers.Update.ToString();
                ripInvalidTimerTextBox.Text = ripHandler.Timers.Invalid.ToString();
                ripHolddownTimerTextBox.Text = ripHandler.Timers.Holddown.ToString();
                ripFlushTimerTextBox.Text = ripHandler.Timers.Flush.ToString();
                return;
            }
            RIPv2Timer t = new RIPv2Timer();
            t.Update = (int)u;
            t.Invalid = (int)i;
            t.Holddown = (int)h;
            t.Flush = (int)f;

            ripHandler.Timers = t; 
        }

        private void RIPv2StateButton_Click(object sender, EventArgs e)
        {
            if (ripHandler.IsRIPv2Enabled)
            {
                ripHandler.IsRIPv2Enabled = false;
                label21.Text = "Off";
                label22.Text = "Off";
                label23.Text = "Off";
                ripHandler.Reciever2.Recieving = false;
                ripHandler.Sender2.Sending = false;
                ripHandler.Reciever1.Recieving = false;
                ripHandler.Sender1.Sending = false;
                RIPv2Port1StateButton.Enabled = false;
                RIPv2Port2StateButton.Enabled = false;
            }
            else
            {
                ripHandler.IsRIPv2Enabled = true;
                label23.Text = "On";
                RIPv2Port1StateButton.Enabled = true;
                RIPv2Port2StateButton.Enabled = true;
            }
        }

        private void AddRIPv2NetworkButton_Click(object sender, EventArgs e)
        {
            IpV4Address ip;
            try
            {
                ip = new IpV4Address(RIPv2NetworkTextBox.Text);
            }
            catch (Exception)
            {

                return;
            }
            if (true)
            {
                if (router.RoutingTable.Contains(ip) && !ripHandler.Process.IsInProcess(ip))
                {
                    ripHandler.Process.Add(new RIPv2Entry(ip, router.RoutingTable.GetMask(ip), 1));
                    RIPv2NetworksListView.Items.Clear();
                    RIPv2NetworksListView.View = View.Details;
                    var en = ripHandler.Process.GetAddedNetworks();
                    RIPv2EntryOrdered res;
                    while (en.TryDequeue(out res))
                    {
                        RIPv2NetworksListView.Items.Add(res.Id.ToString() + " " + res.Ip.ToString());
                    }
                }
                else if (ip == new IpV4Address("0.0.0.0") && !ripHandler.Process.IsInProcess(ip))
                {
                    ripHandler.Process.Add(new RIPv2Entry(ip, "0.0.0.0", 1));
                    RIPv2NetworksListView.Items.Clear();
                    RIPv2NetworksListView.View = View.Details;
                    var en = ripHandler.Process.GetAddedNetworks();
                    RIPv2EntryOrdered res;
                    while (en.TryDequeue(out res))
                    {
                        RIPv2NetworksListView.Items.Add(res.Id.ToString() + " " + res.Ip.ToString());
                    }
                }
            }
        }

        private void label36_Click(object sender, EventArgs e)
        {

        }

        private void SetDHCPPoolPort1_Click(object sender, EventArgs e)
        {
            dhcpPort1RichTextBox.Clear();
            var ip = new IpV4Address();
            try {
                ip = new IpV4Address(dhcpPoolPort1IPTextBox.Text); }
            catch (Exception) { 
                return; }
            string mask = dhcpPoolPort1MaskTextBox.Text;
            router.Port1.DhcpServer.DhcpIpPool.SetPool(ip, mask);
        }

        private void label38_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            router.Port1.DhcpServer.IsEnabled = !router.Port1.DhcpServer.IsEnabled;
            if (router.Port1.DhcpServer.IsEnabled)
                label35.Text = "DHCP server on";
            else
                label35.Text = "DHCP server off";
        }

        private void button15_Click(object sender, EventArgs e)
        {
            router.Port2.DhcpServer.IsEnabled = !router.Port2.DhcpServer.IsEnabled;
            if (router.Port2.DhcpServer.IsEnabled)
                label39.Text = "DHCP server on";
            else
                label39.Text = "DHCP server off";
        }

        private void button16_Click(object sender, EventArgs e)
        {
            dhcpPort2RichTextBox.Clear();
            var ip = new IpV4Address();
            try
            {
                ip = new IpV4Address(dhcpPoolPort2IPTextBox.Text);
            }
            catch (Exception)
            {
                return;
            }
            string mask = dhcpPoolPort2MaskTextBox.Text;
            router.Port2.DhcpServer.DhcpIpPool.SetPool(ip, mask);
        }

        //dhcp timer port 1
        private void button13_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                router.Port1.DhcpServer.IsLeasing = false;
                return;
            }
            else
            {
                uint leaseTime;
                try
                {
                    leaseTime = UInt32.Parse(textBox3.Text);
                }
                catch (Exception)
                {

                    return;
                }
                router.Port1.DhcpServer.IsLeasing = true;
                router.Port1.DhcpServer.LeaseTime = leaseTime;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox3.Enabled = false;
                textBox3.Text = "";
            }
            else
                textBox3.Enabled = true;

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                textBox8.Enabled = false;
                textBox8.Text = "";
            }
            else
                textBox8.Enabled = true;
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                router.Port2.DhcpServer.IsLeasing = false;
                return;
            }
            else
            {
                uint leaseTime;
                try
                {
                    leaseTime = UInt32.Parse(textBox8.Text);
                }
                catch (Exception)
                {

                    return;
                }
                router.Port2.DhcpServer.IsLeasing = true;
                router.Port2.DhcpServer.LeaseTime = leaseTime;
            }

        }

        //manual alloc port1
        private void button14_Click(object sender, EventArgs e)
        {
            var ip = new IpV4Address();
            var mac = new MacAddress();
            try
            {
                mac = new MacAddress(textBox5.Text);
                ip = new IpV4Address(textBox6.Text);
            }
            catch (Exception)
            {
                return;
            }
            try
            {
                router.Port1.DhcpServer.DhcpIpPool.ManualAlloc(mac, ip);
            }
            catch (Exception)
            {

                MessageBox.Show("IP is already allocated", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
            }
        }

        //manual alloc port2
        private void button18_Click(object sender, EventArgs e)
        {
            var ip = new IpV4Address();
            var mac = new MacAddress();
            try
            {
                mac = new MacAddress(textBox9.Text);
                ip = new IpV4Address(textBox10.Text);
            }
            catch (Exception)
            {
                return;
            }
            try
            {
                router.Port2.DhcpServer.DhcpIpPool.ManualAlloc(mac, ip);
            }
            catch (Exception)
            {

                MessageBox.Show("IP is already allocated", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }

        private void RemoveRIPv2NetworkButton_Click(object sender, EventArgs e)
        {
            try
            {
                var x = RIPv2NetworksListView.SelectedItems[0];
                var s = x.Text.Split(' ');
                ripHandler.Process.Delete(Int32.Parse(s[0]));
                RIPv2NetworksListView.Items.Clear();
                RIPv2NetworksListView.View = View.Details;
                var en = ripHandler.Process.GetAddedNetworks();
                RIPv2EntryOrdered res;
                while (en.TryDequeue(out res))
                {
                    RIPv2NetworksListView.Items.Add(res.Id.ToString() + " " + res.Ip.ToString());
                }
                //router.ArpTable.Remove(new IpV4Address(s[0]));
            }
            catch (Exception)
            { }
        }

        private void pingButton_Click(object sender, EventArgs e)
        {
            try
            {
                new IpV4Address(IPtoPingTextBox.Text);
            }
            catch (Exception)
            {
                routerStatusBar.AppendText(DateTime.Now.ToString() + " Invalid ping IP Adress\n");
                return;
            }

            pingButton.Enabled = false;
            new Thread(() =>
            {
                router.Ping(new IpV4Address(IPtoPingTextBox.Text), pingProgressBar, pingTextBox, label31, pingButton);
            }).Start();
        }

        private void clearStatsButton_Click(object sender, EventArgs e)
        {
            if (router != null)
            {
                in1.ResetStats();
                in2.ResetStats();
                out1.ResetStats();
                out2.ResetStats();
            }
        }
    }
}
