
namespace Router
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.tabs = new System.Windows.Forms.TabControl();
            this.routerTab = new System.Windows.Forms.TabPage();
            this.button2 = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.port1IpTextBox = new System.Windows.Forms.TextBox();
            this.port2IpTextBox = new System.Windows.Forms.TextBox();
            this.port1MaskTextBox = new System.Windows.Forms.TextBox();
            this.port2MaskTextBox = new System.Windows.Forms.TextBox();
            this.appSettingTab = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.intPort1 = new System.Windows.Forms.TextBox();
            this.intPort2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.interfaceInfoRich = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.routerStatusBar = new System.Windows.Forms.RichTextBox();
            this.tabs.SuspendLayout();
            this.routerTab.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.appSettingTab.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(6, 54);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(331, 306);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.routerTab);
            this.tabs.Controls.Add(this.appSettingTab);
            this.tabs.Location = new System.Drawing.Point(12, 12);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(751, 394);
            this.tabs.TabIndex = 1;
            // 
            // routerTab
            // 
            this.routerTab.Controls.Add(this.routerStatusBar);
            this.routerTab.Controls.Add(this.button2);
            this.routerTab.Controls.Add(this.tableLayoutPanel2);
            this.routerTab.Location = new System.Drawing.Point(4, 24);
            this.routerTab.Name = "routerTab";
            this.routerTab.Padding = new System.Windows.Forms.Padding(3);
            this.routerTab.Size = new System.Drawing.Size(743, 366);
            this.routerTab.TabIndex = 0;
            this.routerTab.Text = "Router";
            this.routerTab.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(488, 175);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(183, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Set IP address";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ipAddSetButton_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Controls.Add(this.label5, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.label4, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label6, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label7, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.port1IpTextBox, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.port2IpTextBox, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.port1MaskTextBox, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.port2MaskTextBox, 2, 2);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(392, 41);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(279, 128);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(213, 27);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 15);
            this.label5.TabIndex = 1;
            this.label5.Text = "Port 2";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(120, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "Port 1";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(73, 55);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(17, 15);
            this.label6.TabIndex = 2;
            this.label6.Text = "IP";
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(55, 98);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(35, 15);
            this.label7.TabIndex = 3;
            this.label7.Text = "Mask";
            // 
            // port1IpTextBox
            // 
            this.port1IpTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.port1IpTextBox.Location = new System.Drawing.Point(96, 51);
            this.port1IpTextBox.Name = "port1IpTextBox";
            this.port1IpTextBox.Size = new System.Drawing.Size(87, 23);
            this.port1IpTextBox.TabIndex = 4;
            // 
            // port2IpTextBox
            // 
            this.port2IpTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.port2IpTextBox.Location = new System.Drawing.Point(189, 51);
            this.port2IpTextBox.Name = "port2IpTextBox";
            this.port2IpTextBox.Size = new System.Drawing.Size(87, 23);
            this.port2IpTextBox.TabIndex = 5;
            // 
            // port1MaskTextBox
            // 
            this.port1MaskTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.port1MaskTextBox.Location = new System.Drawing.Point(96, 94);
            this.port1MaskTextBox.Name = "port1MaskTextBox";
            this.port1MaskTextBox.Size = new System.Drawing.Size(87, 23);
            this.port1MaskTextBox.TabIndex = 6;
            // 
            // port2MaskTextBox
            // 
            this.port2MaskTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.port2MaskTextBox.Location = new System.Drawing.Point(189, 94);
            this.port2MaskTextBox.Name = "port2MaskTextBox";
            this.port2MaskTextBox.Size = new System.Drawing.Size(87, 23);
            this.port2MaskTextBox.TabIndex = 7;
            // 
            // appSettingTab
            // 
            this.appSettingTab.Controls.Add(this.tableLayoutPanel1);
            this.appSettingTab.Controls.Add(this.label1);
            this.appSettingTab.Controls.Add(this.richTextBox1);
            this.appSettingTab.Location = new System.Drawing.Point(4, 24);
            this.appSettingTab.Name = "appSettingTab";
            this.appSettingTab.Padding = new System.Windows.Forms.Padding(3);
            this.appSettingTab.Size = new System.Drawing.Size(743, 366);
            this.appSettingTab.TabIndex = 1;
            this.appSettingTab.Text = "App Settings";
            this.appSettingTab.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.intPort1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.intPort2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.button1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.interfaceInfoRich, 0, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(352, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22.47706F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22.47706F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.6422F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34.40367F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(335, 182);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // intPort1
            // 
            this.intPort1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.intPort1.Location = new System.Drawing.Point(3, 48);
            this.intPort1.Name = "intPort1";
            this.intPort1.Size = new System.Drawing.Size(161, 23);
            this.intPort1.TabIndex = 2;
            // 
            // intPort2
            // 
            this.intPort2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.intPort2.Location = new System.Drawing.Point(170, 48);
            this.intPort2.Name = "intPort2";
            this.intPort2.Size = new System.Drawing.Size(162, 23);
            this.intPort2.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(64, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Port 1";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(232, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Port 2";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.button1, 2);
            this.button1.Location = new System.Drawing.Point(3, 83);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(329, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.interfaceSaveButton_Click);
            // 
            // interfaceInfoRich
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.interfaceInfoRich, 2);
            this.interfaceInfoRich.Location = new System.Drawing.Point(3, 120);
            this.interfaceInfoRich.Name = "interfaceInfoRich";
            this.interfaceInfoRich.Size = new System.Drawing.Size(329, 59);
            this.interfaceInfoRich.TabIndex = 7;
            this.interfaceInfoRich.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(3, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "Interfaces";
            // 
            // routerStatusBar
            // 
            this.routerStatusBar.Location = new System.Drawing.Point(0, 264);
            this.routerStatusBar.Name = "routerStatusBar";
            this.routerStatusBar.Size = new System.Drawing.Size(740, 96);
            this.routerStatusBar.TabIndex = 2;
            this.routerStatusBar.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(775, 450);
            this.Controls.Add(this.tabs);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tabs.ResumeLayout(false);
            this.routerTab.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.appSettingTab.ResumeLayout(false);
            this.appSettingTab.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage routerTab;
        private System.Windows.Forms.TabPage appSettingTab;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox intPort1;
        private System.Windows.Forms.TextBox intPort2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox port1IpTextBox;
        private System.Windows.Forms.TextBox port2IpTextBox;
        private System.Windows.Forms.TextBox port1MaskTextBox;
        private System.Windows.Forms.TextBox port2MaskTextBox;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.RichTextBox interfaceInfoRich;
        private System.Windows.Forms.RichTextBox routerStatusBar;
    }
}

