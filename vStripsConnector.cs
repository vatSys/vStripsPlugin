using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using vatsys;
using vatsys.Plugin;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Linq;

namespace vStripsPlugin
{
    internal class vStripsConnector
    {
        public const string DEFAULT_RUNWAYS = "00:00";

        private IPEndPoint vStripsHost = new IPEndPoint(IPAddress.Loopback, VSTRIPS_PORT);
        private UdpClient vStripsSocket;
        private const int VSTRIPS_PORT = 60301;
        private const string MIN_VSTRIPS_PLUGIN_VERSION = "1.18";
        private CancellationTokenSource cancellationToken;
        
        private ConcurrentDictionary<string, int> vStripsAssignedHeadings = new ConcurrentDictionary<string, int>();
        private List<string> vStripsSentATCCallsigns = new List<string>();
        private bool connected = false;
        private bool setRunways = false;

        private static vStripsConnector Instance;

        public static event EventHandler<PacketReceivedEventArgs> PacketReceived;

        private static string runways = DEFAULT_RUNWAYS;
        public static string Runways
        {
            get { return runways; }
            set { runways = value; Instance.setRunways = true; Instance.SendRunways(); }
        }

        public static Airspace2.Airport Airport = null;

        private vStripsConnector()
        {
            vStripsSocket = new UdpClient();
            cancellationToken = new CancellationTokenSource();
        }

        private void Network_OnlineATCChanged(object sender, Network.ATCUpdateEventArgs e)
        {
            if (e.UpdatedATC == null)
                SyncATCLists();
            else
            {
                bool exists = Network.GetOnlineATCs.Contains(e.UpdatedATC);
                bool sent = vStripsSentATCCallsigns.Contains(e.UpdatedATC.Callsign);

                if (exists)
                    SendATCOnline(e.UpdatedATC);
                else if (!exists && sent)
                    SendATCOffline(e.UpdatedATC.Callsign);
            }
        }

        private void SyncATCLists()
        {
            var online = Network.GetOnlineATCs;

            foreach (var atc in online.Where(a=>!vStripsSentATCCallsigns.Contains(a.Callsign)))
            {
                SendATCOnline(atc);
                vStripsSentATCCallsigns.Add(atc.Callsign);
            }
            foreach (var atc in vStripsSentATCCallsigns.Except(online.Select(a => a.Callsign)))
                SendATCOffline(atc);
        }

        private void Network_PrimaryFrequencyChanged(object sender, EventArgs e)
        {
            SendControllerInfo();
        }


        public static void Restart()
        {
            Stop();
            Instance = null;
            Instance = new vStripsConnector();
            Instance.Connect();
        }

        public static void Start()
        {
            Instance = new vStripsConnector();
            Instance.Connect();            
        }

        public static void Stop()
        {
            Instance?.Disconnect();
            Instance?.vStripsAssignedHeadings.Clear();
        }

        public static void UpdateFDR(FDP2.FDR fdr)
        {
            if(Instance?.connected == true)
                Instance.SendAircraftMetadata(fdr);
        }

        public static void RemoveFDR(FDP2.FDR fdr)
        {
            if (Instance?.connected == true)
            {
                Instance.SendDeleteAircraft(fdr);
                Instance.vStripsAssignedHeadings.TryRemove(fdr.Callsign, out _);
            }
        }

        public static void SelectStrip(String callsign)
        {
            if (Instance?.connected == true)
            {
                Instance.SendPacket(">" + callsign);
            }
        }


        private void Connect()
        {
            vStripsSocket.Connect(vStripsHost);
            Task.Run(() => ReceiveData());
            Task.Run(() => PollForConnection());
        }

        private void PollForConnection()
        {
            while(!connected && !cancellationToken.IsCancellationRequested)
            {
                SendVersion();
                Thread.Sleep(1000);
            }
        }

        private void SendVersion()
        {
            SendPacket("h" + MIN_VSTRIPS_PLUGIN_VERSION);
        }

