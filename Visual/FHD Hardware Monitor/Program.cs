using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO.Ports;
using LibreHardwareMonitor.Hardware;
//using System.Diagnostics;
//using LibreHardwareMonitor.Hardware.Cpu;

/// <summary>
/// The EHM is a miniature monitoring program that uses the LibreHardwareMonitorLib ( https://github.com/LibreHardwareMonitor/LibreHardwareMonitor )
/// to retrieve, parse and prepare a few snippets of data from the windows hardware for sending to an external monitor connect by USB (As Serial COM port)
/// Program has been measured to use <0.05% of a Ryzen 1600 and ~7MB of (working set) memory 
/// 
/// Changelog: 0.1:
///     -Initial version based on OpenHardwareMonitor
/// 0.2:
///     -Optimized OHM implementation, Reduced CPU Foot print from 2% to <0.05% (Negligable) by avoiding unnecessary constant opening/closing.
///     -Rewritten code for consistency and simplicity (no more frankensteined constructors!)
/// 0.3:
///     -Better sectioning of resources, better functions. Lot smaller and lot more readable. 
///     -Implemented proper Disposal of unmanaged resources (System Tray icon and SerialPort) on exit.
///     -Implemented few tweaks to make adding/removing sensors just a smidge easier.
/// 0.4:
///     -Migrated from Deprecated OHM DLL to LibreHardwareMonitor with NUGET
///     -Implemented ApplicationSettings for enabling auto-attempt via the Config file.
/// </summary>

