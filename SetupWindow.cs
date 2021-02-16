using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using vatsys;

namespace vStripsPlugin
{
    public partial class SetupWindow : BaseForm
    {
        private bool closeDialog = false;

        public SetupWindow()
        {
            InitializeComponent();

            arrivalView.BackColor = Colours.GetColour(Colours.Identities.WindowBackgound);
            arrivalView.ForeColor = Colours.GetColour(Colours.Identities.InteractiveText);
            departureView.BackColor = Colours.GetColour(Colours.Identities.WindowBackgound);
            departureView.ForeColor = Colours.GetColour(Colours.Identities.InteractiveText);

            // Retrieve Properties Information
            //https://www.daveoncsharp.com/2009/07/using-the-settings-file-in-csharp/
            t_vStripsHostIP.Text = Properties.Settings.Default.vStripsHost;

            if (vStripsConnector.Airport != null)
            {
                airportLabel.Enabled = true;
                airportLabel.Text = vStripsConnector.Airport.ICAOName;

                var rwys = Airspace2.GetRunways(vStripsConnector.Airport.ICAOName);
                if(rwys != null)
                {
                    string[] split = vStripsConnector.Runways.Split(':');
                    string[] arwys = split[0].Split('/');
                    string[] drwys = split[1].Split('/');

                    foreach (var rwy in rwys.Select(r => r.Name).OrderBy(n => n))
                    {
                        var anode = arrivalView.Nodes.Add(rwy);
                        if (arwys.Contains(rwy))
                            anode.Checked = true;
                        var dnode = departureView.Nodes.Add(rwy);
                        if (drwys.Contains(rwy))
                            dnode.Checked = true;
                    }

                    arrivalView.Enabled = true;
                    departureView.Enabled = true;
                }
            }
        }

        /**
         * On key up, checks if Enter was pressed. If so, store and update the IP
         */
        private void HostIP_onKeyup(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Return)) 
            {
                b_restartPlugin.Focus();
                updateHostIP(t_vStripsHostIP.Text);                                
            }
        }

        private void storeHostIPChange(object sender, EventArgs e)
        {
            updateHostIP(t_vStripsHostIP.Text);
        }

        /**
         *  If the Host IP has changed, try to convert the string to an IP address
         *  If successful, update the Preferences and update the vStripConnector HostIP
         */

        private void updateHostIP(string hostip)
        {
            IPAddress ip;
            bool result = IPAddress.TryParse(hostip, out ip);
            if (result == true)
            {
                Properties.Settings.Default.vStripsHost = hostip;                                   // update settings
                vStripsConnector.HostIP = ip;                                                       // update hostIP
                Properties.Settings.Default.Save();                                                 // save settings
            }
            else                                                                                    // IP invalid, restore old setting
            {
                // Add error notification?
                t_vStripsHostIP.Text = Properties.Settings.Default.vStripsHost;                     // put old value back
            }
        }

        private void storeButton_Click(object sender, EventArgs e)
        {
            List<string> arwys = new List<string>();
            List<string> drwys = new List<string>();
            foreach (TreeNode node in arrivalView.Nodes)
            {
                if (node.Checked)
                    arwys.Add(node.Text);
            }
            foreach (TreeNode node in departureView.Nodes)
            {
                if (node.Checked)
                    drwys.Add(node.Text);
            }

            vStripsConnector.Runways = string.Join(":", string.Join("/", arwys), string.Join("/", drwys));

            closeDialog = true;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void restartButton_Click(object sender, EventArgs e)
        {
            vStripsConnector.Restart();
        }

        private void SetupWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !closeDialog)
                vStripsConnector.Runways = vStripsConnector.Runways;//resets

            this.Dispose();
        }

        private void View_AfterCheck(object sender, TreeViewEventArgs e)
        {
            bool pass = false;
            foreach(TreeNode node in arrivalView.Nodes)
            {
                if(node.Checked)
                {
                    pass = true;
                    break;
                }
            }

            if(pass)
            {
                pass = false;

                foreach (TreeNode node in departureView.Nodes)
                {
                    if (node.Checked)
                    {
                        pass = true;
                        break;
                    }
                }

                storeButton.Enabled = pass;
                return;
            }

            storeButton.Enabled = false;
        }

        private void textLabel2_Click(object sender, EventArgs e)
        {

        }
    }
}
