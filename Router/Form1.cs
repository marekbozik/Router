using PcapDotNet.Core;
using PcapDotNet.Packets.IpV4;
using System;
using System.Threading;
using System.Windows.Forms;


namespace Router
{
    public partial class Form1 : Form
    {
        private Router router;
        public Form1()
        {
            InitializeComponent();
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

            new Thread(() =>
            {
                var devs = Router.GetPacketDevices();
                for (int i = 0; i != devs.Count; ++i)
                {
                    LivePacketDevice device = devs[i];

                    var x  = richTextBox1.BeginInvoke(new Action(() =>
                    {
                        richTextBox1.AppendText((i + 1) + ". " + device.Name);
                    }));

                    while (!x.IsCompleted) ;
                    
                    if (device.Description != null)
                    {
                        x = richTextBox1.BeginInvoke(new Action(() =>
                        {
                            richTextBox1.AppendText(" (" + device.Description + ")\n");
                        }));
                        while (!x.IsCompleted) ;
                    }

                    else
                    {
                        x = richTextBox1.BeginInvoke(new Action(() =>
                        {
                            richTextBox1.AppendText(" (No description available)");
                            richTextBox1.AppendText("\n");
                        }));
                        while (!x.IsCompleted) ;
                    }
                }
            }).Start();

            
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
            routerStatusBar.AppendText(DateTime.Now.ToString() + " IP address changed\n");

            port1IpTextBox.Text = router.Port1.IpAddress1.ToString();
            port1MaskTextBox.Text = router.Port1.Mask1.ToString();
            port2IpTextBox.Text = router.Port2.IpAddress1.ToString();
            port2MaskTextBox.Text = router.Port2.Mask1.ToString();
        }
    }
}