namespace FHD_Hardware_Monitor
{
    /// <summary>
    /// Hardware Monitoring and Serial Writing functionality
    /// </summary>
    static class Measuring_class
    {
        //Declare Sensor values. If a value remains 255 it means it ain't getting a new value below. Add/Remove as you need.
        public static byte Sensor_1 = 0xFF;
        public static byte Sensor_2 = 0xFF;
        public static byte Sensor_3 = 0xFF;
        public static int Sensor_3_RAW = 1;
        public static bool Sending = false;                 
        private static SerialPort _serialPort;
        private static readonly UpdateVisitor updateVisitor = new UpdateVisitor();
        private static Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = false,
            IsNetworkEnabled = true,
            IsStorageEnabled = false
        };


        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
        public static void Start()
        {
            // Starts the Monitor (Note that the monitor will ONLY gather info from components that are enabled! disabled components will not return sensor values)
            
            // Construct the SerialPort. Configured according to default Arduino settings at highest speed (115200).
            _serialPort = new SerialPort
            {
                BaudRate = 115200,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            computer.Open();
        }
        public static void GetSystemInfo()  //Parses sensor, updates local values and transmits values to Serial Bus.
        {
            //computer.Accept(new UpdateVisitor());
            computer.Accept(updateVisitor); //Visits and accepts hardware information
            
            /* HARDWARE & SENSOR CHEATSHEET (CPU, NAME, TYPE, Unit)
             * CPU  ,CPU Core #n        ,LOAD   ,percentage //Core specific load
             * CPU  ,CPU Total          ,LOAD   ,percentage //Total CPU load
             * 
             * Memory, Memory Used      ,DATA   ,Gigabytes  //Total Physical Memory in use
             * Memory, Memory Available ,DATA,Gigabytes  //Total Physical Memory available
             * Memory, Memory           ,LOAD   ,Percentage //Total memory in use
             * GPU  ,GPU Core           ,Temperature, Celsius
             * GPU  ,GPU Core           ,Clock  ,Mhz
             * GPU  ,GPU Fan,           ,Control,Percentage
             */
            foreach (IHardware hardware in computer.Hardware)
            {
                /*foreach (IHardware subhardware in hardware.SubHardware)
                {
                    foreach (ISensor sensor in subhardware.Sensors)
                    {
                        //?
                    }
                }*/

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if(hardware.HardwareType == HardwareType.Cpu && sensor.Name == "CPU Total")
                    {
                        //Console.WriteLine("FOUND THE TOTAL CPU!!: {0}", sensor.Value);
                        double CPU_D = Convert.ToDouble(sensor.Value.ToString());
                        Sensor_1 = (byte)(CPU_D * 2);
                    }
                    if (hardware.HardwareType == HardwareType.Memory && sensor.SensorType == SensorType.Load && sensor.Name == "Memory")
                    {
                        //Console.WriteLine("FOUND THE TOTAL RAM!!: {0}", sensor.Value);
                        double MEM_D = Convert.ToDouble(sensor.Value.ToString());
                        Sensor_2 = (byte)(MEM_D * 2);
                    }
                    if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                        if (sensor.Name == "GPU Power")
                        {
                        //Console.WriteLine("FOUND THE TOTAL GPU!!: {0}", sensor.Value);
                        double GPU_D = Convert.ToDouble(sensor.Value.ToString());
                        Sensor_3 = (byte)(GPU_D * 2);
                        Sensor_3_RAW = (int)(GPU_D);
                    }
                }
            }
            //Generate Serial Payload with Pre-Amble (Pre-Amble serves to allow the device to recognize as legit traffic, rather than just accept any random garbage)
            byte[] serial_buffer = new byte[5] { 0x03, 0xF4, Sensor_1, Sensor_2, Sensor_3 };
            if (_serialPort.IsOpen)
                _serialPort.Write(serial_buffer, 0, 5);
        }
        public static bool Set_Port(string COMS)
        {
            if (_serialPort.IsOpen)         //If already open. Close before changing Port
                _serialPort.Close();
            _serialPort.PortName = COMS;
            return Try_open();
        }
        public static bool Try_open()
        {
            //Tries to open the COM Port.
            try
            {
                _serialPort.Open();
                Sending = true;
            }
            catch
            {
                Sending = false;
            }
            return Sending;
        }
        public static void Close_Port()
        {
            //Closes Port (if not already)
            if (_serialPort.IsOpen)
                _serialPort.Close();
        }
        public static void Close_monitor()
        {
            //Disposes of serial Port. Then Closes Computer.
            //Fun-Fact: the close() function really just calls the Dispose() function in turn and thus effectively is equal to Dispose().
            //          The different names being mostly for proper coding ethics as disposal is treated more permanently than closing a port.
            //          The computer object from the LHM library lacks a Dispose() so only Close() is used.
            _serialPort.Dispose();
            computer.Close();
        }
    }


    public class Program
    {
        /// <summary>
        /// The main entry point for the application. Really only serves to the start the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Measuring_class.Start();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FHD_Monitor_Tray());
        }
    }


    public class FHD_Monitor_Tray : ApplicationContext
    {
        /// <summary>
        /// Interface Functionality.
        /// </summary>
        static readonly System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
        private static bool port_open = false;
        private static NotifyIcon trayIcon;
        private static MenuItem COM_Menu;
        private const byte SensorCount = 3;       //Simplifies adding/removing sensors a bit.
        private static string Comport; 
        public FHD_Monitor_Tray()
        {
            //Generate the Com Menu.
            COM_Menu = new MenuItem("COM Ports");
            foreach (string s in SerialPort.GetPortNames())
            {
                COM_Menu.MenuItems.Add(new MenuItem(s, Open_Com));
            }
            //Notifyicon Constructor
            trayIcon = new NotifyIcon()
            {
                Icon = new Icon("favicon.ico"),
                Text = "FHD Hardware Monitor",
                ContextMenu = new ContextMenu(new MenuItem[] {          //Creates initial menu. Add/remove sensors as needed (Be sure to check Sensorcount and the Monitoring function!)
                    new MenuItem("Sensor1"),
                    new MenuItem("Sensor2"),
                    new MenuItem("Sensor3"),
                    new MenuItem("-"),
                    new MenuItem("SET PORT!", Toggle_Com),
                    COM_Menu,
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };
            //Grey out unuseable fields (Sensor data and the dividing line)
            for (byte index = 0; index < SensorCount + 2; index++)
                trayIcon.ContextMenu.MenuItems[index].Enabled = false;
            //Check settings for a Default COM
            if (Properties.Settings.Default.COM_Autotry == true)
            {
                Comport = Properties.Settings.Default.COM_Default;
                port_open = true;
                trayIcon.ContextMenu.MenuItems[SensorCount + 1].Enabled = true;
            }
            //Timer Setup
            myTimer.Tick += new EventHandler(Monitor);
            myTimer.Interval = 1000;
            myTimer.Start();
        }

        private void Open_Com(object sender, EventArgs e)
        {
            //Sets the COM port.
            MenuItem COM = (MenuItem)sender;    //reference the original sender
            Comport = COM.Text;
            if (Measuring_class.Set_Port(Comport))  //Set the Port
                trayIcon.ContextMenu.MenuItems[SensorCount + 1].Text = Comport + " Sending (Close?)";
            else
                trayIcon.ContextMenu.MenuItems[SensorCount+1].Text = Comport + " CLOSED, Retrying";
            trayIcon.ContextMenu.MenuItems[SensorCount+1].Enabled = true;   //Enable button for Toggling.
            port_open = true;
        }
        private void Toggle_Com(object sender, EventArgs e)
        {
            //Toggles Serial Communication. Not elegant as it reaches straight into the OHM class.
            if (port_open)
            {
                port_open = false;
                Measuring_class.Close_Port();
                trayIcon.ContextMenu.MenuItems[SensorCount + 1].Text = Comport + " CLOSED(open?)";
            } else {
                port_open = true;
                if (Measuring_class.Try_open())
                    trayIcon.ContextMenu.MenuItems[SensorCount + 1].Text = Comport + " Sending (Close?)";
                else
                    trayIcon.ContextMenu.MenuItems[SensorCount + 1].Text = Comport + " Failed, Retrying...";
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            // Hide and dispose the trayIcon, close the monitor, dispose yourself, Exit the program.
            // trayIcon.Visible = false;
            trayIcon.Dispose();
            Measuring_class.Close_monitor();
            Dispose(true);
            Application.Exit();
        }

        private static void Monitor(Object myObject,EventArgs myEventArgs)
        {
            //Timed Event. Should update all the sensors.  Add/Remove sensor readouts as needed (
            Measuring_class.GetSystemInfo();
            trayIcon.ContextMenu.MenuItems[0].Text = "CPU : " + Measuring_class.Sensor_1/2 + "%";
            trayIcon.ContextMenu.MenuItems[1].Text = "MEM : " + Measuring_class.Sensor_2/2 + "%";
            trayIcon.ContextMenu.MenuItems[2].Text = "GPU : " + Measuring_class.Sensor_3_RAW; //
            //If set to send, but no port is open. Try opening it.
            if (!Measuring_class.Sending && port_open)
            {
                if (Measuring_class.Try_open())
                    trayIcon.ContextMenu.MenuItems[SensorCount+1].Text = Comport + " Sending (Close?)";
                else
                    trayIcon.ContextMenu.MenuItems[SensorCount+1].Text = Comport + " Failed, Retrying...";
            }
        }
    }
}