        private void OnConnected()
        {
            connected = true;
            Network.PrimaryFrequencyChanged += Network_PrimaryFrequencyChanged;
            Network.OnlineATCChanged += Network_OnlineATCChanged;
            SyncATCLists();
        }

        private void SendControllerInfo()
        {
            if (!Network.IsConnected)
                return;

            SendVersion();

            //CALLSIGN:NAME:FREQ
            string pack = $"U{Network.Me.Callsign}:{Network.Me.RealName}:{ConvertToFreqString(Network.Me.Frequency)}";
            SendPacket(pack);

            string trans = $"T{RDP.TRANSITION_ALTITUDE}";
            SendPacket(trans);
        }
        



        private void SendATCOnline(NetworkATC atc)
        {
            SendPacket($"C{atc.Callsign}:{atc.RealName}:{ConvertToFreqString(atc.Frequency)}");
        }

        private void SendATCOffline(string callsign)
        {
            SendPacket($"c{callsign}");
        }

        private void SendDeleteAircraft(FDP2.FDR fdr)
        {
            SendPacket($"D{fdr.Callsign}");
        }

        private void SendRemarks(FDP2.FDR fdr)
        {
            string rmks = $"P{fdr.Callsign}:{fdr.Remarks}";
            SendPacket(rmks);
        }


        private void SendAircraftMetadata(FDP2.FDR fdr)
        {
            #region MetaDataPacket
            //string[] strArray = msg.Split(':');
            //this.callsign = strArray[0];
            //this.upDown = this.callsign[0];
            //this.callsign = this.callsign.Substring(1);
            //this.origin = strArray[1];
            //this.dest = strArray[2];
            //this.squawk = string.IsNullOrEmpty(strArray[3]) ? (string)null : strArray[3];
            //this.sentSquawk = string.IsNullOrEmpty(strArray[4]) ? (string)null : strArray[4];
            //this.groundState = strArray[5][0];
            //this.altitude = int.Parse(strArray[6]);
            //this.heading = int.Parse(strArray[7]);
            //this.gone = int.Parse(strArray[8]);
            //this.toGo = int.Parse(strArray[9]);
            //this.aircraftType = strArray[10];
            //this.sidName = string.IsNullOrEmpty(strArray[11]) ? (string)null : strArray[11];
            //this.runway = string.IsNullOrEmpty(strArray[12]) ? (string)null : strArray[12];
            //this.fpRules = strArray[13][0] == 'V' ? FltRules.VFR : FltRules.IFR;
            //this.rfl = int.Parse(strArray[14]);
            //this.estDepTime = strArray[15];
            //this.spd = strArray[16];
            //this.groundspeed = int.Parse(strArray[17]);
            //this.route = strArray[18];
            //this.distanceFromMe = int.Parse(strArray[19]);
            //this.commsType = strArray[20][0];
            //try
            //{
            //    this.lat = double.Parse(strArray[21]);
            //    this.lon = double.Parse(strArray[22]);
            //}
            //catch
            //{
            //}
            //if (!(Presenter.CurrentAirfield == "EGLL") && !(Presenter.CurrentAirfield == "EGCC"))
            //    return;
            //this.scratchpad = strArray[23];
            #endregion

            if (fdr == null)
                return;

            if (fdr.PredictedPosition.Location == null)
                return;

            RDP.RadarTrack radTrack = fdr.CoupledTrack;
            if (radTrack == null)
                radTrack = RDP.RadarTracks.FirstOrDefault(r => r.ASMGCSCoupledFDR == fdr);

            string gone = "0"; 
            string togo = "0";
            string tome = "0";
            if(fdr.ParsedRoute?.Count > 0)
            {
                gone = Conversions.CalculateDistance(fdr.ParsedRoute.First().Intersection.LatLong, fdr.PredictedPosition.Location).ToString("0");
                togo = Conversions.CalculateDistance(fdr.ParsedRoute.Last().Intersection.LatLong, fdr.PredictedPosition.Location).ToString("0");
                tome = Conversions.CalculateDistance(Airport == null ? Network.Me.Position : Airport.LatLong, fdr.PredictedPosition.Location).ToString("0");
            }

            string upDown = "U";
            if(radTrack?.OnGround == true || fdr.ATD == DateTime.MaxValue)
                upDown = "D";

            string gndState = " ";
            if(fdr.DepAirport == Airport?.ICAOName && fdr.State == FDP2.FDR.FDRStates.STATE_COORDINATED)
            {
                gndState = "T";
            }

            int ahdg = 0;
            vStripsAssignedHeadings.TryGetValue(fdr.Callsign, out ahdg);

            string meta = string.Join(":", 
                upDown + fdr.Callsign, 
                fdr.DepAirport, 
                fdr.DesAirport, 
                fdr.AssignedSSRCode == -1 ? "" : Convert.ToString(fdr.AssignedSSRCode, 8),
                radTrack != null ? Convert.ToString(radTrack.ActualAircraft.TransponderCode, 8): "",
                gndState,
                fdr.CFLUpper <= FDP2.FDR.LEVEL_VSA ? 0 : fdr.CFLUpper,
                ahdg,
                gone,
                togo,
                fdr.AircraftType + "/" + fdr.AircraftWake,
                (fdr.DepAirport == Airport?.ICAOName ? fdr.SIDSTARString:""),                       //  modified JMG - Inhibit STAR population for airborne
                fdr.RunwayString, 
                fdr.FlightRules, 
                fdr.RFL, 
                fdr.ETD.ToString("HHmm"), 
                0, 
                fdr.PredictedPosition.Groundspeed.ToString("0"), 
                fdr.Route, 
                tome, 
                fdr.TextOnly ? "T" : fdr.ReceiveOnly ? "R" : "V", 
                fdr.PredictedPosition.Location.Latitude, 
                fdr.PredictedPosition.Location.Longitude, 
                fdr.LabelOpData);

            SendPacket("M" + meta);
            SendRemarks(fdr);
        }

