using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using vatsys;

namespace vStripsPlugin
{
    public partial class Form1 : BaseForm
    {
        public Form1()
        {
            InitializeComponent();
            vStripsConnector.PacketReceived += VStripsConnector_PacketReceived;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            vStripsConnector.Start();
        }

        private void VStripsConnector_PacketReceived(object sender, vStripsConnector.PacketReceivedEventArgs e)
        {
            if (richTextBox1.InvokeRequired)
                richTextBox1.BeginInvoke((MethodInvoker)delegate { AddPacketToBox(e.Packet); });
            else
                AddPacketToBox(e.Packet);
        }

        private void AddPacketToBox(string packet)
        {
            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.AppendText(packet);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            vStripsConnector.Runways = textBox1.Text + ":" + textBox2.Text;
        }
    }
}
