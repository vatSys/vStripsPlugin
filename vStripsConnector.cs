﻿using System;
using System.Collections.Generic;
using vatsys;
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
        
        private IPEndPoint vStripsHost;
        private UdpClient vStripsSocket;
        private const int VSTRIPS_PORT = 60301;
        private const string MIN_VSTRIPS_PLUGIN_VERSION = "1.18";
        private CancellationTokenSource cancellationToken;
        
        private ConcurrentDictionary<string, int> vStripsAssignedHeadings = new ConcurrentDictionary<string, int>();
        private List<string> vStripsSentATCCallsigns = new List<string>();
        private bool connected = false;
        private bool setRunways = false;
        
        public static event EventHandler<PacketReceivedEventArgs> PacketReceived;
        
        public static IPAddress HostIP = IPAddress.Loopback;        
        public static string runways = DEFAULT_RUNWAYS;
        private static string qnh="";
        public static Airspace2.Airport Airport = null;

        private static vStripsConnector Instance;

        private vStripsConnector()
        {
            vStripsHost = new IPEndPoint(HostIP, VSTRIPS_PORT);
            vStripsSocket = new UdpClient();
            cancellationToken = new CancellationTokenSource();

        }

        public static string Runways
        {
            get { return runways; }
            set { runways = value; Instance.setRunways = true; Instance.SendRunways(); }
        }

        private void Network_OnlineATCChanged(object sender, Network.ATCUpdateEventArgs e)
        {
            if (Instance?.connected == true)
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
        }

        private void SyncATCLists()
        {
            var online = Network.GetOnlineATCs;
            if (Instance?.connected == true)
            {
                foreach (var atc in online.Where(a => !vStripsSentATCCallsigns.Contains(a.Callsign)))
                {
                    SendATCOnline(atc);
                    vStripsSentATCCallsigns.Add(atc.Callsign);
                }
                foreach (var atc in vStripsSentATCCallsigns.Except(online.Select(a => a.Callsign)))
                    SendATCOffline(atc);
            }
        }

        private void Network_PrimaryFrequencyChanged(object sender, EventArgs e)
        {
            SendControllerInfo();
        }

        public static void Restart()
        {
            Instance?.Stop();
            Instance = null;
            Start();
        }

        public static void Start()
        {
            Instance = new vStripsConnector();
            Instance?.Connect();            
        }

        private void Stop()
        {
            Instance?.Disconnect();
            Instance?.vStripsAssignedHeadings.Clear();
            Instance?.vStripsSentATCCallsigns.Clear();
        }

        private void Connect()
        {
            vStripsSocket.Connect(vStripsHost);
            Task.Run(() => ReceiveData());
            Task.Run(() => PollForConnection());
        }

        private void Disconnect()
        {
            connected = false;
            cancellationToken.Cancel();
            vStripsSocket.Close();
            vStripsSocket.Dispose();            
        }


        private void PollForConnection()
        {
            while (!connected && !cancellationToken.IsCancellationRequested)
            {
                SendVersion();
                Thread.Sleep(1000);
            }
        }

        private void SendVersion()
        {
            SendData("h" + MIN_VSTRIPS_PLUGIN_VERSION);
        }

        private void OnConnected()
        {
            
            Network.PrimaryFrequencyChanged += Network_PrimaryFrequencyChanged;
            Network.OnlineATCChanged += Network_OnlineATCChanged;
            MET.Instance.ProductsChanged += MET_ProductChanged;
            SyncATCLists();
            connected = true;
            SendQnh();            
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
                Instance.SendData(">" + callsign);            
        }

        /*
         * Called on MET change event
         * If a VATSIM METAR received and it matches the ICAO of our Airfield, store in the qnh var
         * Then if online, update online
         * 
         */
        public void MET_ProductChanged(object sender, MET.ProductsChangedEventArgs e)
        {
            if (e.ProductRequest.Icao == Airport?.ICAOName)                
            {
                try
                {
                    var products = MET.Instance.Products[e.ProductRequest];
                    var prod = products.FirstOrDefault();
                    switch (prod)
                    {
                        case MET.VATSIM_METAR vatmet:
                            qnh = vatmet.QNH;
                            if (Instance?.connected == true)
                                SendQnh();
                            break;
                        default:
                            break;
                    }
                } catch (Exception ex) {
                    // When an Airfield is cleared from the AIS window 
                    // var products = MET.Instance.Products[e.ProductRequest];
                    // generates an Indec Not found exception
                }
            }
        }

        /*
         * Request a VATSIM METAR
         * Called on reciept of Airfield code from vatSys
         */
        private void getMetar(string ICAO)
        {
            if (Network.IsConnected)                                                                
            {
                if (qnh == "" || Airport.ICAOName != ICAO) {                                                            // Only call if no QNH or Aifield changed
                    MET.ProductRequest myreq = new MET.ProductRequest(MET.ProductType.VATSIM_METAR, ICAO, true);
                    MET.Instance.RequestProduct(myreq);
                }
                SendQnh();
            }
        }

       
        private void SendControllerInfo()
        {
            if (!Network.IsConnected)
                return;

            SendVersion();

            //CALLSIGN:NAME:FREQ
            string pack = $"U{Network.Me.Callsign}:{Network.Me.RealName}:{ConvertFreqArrayToString(Network.Me.Frequencies)}";
            Instance?.SendData(pack);

            string trans = $"T{RDP.TRANSITION_ALTITUDE}";
            Instance?.SendData(trans);
        }
        
        /*
         * Construct the QNH string for the vStrips selected airport and send
         */
        private void SendQnh()
        {
            if (Instance?.connected == true && qnh != "" && Airport?.ICAOName != null)            
                Instance?.SendData($"Q{Airport.ICAOName} Q{qnh}");            
        }

        private void SendATCOnline(NetworkATC atc)
        {
            if (Instance?.connected==true)
            {               
                Instance?.SendData($"C{atc.Callsign}:{atc.RealName}:{(ConvertFreqArrayToString(atc.Frequencies))}");
            }
        }

        private void SendATCOffline(string callsign)
        {
            if (Instance?.connected == true)
                Instance?.SendData($"c{callsign}");
        }

        private void SendDeleteAircraft(FDP2.FDR fdr)
        {
            if (Instance?.connected == true)
                Instance?.SendData($"D{fdr.Callsign}");
        }

        private void SendRemarks(FDP2.FDR fdr)
        {
            string rmks = $"P{fdr.Callsign}:{fdr.Remarks}";
            if (Instance?.connected == true)
                Instance?.SendData(rmks);
        }

        private void SendRunways()                                                                  // modified JMG to force runways to none and add send QNH
        {
            Instance?.SendData($"R00:00");
            //SendData($"R{Runways}"); //Old                                                                          
        }

        /**
         *  Construct Aircraft data
         *  
         *      string[] strArray = msg.Split(':');
                this.callsign = strArray[0];
                this.upDown = this.callsign[0];
                this.callsign = this.callsign.Substring(1);
                this.origin = strArray[1];
                this.dest = strArray[2];
                this.squawk = string.IsNullOrEmpty(strArray[3]) ? (string)null : strArray[3];
                this.sentSquawk = string.IsNullOrEmpty(strArray[4]) ? (string)null : strArray[4];
                this.groundState = strArray[5][0];
                this.altitude = int.Parse(strArray[6]);
                this.heading = int.Parse(strArray[7]);
                this.gone = int.Parse(strArray[8]);
                this.toGo = int.Parse(strArray[9]);
                this.aircraftType = strArray[10];
                this.sidName = string.IsNullOrEmpty(strArray[11]) ? (string)null : strArray[11];
                this.runway = string.IsNullOrEmpty(strArray[12]) ? (string)null : strArray[12];
                this.fpRules = strArray[13][0] == 'V' ? FltRules.VFR : FltRules.IFR;
                this.rfl = int.Parse(strArray[14]);
                this.estDepTime = strArray[15];
                this.spd = strArray[16];
                this.groundspeed = int.Parse(strArray[17]);
                this.route = strArray[18];
                this.distanceFromMe = int.Parse(strArray[19]);
                this.commsType = strArray[20][0];
                try
                {
	                this.lat = double.Parse(strArray[21]);
	                this.lon = double.Parse(strArray[22]);
                }
                catch
                {
                }
                if (!(Presenter.CurrentAirfield == "EGLL") && !(Presenter.CurrentAirfield == "EGCC"))
	                return;
                this.scratchpad = strArray[23];
         * 
         */

        private void SendAircraftMetadata(FDP2.FDR fdr)
        {            
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

            Instance?.SendData("M" + meta);
            SendRemarks(fdr);
        }

        private void SendData(string packet)
        {
            var data = Encoding.ASCII.GetBytes(packet);
            Instance?.vStripsSocket.SendAsync(data, data.Length);
        }
      

        private void ReceiveData()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    byte[] data = vStripsSocket.Receive(ref vStripsHost);
                    string packet = Encoding.ASCII.GetString(data);
                    ProcessData(packet);
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

       
        private void ProcessData(string packet)
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
                
                case 'r':                                                                              // Runway request     
                    if (Airport?.ICAOName != msg)// || setRunways == false)                            // Commented out as part of  runway supression
                    {
                        getMetar(msg);                                                                 // Call before we update 'Airport'
                        Airport = Airspace2.GetAirport(msg);
                        setRunways = false;
                        //vStripsPlugin.ShowSetupWindow();                                             // Commented out to stop popup JMG                        
                    }
                    else
                        //SendRunways();

                    Airport = Airspace2.GetAirport(msg);
                    break;
                
                case 'S':                                                                               // State
                    if (msg_fields.Length > 1 && fdr != null)
                    {
                        switch (msg_fields[1])
                        {
                            case "TAXI":
                                MMI.EstFDR(fdr);
                                break;                            
                        }
                    }
                    break;
                
                /*
                 * JMG 
                 * vStrips doesn't send Arrival Runway, so we use Dep runway in vStrips for Arrival runway allocation. 
                 * We need to keep an eye out when Route changes received that have a Dep runway and reassign to Arr runway.
                 * 
                 */
                case 'R':                                                                                   // Route
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
                
                case 'H':                                                                                   // Heading
                    if (msg_fields.Length > 1 && fdr != null)
                    {
                        vStripsAssignedHeadings.AddOrUpdate(fdr.Callsign, int.Parse(msg_fields[1]), (s, i) => int.Parse(msg_fields[1]));

                        if (!string.IsNullOrEmpty(msg_fields[1]) && msg_fields[1] != "0")
                            FDP2.SetGlobalOps(fdr, $"H{msg_fields[1]}");

                        if (msg_fields.Length > 2 && msg_fields[2] != "0")
                            FDP2.SetCFL(fdr, FDP2.FDR.LEVEL_NONE, int.Parse(msg_fields[2]), false);
                    }
                    break;
                
                case '>':                                                                               // Select Callsign
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

        private static string ConvertFreqArrayToString(int[] vatsysFreqArray)
        {
            return vatsysFreqArray[0].ToString().Substring(0, 5);
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
