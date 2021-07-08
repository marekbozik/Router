using PcapDotNet.Core;
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
        private Stats in1, in2;
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

            new Thread(() => { router.Forward(router.Port1); }).Start();
            new Thread(() => { router.Forward(router.Port2); }).Start();


        }

        private void Stats()
        {
            if (router != null)
            {
                Stats s1 = new Stats(router);
                in1 = s1;
                Stats s2 = new Stats(router);
                in2 = s2;
                new Thread(() => { new Sniffer(router.Port1.DeviceInterface, s1).Sniffing(); }).Start();
                new Thread(() => { new Sniffer(router.Port2.DeviceInterface, s2).Sniffing(); }).Start();
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
                            richTextBox5.AppendText(router.Out1.GetStats());
                        }));
                        richTextBox4.BeginInvoke(new Action(() =>
                        {
                            richTextBox4.Clear();
                            richTextBox4.AppendText(router.Out2.GetStats());
                        }));
                        arpListView.BeginInvoke(new Action(() =>
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
                            try
                            {
                                arpListView.Items[q].Selected = true;
                                arpListView.Select();
                            }
                            catch (Exception) { }

                        }
                        ));

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

            router.Port1.Ip = ip1;
            router.Port1.Mask = port1MaskTextBox.Text;
            router.Port2.Ip = ip2;
            router.Port2.Mask = port2MaskTextBox.Text;

            port1IpTextBox.Text = router.Port1.Ip.ToString();
            port1MaskTextBox.Text = router.Port1.Mask.ToString();
            port2IpTextBox.Text = router.Port2.Ip.ToString();
            port2MaskTextBox.Text = router.Port2.Mask.ToString();
            routerStatusBar.AppendText(DateTime.Now.ToString() + " IP address changed\n");
            router.Serialize();
        }

        private void clearStatsButton_Click(object sender, EventArgs e)
        {
            if (router != null)
            {
                in1.ResetStats();
                in2.ResetStats();
                router.Out1.ResetStats();
                router.Out2.ResetStats();
            }
        }
    }
}