        private void SendPacket(string packet)
        {
            var data = Encoding.ASCII.GetBytes(packet);
            vStripsSocket.SendAsync(data, data.Length);
        }

        private void Disconnect()
        {
            cancellationToken.Cancel();
            vStripsSocket.Close();
            vStripsSocket.Dispose();
        }

        private void ReceiveData()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    byte[] data = vStripsSocket.Receive(ref vStripsHost);
                    string packet = Encoding.ASCII.GetString(data);
                    ProcessPacket(packet);
                }
                catch (SocketException) 
                {
                    Thread.Sleep(1000);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void SendRunways()                                                                  // modified JMG to force runways to none and add send QNH
        {
            if (connected)
            { 
                SendPacket($"R00:00");
                //SendPacket($"R{Runways}"); //Old                                                              
            }
        }

        private void ProcessPacket(string packet)
        {
            if (packet.Length == 0)
                return;

            string msg = packet.Substring(1);
            string[] msg_fields = msg.Split(':');

            FDP2.FDR fdr = null;
            if (msg_fields.Length > 0)
            {
                fdr = FDP2.GetFDRs.FirstOrDefault(f => f.Callsign == msg_fields[0]);
            }

            switch (packet[0])
            {
                case 'U':
                    if (!connected)
                        OnConnected();
                    SendControllerInfo();
                    break;
                case 'r':
                    if (Airport?.ICAOName != msg || setRunways == false)
                    {
                        Airport = Airspace2.GetAirport(msg);
                        setRunways = false;
                        //vStripsPlugin.ShowSetupWindow();                                             // Commented out to stop popup JMG
                    }
                    else
                        SendRunways();

                    Airport = Airspace2.GetAirport(msg);
                    break;
                case 'S':
                    if (msg_fields.Length > 1 && fdr != null)
                    {
                        switch (msg_fields[1])
                        {
                            case "TAXI":
                                MMI.EstFDR(fdr);
                                break;
                            //case "DEPA":
                            //    FDP2.DepartFDR(fdr, DateTime.UtcNow + TimeSpan.FromSeconds(180));
                            //    break;
                        }
                    }
                    break;
                
                /*
                 * JMG 
                 * vStrips doesn't send Arrival Runway, so we use Dep runway in vStrips for Arrival runway allocation. 
                 * We need to keep an eye out when Route changes received that have a Dep runway and reassign to Arr runway.
                 * 
                 */
                case 'R':
                    if (msg_fields.Length > 3 && fdr != null)
                    {                                                                        
                        if (fdr.DepAirport == msg_fields[1] && fdr.DesAirport == msg_fields[2])
                        {

                            string rte = msg_fields[3];
                            string[] rte_fields = rte.Split(' ');                                           // parse route on space                            
                            

                            if (rte_fields[0].Contains('/') )                                               // If the first field has a slash,  it's a Dep runway assignment.
                            {
                                string[] start_fields = rte_fields[0].Split('/');                                                           
                                String NewRwy = start_fields[1];                                            // get the runway

                                if (fdr.CoupledTrack?.OnGround == false)                                    // if we're airborne apply the runway to Arrivals
                                {
                                    FDP2.SetArrivalRunway(fdr, Airspace2.GetRunway(fdr.DesAirport, NewRwy));
                                }
                                else                                                                        // Apply the Route change, or Dep runway change
                                {
                                    string temprwy = "";
                                    if (fdr.DepartureRunway != null)
                                        temprwy = fdr.DepartureRunway.ToString();

                                    if (temprwy != NewRwy)                                                  // if the Dep runway has changed
                                    {
                                        FDP2.SetDepartureRunway(fdr, Airspace2.GetRunway(fdr.DepAirport, NewRwy));
                                    }
                                    else                                                                    // Not a runway change, so update the route
                                    {
                                        FDP2.ModifyRoute(fdr, rte);
                                    }
                                }
                            }                          

                        } else
                            FDP2.ModifyFDR(fdr, fdr.Callsign, fdr.FlightRules, msg_fields[1], msg_fields[2], msg_fields[3], fdr.Remarks, fdr.AircraftCount.ToString(), fdr.AircraftType, fdr.AircraftWake, fdr.AircraftEquip, fdr.AircraftSurvEquip, fdr.TAS.ToString(), fdr.RFL.ToString(), fdr.ETD.ToString("HHmm"), fdr.EET.ToString("HHmm"));
                        }
                        break;
                case 'H':
                    if (msg_fields.Length > 1 && fdr != null)
                    {
                        vStripsAssignedHeadings.AddOrUpdate(fdr.Callsign, int.Parse(msg_fields[1]), (s, i) => int.Parse(msg_fields[1]));

                        if (!string.IsNullOrEmpty(msg_fields[1]) && msg_fields[1] != "0")
                            FDP2.SetGlobalOps(fdr, $"H{msg_fields[1]}");

                        if (msg_fields.Length > 2 && msg_fields[2] != "0")
                            FDP2.SetCFL(fdr, FDP2.FDR.LEVEL_NONE, int.Parse(msg_fields[2]), false);
                    }
                    break;
                case '>':
                    // Edited JMG to ensure when strip is selected in vStrips, track is selected in vatSys
                    if(fdr != null)
                    {
                        var currentSelection = MMI.SelectedTrack;                                       // Deselect old
                        if(MMI.SelectedTrack != null)                                                   
                            MMI.SelectOrDeselectTrack(currentSelection);

                        var trk = MMI.FindTrack(fdr);
                        if (trk != null)
                        {                                                        
                            MMI.SelectOrDeselectTrack(trk);
                        }
                    }                    
                    break;
            }

            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(packet));
            Console.WriteLine(packet);
        }

        private static string ConvertToFreqString(uint vatsysFreq)
        {
            return vatsysFreq.ToString().Substring(0, 5);
        }

        public class PacketReceivedEventArgs : EventArgs
        {
            public readonly string Packet;
            public PacketReceivedEventArgs(string packet)
            {
                Packet = packet;
            }
        }
    }
}
