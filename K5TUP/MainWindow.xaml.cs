/*
 * The K5 Test Utility Program (K5TUP) enables users to test the K5’s CO2,GPS,BT and ANT subsystems.
 * The K5TUP can help the K5 Manufacturer during post board assembly testing, allowing a quick functional check for both the USB signal 
 * infrastructure of the K5 Digital Board and the forementioned hardware subsystems.
 * It also enable module firmware upgrade when combined with external tools.
 * The K5TUP works without the direct intervention of the target CPU.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using ANT_Managed_Library;
using System.Data;
using System.Runtime.InteropServices;
using FTD2XX_NET;

namespace K5TUP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region variables
        K5TUPInfo info = new K5TUPInfo();
        FlowDocument mcFlowDoc = new FlowDocument();
        Paragraph para = new Paragraph();
        SerialPort serial = null;
        string terminator;
        EventWaitHandle waitHandle;
        #endregion

        public void SerializeToXML(K5TUPInfo info)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(K5TUPInfo));
            TextWriter textWriter = new StreamWriter(@"C:\K5TUPInfo.xml");
            serializer.Serialize(textWriter, info);
            textWriter.Close();
        }
        public K5TUPInfo DeserializeFromXML()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(K5TUPInfo));
            TextReader textReader = new StreamReader(@"C:\K5TUPInfo.xml");
            K5TUPInfo info;
            info = (K5TUPInfo)deserializer.Deserialize(textReader);
            textReader.Close();
            return info;
        }

        public void delay(double ms)
        {
            DateTime dt = DateTime.Now.AddMilliseconds(ms);
            while (DateTime.Now <= dt) { ;}
        }

        public MainWindow()
        {
            InitializeComponent();

            if (!File.Exists(@"C:\K5TUPInfo.xml"))
            {
                //Does not exists
                info.DeviceName[0] = "CO2";
                info.ComPort[0] = "COM4";
                info.BaudRateIdx[0] = 0;
                info.DeviceName[1] = "GPS";
                info.ComPort[1] = "COM5";
                info.BaudRateIdx[1] = 0;
                info.DeviceName[2] = "Bluetooth";
                info.ComPort[2] = "COM6";
                info.BaudRateIdx[2] = 0;
                info.DeviceName[3] = "ANT";
                info.ComPort[3] = "COM7";
                info.BaudRateIdx[3] = 0;
                info.btRuAddr = "00:01:95:09:5e:f5";
                info.btPasskey = "1234";

                SerializeToXML(info);
            }

            // 
            // Buttons
            // 
            Connect_btn.Content = "Connect";
            Sel_btn.IsEnabled = false;
            Send_btn.IsEnabled = false;

            info = DeserializeFromXML();

            // 
            // comboBox
            // 
            Device_Name.Items.Add(info.DeviceName[0]);  //"CO2"
            Device_Name.Items.Add(info.DeviceName[1]);  //"GPS"
            Device_Name.Items.Add(info.DeviceName[2]);  //"Bluetooth"
            Device_Name.Items.Add(info.DeviceName[3]);  //"ANT"
            //            Device_Name.SelectedIndex = 0;   // default selection
            this.Device_Name.SelectionChanged += new SelectionChangedEventHandler(OnMyComboBoxChanged);

            // 
            // Test backgroundWorker
            // 
            Test.DoWork += Test_DoWork;
            Test.RunWorkerCompleted += Test_RunWorkerCompleted;
            Test.WorkerReportsProgress = true;
            Test.ProgressChanged += new ProgressChangedEventHandler(Test_ProgressChanged);

            progressBar1.TabIndex = 0;

            MessageBoxResult result = MessageBox.Show(this, "Before to proceed please connect K5DB via USB, set the onboard switch SW2.4 to OFF and power-on the board", "Confirmation", MessageBoxButton.OK);
            /*
            if (result == MessageBoxResult.OK)
            {
            }
             */
        }

        private void OnMyComboBoxChanged(object sender, SelectionChangedEventArgs e)
        {
            Comm_Port_Names.Items.Clear();
            Baud_Rates.Items.Clear();
            MsgData.Items.Clear();
            Comm_Port_Names.Text = "";

            //let's refresh the available COM ports and set the default one for the current device
            foreach (string s in SerialPort.GetPortNames())
            {
                Comm_Port_Names.Items.Add(s);
                if (String.Compare(s, info.ComPort[Device_Name.SelectedIndex]) == 0)
                    Comm_Port_Names.Text = info.ComPort[Device_Name.SelectedIndex];
            }

            switch (Device_Name.SelectedIndex)
            {
                case 0://"CO2":
                    {
                        image1.Source = (ImageSource)new ImageSourceConverter().ConvertFrom("pack://application:,,,/K5TUP;component/Images/co2.ico");
                        Baud_Rates.Items.Add("19200");
                        Baud_Rates.Items.Add("115200");
                        Baud_Rates.SelectedIndex = info.BaudRateIdx[0];   // default selection
                        MsgData.Items.Add("Get Serial Number");
                        MsgData.Items.Add("Enter pulse mode (sets to 115200)");
                        MsgData.Items.Add("Exit pulse mode (sets to 19200)");
                        MsgData.Items.Add("Get Extended Diag. Info");
                        MsgData.Items.Add("Set params to default");        //to set default values of trimmer, gain, ecc.
                        MsgData.Items.Add("Switch IR On");
                        MsgData.Items.Add("Switch IR Off");
                        //MsgData.Items.Add("Debug mode On/Off");
                        MsgData.Items.Add('"');         //to get Diagnostic Informations
                        MsgData.Items.Add("XT512");     //to set internal trimmer value to 512 (allowed 0..1023)
                        MsgData.Items.Add("XT1000");    //to set internal gain value to 1000 (allowed 100..4000)
                        MsgData.Items.Add("00.12");     //to calibrate at, for instance, [CO2] = 0.12 %
                        MsgData.SelectedIndex = 0;      //default selection
                        terminator = "\r";
                        break;
                    }
                case 1://"GPS":
                    {
                        image1.Source = (ImageSource)new ImageSourceConverter().ConvertFrom("pack://application:,,,/K5TUP;component/Images/gps.ico");
                        Baud_Rates.Items.Add("9600");
                        Baud_Rates.Items.Add("115200");
                        Baud_Rates.SelectedIndex = info.BaudRateIdx[1];   // default selection
                        MsgData.Items.Add("Warm Start");
                        MsgData.Items.Add("Cold Start");
                        MsgData.Items.Add("Set BaudRate to 115200");
                        MsgData.Items.Add("Set BaudRate to 9600");
                        //MsgData.Items.Add("Set BaudRate to 115200 (One-Time-Programmable)");
                        MsgData.Items.Add("Set Navigation rate to 1Hz");
                        MsgData.Items.Add("Set Navigation rate to 10Hz");
                        MsgData.SelectedIndex = 0;      //default selection
                        terminator = "\r\n";
                        break;
                    }
                case 2://"Bluetooth":
                    {
                        image1.Source = (ImageSource)new ImageSourceConverter().ConvertFrom("pack://application:,,,/K5TUP;component/Images/bluetooth.ico");
                        Baud_Rates.Items.Add("115200");
                        Baud_Rates.Items.Add("460800");
                        Baud_Rates.SelectedIndex = info.BaudRateIdx[2];   // default selection
                        MsgData.Items.Add("Mux Mode Off");
                        MsgData.Items.Add("RESET");
                        MsgData.Items.Add("INFO");  //iWRAP info
                        MsgData.Items.Add("SET");
                        MsgData.Items.Add("Set BaudRate to 460800");
                        MsgData.Items.Add("Set BaudRate to 115200");
                        MsgData.Items.Add("Reboot in HCI mode");
                        MsgData.Items.Add("INQUIRY 3");
                        MsgData.Items.Add("Connect SPP");
                        MsgData.Items.Add("Connect HFP");
                        MsgData.Items.Add("Play Music");
                        MsgData.SelectedIndex = 0;      //default selection
                        terminator = "\n\r";
                        break;
                    }
                case 3://"ANT":
                    {
                        image1.Source = (ImageSource)new ImageSourceConverter().ConvertFrom("pack://application:,,,/K5TUP;component/Images/ant.ico");
                        Baud_Rates.Items.Add("57600");
                        Baud_Rates.SelectedIndex = info.BaudRateIdx[3];   // default selection
                        MsgData.Items.Add("Reset");
                        MsgData.Items.Add("Start HRM");
                        MsgData.Items.Add("Stop HRM");
                        MsgData.SelectedIndex = 0;      //default selection
                        terminator = "";
                        break;
                    }
                default:
                    break;
            }
        }

        //it runs within WPF thread
        private void Connect_Comms(object sender, RoutedEventArgs e)
        {
            //we cannot use the serialCom methods here because this object belongs to a different thread. Let's use a dispatcher control
            if ((string)Connect_btn.Content == "Connect")
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => open_enControls()));
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => close_enControls()));
            }
        }

        private void open_enControls()
        {
            try
            {
                serial = new SerialPort();
                serial.PortName = Comm_Port_Names.Text;
                serial.BaudRate = Convert.ToInt32(Baud_Rates.Text);
                serial.Handshake = System.IO.Ports.Handshake.None;
                serial.Parity = Parity.None;
                serial.DataBits = 8;
                serial.StopBits = StopBits.One;
                serial.ReadTimeout = 20;
                serial.WriteTimeout = 50;
                serial.RtsEnable = serial.DtrEnable = true;
                serial.DtrEnable = false;   /* check if useful */
                serial.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to OPEN:\n" + ex + "\n", "Open");
            }

            if (serial.IsOpen)
            {
                // let's prevent some changes
                Device_Name.IsEnabled = false;
                Comm_Port_Names.IsEnabled = false;
                Baud_Rates.IsEnabled = false;
                Sel_btn.IsEnabled = true;
                Send_btn.IsEnabled = true;
                WriteData("\r\n--- " + Device_Name.SelectedItem.ToString() + " is connected ---\r\n");
                Connect_btn.Content = "Disconnect";
                serial.DataReceived += Receive;     //Creates function call on data received
            }
        }

        private void close_enControls()
        {
            if (serial != null)
            {
                serial.DataReceived -= Receive;
                serial.Close();

                //                if (!serial.IsOpen)
                {
                    Device_Name.IsEnabled = true;
                    Comm_Port_Names.IsEnabled = true;
                    Baud_Rates.IsEnabled = true;
                    Sel_btn.IsEnabled = false;
                    Send_btn.IsEnabled = false;
                    WriteData("\r\n--- Disconnecting " + Device_Name.SelectedItem.ToString() + " ---\r\n");
                    Connect_btn.Content = "Connect";
                }
            }
        }

        #region Receiving

        private delegate void UpdateUiTextDelegate(string text);

        private void Receive(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // Collecting the characters received to our 'buffer' (string).
            Dispatcher.Invoke(DispatcherPriority.Normal, new UpdateUiTextDelegate(WriteData), "");
        }

        private void WriteData(string text)
        {
            if (string.IsNullOrEmpty(text))
                text = serial.ReadExisting();

            if (para.Inlines.Count >= 300)
                para.Inlines.Clear();

            para.Inlines.Add(text);
            mcFlowDoc.Blocks.Add(para);
            ReceiveBox.Document = mcFlowDoc;
            ReceiveBox.ScrollToEnd();
        }

        #endregion

        #region Sending

        private void Sel_Data(object sender, RoutedEventArgs e)
        {
            SerialData.Text = MsgData.SelectedItem.ToString();
        }

        private void Send_Data(object sender, RoutedEventArgs e)
        {
            switch (Device_Name.SelectedIndex)
            {
                case 0://"CO2":
                    {
                        switch (SerialData.Text)
                        {
                            case "Get Serial Number":
                                {
                                    SerialCmdSend("!");
                                    break;
                                }
                            case "Enter pulse mode (sets to 115200)":
                                {
                                    SerialCmdSend("XP");
                                    break;
                                }
                            case "Exit pulse mode (sets to 19200)":
                                {
                                    SerialCmdSend("XC");
                                    break;
                                }
                            case "Get Extended Diag. Info":
                                {
                                    SerialCmdSend("&");
                                    break;
                                }
                            case "Switch IR On":
                                {
                                    SerialCmdSend("XE");
                                    break;
                                }
                            case "Switch IR Off":
                                {
                                    SerialCmdSend("XO");
                                    break;
                                }
                            /*
                        case "Debug mode On/Off":
                            {
                                SerialCmdSend("XD");
                                break;
                            }
                             * */
                            case "Set params to default":
                                {
                                    SerialCmdSend("XR");
                                    break;
                                }
                            default:
                                SerialCmdSend(SerialData.Text);
                                break;
                        }
                        break;
                    }
                case 1://"GPS":
                    {
                        switch (SerialData.Text)
                        {
                            case "Warm Start":
                                {
                                    byte[] hdata = { 0xB5, 0x62, 0x06, 0x04, 0x04, 0x00, 0x01, 0x00, 0x02, 0x00, 0x11, 0x6C };
                                    SerialCmdSend(hdata);
                                    break;
                                }
                            case "Cold Start":
                                {
                                    byte[] hdata = { 0xB5, 0x62, 0x06, 0x04, 0x04, 0x00, 0xFF, 0x87, 0x02, 0x00, 0x96, 0xF9 };
                                    SerialCmdSend(hdata);
                                    //SerialCmdSend("$PMTK104*37"); /* for MediaTek MT33xx based devices, like UX530, CAM-8MQ, ecc. */
                                    break;
                                }
                            case "Set BaudRate to 9600":
                                {
                                    byte[] hdata = { 0xB5, 0x62, 0x06, 0x00, 0x14, 0x00, 0x01, 0x00, 0x00, 0x00, 0xD0, 0x08, 0x00, 0x00, 0x80, 0x25, 0x00, 0x00, 0x07, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA2, 0xB5 };
                                    SerialCmdSend(hdata);
                                    //SerialCmdSend("$PMTK251,9600*17");
                                    break;
                                }
                            case "Set BaudRate to 115200":
                                {
                                    byte[] hdata = { 0xB5, 0x62, 0x06, 0x00, 0x14, 0x00, 0x01, 0x00, 0x00, 0x00, 0xD0, 0x08, 0x00, 0x00, 0x00, 0xC2, 0x01, 0x00, 0x07, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC0, 0x7E };
                                    SerialCmdSend(hdata);
                                    //SerialCmdSend("$PMTK251,115200*1F");
                                    break;
                                }
                            case "Set BaudRate to 115200 (One-Time-Programmable)":
                                {//can be done only once
                                    byte[] hdata = { 0xB5, 0x62, 0x06, 0x41, 0x09, 0x00, 0x01, 0x01, 0x30, 0x81, 0x00, 0x00, 0x00, 0x00, 0xF8, 0xFB, 0x1C };
                                    SerialCmdSend(hdata);
                                    break;
                                }
                            case "Set Navigation rate to 1Hz":
                                {
                                    byte[] hdata = { 0xB5, 0x62, 0x06, 0x08, 0x06, 0x00, 0xE8, 0x03, 0x01, 0x00, 0x01, 0x00, 0x01, 0x39 };
                                    SerialCmdSend(hdata);
                                    //SerialCmdSend("$PMTK300,1000,0,0,0,0*1C");
                                    break;
                                }
                            case "Set Navigation rate to 10Hz":
                                {
                                    byte[] hdata = { 0xB5, 0x62, 0x06, 0x08, 0x06, 0x00, 0x64, 0x00, 0x0A, 0x00, 0x01, 0x00, 0x83, 0x36 };
                                    //byte[] hdata = { 0xB5, 0x62, 0x06, 0x08, 0x06, 0x00, 0x64, 0x00, 0x01, 0x00, 0x01, 0x00, 0x7A, 0x12 };
                                    SerialCmdSend(hdata);
                                    //SerialCmdSend("$PMTK300,100,0,0,0,0*2C");
                                    break;
                                }
                            default:
                                SerialCmdSend(SerialData.Text);
                                break;
                        }
                        break;
                    }
                case 2://"Bluetooth":
                    {
                        switch (SerialData.Text)
                        {
                            case "Connect SPP":
                                {
                                    if (!Test.IsBusy)
                                    {
                                        WriteData("\r\n--- Connecting to " + info.btRuAddr + " with Passkey " + info.btPasskey + " ---\r\n");
                                        Test.RunWorkerAsync(0);
                                        Sel_btn.IsEnabled = false;
                                        Send_btn.IsEnabled = false;
                                    }
                                    break;
                                }
                            case "Connect HFP":
                                {
                                    if (!Test.IsBusy)
                                    {
                                        WriteData("\r\n--- Connecting to " + info.btRuAddr + " with Passkey " + info.btPasskey + " ---\r\n");
                                        Test.RunWorkerAsync(1);
                                        Sel_btn.IsEnabled = false;
                                        Send_btn.IsEnabled = false;
                                    }
                                    break;
                                }
                            case "Mux Mode Off":
                                {
                                    byte[] hdata = { 0xBF, 0xFF, 0x00, 0x11, 0x53, 0x45, 0x54, 0x20, 0x43, 0x4f, 0x4e, 0x54, 0x52, 0x4f, 0x4c, 0x20, 0x4d, 0x55, 0x58, 0x20, 0x30, 0x00 };
                                    SerialCmdSend(hdata);
                                    break;
                                }
                            case "Set BaudRate to 460800":
                                {
                                    SerialCmdSend("SET CONTROL BAUD 460800,8N1");
                                    break;
                                }
                            case "Set BaudRate to 115200":
                                {
                                    SerialCmdSend("SET CONTROL BAUD 115200,8N1");
                                    break;
                                }
                            case "Reboot in HCI mode":
                                {
                                    SerialCmdSend("BOOT 4");
                                    break;
                                }
                            case "Play Music":
                                {
                                    SerialCmdSend("PLAY &-5aaa;f:_6c-5a;f:_6c-5a_-6eee;f:_6c-5a;f:_6c-5a");
                                    break;
                                }
                            default:
                                SerialCmdSend(SerialData.Text);
                                break;
                        }
                        break;
                    }
                case 3://"ANT":
                    {
                        switch (SerialData.Text)
                        {
                            case "Reset":
                                {
                                    byte[] hdata = { 0xA4, 0x01, 0x4A, 0x00, 0xEF };
                                    SerialCmdSend(hdata);
                                    break;
                                }
                            case "Stop HRM":
                                {
                                    byte[] hdata = { 0xA4, 0x01, 0x4C, 0x00, 0xE9 };
                                    SerialCmdSend(hdata);
                                    break;
                                }
                            case "Start HRM":
                                {
                                    if (!Test.IsBusy)
                                    {
                                        Test.RunWorkerAsync(2);
                                        Sel_btn.IsEnabled = false;
                                        Send_btn.IsEnabled = false;
                                    }
                                    break;
                                }
                            default: break;
                        }
                        break;
                    }
                default: break;
            }

            SerialData.Text = "";
        }

        public void SerialCmdSend(string data)
        {
            if (serial != null && serial.IsOpen)
            {
                try
                {
                    // Send the ascii data out the port
                    byte[] hexstring = Encoding.ASCII.GetBytes(data + terminator);
                    serial.Write(hexstring, 0, hexstring.Length);
                    /*
                                        foreach (byte hexval in hexstring)
                                        {
                                            byte[] _hexval = new byte[] { hexval }; // need to convert byte to byte[] to write
                                            serial.Write(_hexval, 0, 1);
                                            Thread.Sleep(1);
                                        }
                     */
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Failed to SEND:\n" + ex + "\n", "Send");
                }
            }
        }

        public void SerialCmdSend(byte[] data)
        {
            if (serial != null && serial.IsOpen)
            {
                try
                {
                    // Send the hex data out the port
                    serial.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Failed to SEND:\n" + ex + "\n", "Send");
                }
            }
        }
        #endregion

        #region Testing
        private void port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
        }

        /* BackgroundWorker makes threads easy to implement in Windows Forms. Intensive tasks need to be done on another 
         * thread so that the UI doesn't freeze. It is necessary to post messages and update the user interface when the task is done */
        private readonly BackgroundWorker Test = new BackgroundWorker();

        private void Test_DoWork(object sender, DoWorkEventArgs e)
        {
            int arg = (int)e.Argument;
            bool completed = false;
            long timeOut = DateTime.Now.Ticks;  // actual date and time
            waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            switch (arg)
            {
                case 0://Bluetooth: Connect SPP:
                    {
                        SerialCmdSend("RESET");
                        //Thread.Sleep(500);
                        delay(1000);
                        Test.ReportProgress(10);
                        SerialCmdSend("HCI 0c3f000a00ff00ff00ff00ff00ff00ff0000000000ff007f"); //hci 8
                        delay(500);
                        Test.ReportProgress(20);
                        SerialCmdSend("SET BT PAGEMODE 3 3200 1");
                        delay(500);
                        Test.ReportProgress(30);
                        SerialCmdSend("SET CONTROL MUX 0");
                        delay(500);
                        Test.ReportProgress(40);
                        SerialCmdSend("SET CONTROL CONFIG 1901");
                        delay(500);
                        Test.ReportProgress(50);
                        SerialCmdSend("SET BT SSP 3 0");
                        delay(500);
                        Test.ReportProgress(60);
                        SerialCmdSend("SET PROFILE SPP Bluetooth Serial Port");
                        delay(500);
                        Test.ReportProgress(65);
                        SerialCmdSend("SET PROFILE HFP Hands-Free");
                        delay(500);
                        Test.ReportProgress(70);
                        SerialCmdSend("PAIR " + info.btRuAddr);
                        delay(500);
                        Test.ReportProgress(80);
                        SerialCmdSend("AUTH " + info.btRuAddr + " " + info.btPasskey);
                        delay(500);
                        Test.ReportProgress(90);
                        break;
                    }
                case 1://Bluetooth: Connect HFP:
                    {
                        SerialCmdSend("CALL " + info.btRuAddr + " 0x1FFF HFP" + info.btPasskey);
                        delay(500);
                        Test.ReportProgress(90);
                        break;
                    }
                case 2://ANT: Start HRM
                    {
                        byte[] RESET = { 0xA4, 0x01, 0x4A, 0x00, 0xEF };  //RESET
                        SerialCmdSend(RESET);
                        delay(1000);
                        Test.ReportProgress(10);
                        byte[] SETNETKEY = { 0xA4, 0x09, 0x46, 0x00, 0xB9, 0xA5, 0x21, 0xFB, 0xBD, 0x72, 0xC3, 0x45, 0x64 };
                        SerialCmdSend(SETNETKEY);
                        delay(1000);
                        Test.ReportProgress(20);
                        byte[] SETTXPWR = { 0xA4, 0x02, 0x47, 0x00, 0x04, 0xE5 };    //Set Tx Power 4dB
                        SerialCmdSend(SETTXPWR);
                        delay(1000);
                        Test.ReportProgress(30);
                        byte[] SETCHNUM = { 0xA4, 0x03, 0x42, 0x00, 0x00, 0x00, 0xE5 }; //Assign Channel 0
                        SerialCmdSend(SETCHNUM);
                        delay(1000);
                        Test.ReportProgress(40);
                        byte[] SETCHFREQ = { 0xA4, 0x02, 0x45, 0x00, 0x39, 0xDA };    //Set Channel Frequency 57
                        SerialCmdSend(SETCHFREQ);
                        delay(1000);
                        Test.ReportProgress(50);
                        byte[] SETCHID = { 0xA4, 0x05, 0x51, 0x00, 0x00, 0x00, 0x78, 0x00, 0x88 };   //Set Channel ID to HRM
                        SerialCmdSend(SETCHID);
                        delay(1000);
                        Test.ReportProgress(60);
                        byte[] SETCHPERIOD = { 0xA4, 0x03, 0x43, 0x00, 0x86, 0x1F, 0x7D }; //Set Channel Period to 4Hz
                        SerialCmdSend(SETCHPERIOD);
                        delay(1000);
                        Test.ReportProgress(70);
                        byte[] OPENCH = { 0xA4, 0x01, 0x4B, 0x00, 0xEE };   //Open Channel 0
                        SerialCmdSend(OPENCH);
                        delay(1000);
                        break;
                    }
                default: break;
            }

            e.Result = completed;    //test result
        }

        private void Test_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work
            bool? completed = (e.Result as bool?);
            /*
                        if (completed.HasValue && completed.Value == true)
                            MessageBox.Show(this, "Passed.", "Test");
                        else
                            MessageBox.Show(this, "Failed.", "Test");
             */
            progressBar1.Value = 0;
            Sel_btn.IsEnabled = true;
            Send_btn.IsEnabled = true;
        }

        private void Test_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        #endregion

        #region Form Controls

        private void Close_Form(object sender, RoutedEventArgs e)
        {
            if (serial != null && serial.IsOpen)
                serial.Close();
            this.Close();
        }
        private void Max_size(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized) this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }
        private void Min_size(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized) this.WindowState = WindowState.Minimized;
            else this.WindowState = WindowState.Normal;
        }
        private void Move_Window(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        #endregion

        private void About_btn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "K5 Test Utility Program\nCopyright 2014 COSMED Italy\n\nRelease version: " + "01.00", "About");
        }

        private void MsgData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void SerialData_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void Comm_Port_Names_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Store_btn_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(@"C:\K5TUPInfo.xml") && info != null)
            {
                if (Comm_Port_Names.Text != "")
                    info.ComPort[Device_Name.SelectedIndex] = Comm_Port_Names.Text;
                info.BaudRateIdx[Device_Name.SelectedIndex] = Baud_Rates.SelectedIndex;

                SerializeToXML(info);
            }
        }

        private void Clear_btn_Click(object sender, RoutedEventArgs e)
        {
            byte value = 0;
            para.Inlines.Clear();
            //            ReceiveBox.Document.Blocks.Clear();
        }
    }
}

//this class defines the k5TUPInfo xml file structure
public class K5TUPInfo
{
    public string[] DeviceName { get; set; }
    public string[] ComPort { get; set; }
    public int[] BaudRateIdx { get; set; }
    public string btRuAddr { get; set; }
    public string btPasskey { get; set; }

    //Constructor
    public K5TUPInfo()
    {
        DeviceName = new string[] { "CO2", "GPS", "Bluetooth", "ANT" };
        ComPort = new string[] { "COM3", "COM4", "COM5", "COM6" };
        BaudRateIdx = new int[] { 19200, 9600, 115200, 57600 };
    }
}


