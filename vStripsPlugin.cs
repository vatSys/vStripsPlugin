using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using vatsys;
using vatsys.Plugin;
using System.Collections.Concurrent;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace vStripsPlugin
{
    [Export(typeof(IPlugin))]
    public class vStripsPlugin : IPlugin
    {
        /// Plugin Name
        public string Name { get => "vStrips Connector"; }

        private static SetupWindow setupWindow;

        private CustomToolStripMenuItem setupWindowMenu;

        public vStripsPlugin()
        {
            vStripsConnector.Start();

            setupWindowMenu = new CustomToolStripMenuItem(CustomToolStripMenuItemWindowType.Main, CustomToolStripMenuItemCategory.Settings, new ToolStripMenuItem("vStrips INTAS"));
            setupWindowMenu.Item.Click += SetupWindowMenu_Click;
            MMI.AddCustomMenuItem(setupWindowMenu);
            
            MMI.SelectedTrackChanged += MMI_SelectedTrackChanged;                                           // Subscribe to Selected Track change event JMG

        }

        private void SetupWindowMenu_Click(object sender, EventArgs e)
        {
            DoShowSetupWindow();
        }
        
       

        public void OnFDRUpdate(FDP2.FDR updated)
        {
            if (FDP2.GetFDRIndex(updated.Callsign) == -1)//removed
            {
                updated.PropertyChanged -= FDR_PropertyChanged;
                vStripsConnector.RemoveFDR(updated);
            }
            else
            {
                updated.PropertyChanged -= FDR_PropertyChanged;
                updated.PropertyChanged += FDR_PropertyChanged;                
            }
        }

        private void FDR_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            vStripsConnector.UpdateFDR((FDP2.FDR)sender);
        }

        /*
         * When a track is selected in vatSys, send the selected track to vStrips
         */
        private void MMI_SelectedTrackChanged(object sender, EventArgs e)
        {
            if (MMI.SelectedTrack != null)
            {                
                vStripsConnector.SelectStrip(MMI.SelectedTrack.GetFDR().Callsign);
            }
        }


        public void OnRadarTrackUpdate(RDP.RadarTrack updated)
        {

        }

        public CustomLabelItem GetCustomLabelItem(string itemType, Track track, FDP2.FDR flightDataRecord, RDP.RadarTrack radarTrack)
        {
            return null;
        }

        public CustomColour SelectASDTrackColour(Track track)
        {
            return null;
        }

        public CustomColour SelectGroundTrackColour(Track track)
        {
            return null;
        }

        public static void ShowSetupWindow()
        {
            MMI.InvokeOnGUI((System.Windows.Forms.MethodInvoker)delegate() { DoShowSetupWindow(); });
        }

        private static void DoShowSetupWindow()
        {
            if (setupWindow == null || setupWindow.IsDisposed)
            {
                setupWindow = new SetupWindow();
            }
            else if (setupWindow.Visible)
                return;

            setupWindow.ShowDialog();
        }
    }
}
