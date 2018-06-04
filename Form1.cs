using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DirectInput;
using System.Net.NetworkInformation;
using System.Threading;
using System.Diagnostics;

namespace RouterQuery
{
    public partial class Form1 : Form
    {
        private Pinger pingping = new Pinger();
        public Form1()
        {
            InitializeComponent();

            pingping.ResultsReady += Pingping_ResultsReady;
        }

        private void Pingping_ResultsReady(object sender, EventArgs e)
        {
            MessageBox.Show("Results: " + pingping.Devices.Count);
            ListViewItem listItem;
            foreach(ClientData data in pingping.Devices)
            {
                listItem = new ListViewItem(new string[] { data.IP, data.Name, data.MacAddr });
                lstLocal.Items.Add(listItem);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pingping.PingAll();
            
        }
    }
}
