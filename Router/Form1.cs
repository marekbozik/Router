using PcapDotNet.Core;
using PcapDotNet.Packets.IpV4;
using System;
using System.Threading;
using System.Windows.Forms;


namespace Router
{
    public partial class RouterGui : Form
    {
        private Router router;
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
                port1IpTextBox.Text = router.Port1.IpAddress1.ToString();
                port1MaskTextBox.Text = router.Port1.Mask1.ToString();
                port2IpTextBox.Text = router.Port2.IpAddress1.ToString();
                port2MaskTextBox.Text = router.Port2.Mask1.ToString();
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
            
            InStats();
            
        }

        private void InStats()
        {
            if (router != null)
            {
                Stats s1 = new Stats();
                Stats s2 = new Stats();
                new Thread(() => { new Sniffer(router.Port1.DeviceInterface1, s1).Sniffing(); }).Start();
                new Thread(() => { new Sniffer(router.Port2.DeviceInterface1, s2).Sniffing(); }).Start();
                new Thread(() =>
                {
                    while (true)
                    {
                        System.Threading.Thread.Sleep(1000);
                        richTextBox1.BeginInvoke(new Action(() =>
                        {
                            richTextBox2.Clear();
                            richTextBox2.AppendText(s1.GetStats());
                        }));
                        richTextBox1.BeginInvoke(new Action(() =>
                        {
                            richTextBox3.Clear();
                            richTextBox3.AppendText(s2.GetStats());
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
                    RouterPort rp1 = new RouterPort(devs[indx1], new IpV4Address("10.0.0.1"), "255.255.255.0");
                    RouterPort rp2 = new RouterPort(devs[indx2], new IpV4Address("10.0.0.2"), "255.255.255.0");
                    router = new Router(rp1, rp2);
                }
                catch (Exception)
                {
                    router = null;
                    MessageBox.Show("Error occured", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                router.Serialize();
                port1IpTextBox.Text = router.Port1.IpAddress1.ToString();
                port1MaskTextBox.Text = router.Port1.Mask1.ToString();
                port2IpTextBox.Text = router.Port2.IpAddress1.ToString();
                port2MaskTextBox.Text = router.Port2.Mask1.ToString();
                interfaceInfoRich.AppendText(DateTime.Now.ToString() + " Settings changed \n");
                tabs.TabPages.Remove(appSettingTab);
                tabs.TabPages.Add(routerTab);
                tabs.TabPages.Add(appSettingTab);
                InStats();
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
                router.Port1 = new RouterPort(devs[indx1], router.Port1.IpAddress1, router.Port1.Mask1);
                router.Port2 = new RouterPort(devs[indx2], router.Port2.IpAddress1, router.Port2.Mask1);

                router.Serialize();
                port1IpTextBox.Text = router.Port1.IpAddress1.ToString();
                port1MaskTextBox.Text = router.Port1.Mask1.ToString();
                port2IpTextBox.Text = router.Port2.IpAddress1.ToString();
                port2MaskTextBox.Text = router.Port2.Mask1.ToString();
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
            router.Port1.IpAddress1 = ip1;
            router.Port1.Mask1 = port1MaskTextBox.Text;
            router.Port2.IpAddress1 = ip2;
            router.Port2.Mask1 = port2MaskTextBox.Text;

            port1IpTextBox.Text = router.Port1.IpAddress1.ToString();
            port1MaskTextBox.Text = router.Port1.Mask1.ToString();
            port2IpTextBox.Text = router.Port2.IpAddress1.ToString();
            port2MaskTextBox.Text = router.Port2.Mask1.ToString();
            routerStatusBar.AppendText(DateTime.Now.ToString() + " IP address changed\n");
            router.Serialize();
        }
    }
}
