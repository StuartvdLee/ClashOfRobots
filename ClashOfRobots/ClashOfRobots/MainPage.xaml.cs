using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Contracts;


namespace ClashOfRobots
{
    public partial class MainPage : ContentPage
    {

        bool scanning = false;
        bool writing = false;
        IAdapter Adapter { get { return CrossBluetoothLE.Current.Adapter; } }

        IDevice connectedDevice;
        IService mainService;
        ICharacteristic mainCharacteristic;

        public MainPage()
        {
            InitializeComponent();

            Adapter.DeviceConnected += (s, e) => OnDeviceConnected(s, e);
            SetButtons();
        }

        void SetButtons()
        {

        }

        void OnForward(object sender, EventArgs e)
        {

            TryWrite("f");
        }
        void OnStop(object sender, EventArgs e)
        {
            TryWrite("s");
        }

        void OnScan(object sender, EventArgs e)
        {
            if (scanning == false)
            {
                Scan();
            }
        }
        void OnCyclePaired(object sender, EventArgs e)
        {
            if (scanning == false)
                CyclePaired();
        }
        void OnDisconnect(object sender, EventArgs e)
        {
            foreach (IDevice device in Adapter.ConnectedDevices)
            {
                Adapter.DisconnectDeviceAsync(device);
            }
        }


        async void Scan()
        {
            Console.WriteLine("START SCANNING");
            scanning = true;
            var adapter = CrossBluetoothLE.Current.Adapter;
            adapter.ScanTimeout = 10000;

            adapter.DeviceDiscovered += (s, a) => FoundDevice(s, a);
            await adapter.StartScanningForDevicesAsync();
            scanning = false;
            Console.WriteLine("STOP SCANNING");
        }

        async void CyclePaired()
        {
            scanning = true;
            Console.WriteLine("START CYCLE");

            var systemDevices = Adapter.GetSystemConnectedOrPairedDevices();
            foreach (var device in systemDevices)
            {
                Console.WriteLine("CYCLE FOUND " + device.Name);

                if (device.Name == "Kijkdoos135")
                {
                    Console.WriteLine("CYCLE CONNECT TO " + device.Name);
                    try
                    {
                        await Adapter.ConnectToDeviceAsync(device);
                    }
                    catch (DeviceConnectionException dce)
                    {
                        Console.WriteLine("EXCEPTION " + dce);
                    }
                }
            }

            Console.WriteLine("END CYCLE");
            scanning = false;
        }

        void FoundDevice(Object s, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs a)
        {
            Console.WriteLine("FOUND DEVICE: " + a.Device);
            if (a.Device.Name == "Kijkdoos135")
            {
                ConnectDevice(a.Device);
            }
        }

        async void ConnectDevice(IDevice device)
        {
            Console.WriteLine("START CONNECTION TO " + device.Name);
            try
            {
                Console.WriteLine("ATTEMPTING CONNECTION TO " + device.Name);
                await Adapter.ConnectToDeviceAsync(device);
                //await CrossBluetoothLE.Current.Adapter.ConnectToKnownDeviceAsync(device.guid, cancellationToken);
            }
            catch (Plugin.BLE.Abstractions.Exceptions.DeviceConnectionException e)
            {
                Console.WriteLine("COULD NOT CONNECT " + e);
            }
        }

        void OnDeviceConnected(object s, DeviceEventArgs a)
        {
            if (a.Device != null)
            {
                Console.WriteLine("DEVICE CONNECTED " + a.Device.Name);
                connectedDevice = a.Device;
                GetService();
            }
            else
                Console.WriteLine("DEVICE CONNECTED ?");
        }

        async void FindServices()
        {
            if (connectedDevice == null)
                return;

            Console.WriteLine("START FINDSERVICES");
            try
            {
                var services = await connectedDevice.GetServicesAsync();
                foreach (IService service in services)
                {
                    Console.WriteLine("FOUND SERVICE " + service.Name);
                    Console.WriteLine("SERVICE ID " + service.Id);
                }
            }
            catch { }
            Console.WriteLine("STOP FINDSERVICES");
        }

        async void GetService()
        {
            Console.WriteLine("START GETSERVICE");
            try
            {
                mainService = await connectedDevice.GetServiceAsync(Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb"));

                if (mainService != null)
                {
                    Console.WriteLine("FOUND SERVICE " + mainService.Name);
                    GetCharacteristic();
                }
            }
            catch { }
            Console.WriteLine("STOP GETSERVICE");
        }

        async void FindCharacteristics()
        {
            Console.WriteLine("START FINDCHARACTERISTICS");
            try
            {
                var characteristics = await mainService.GetCharacteristicsAsync();

                foreach (ICharacteristic characteristic in characteristics)
                {
                    Console.WriteLine("FOUND CHARACTERISTIC " + characteristic.Name);
                    Console.WriteLine("CHARACTERISTIC ID " + characteristic.Id);
                }
            }
            catch { }
            Console.WriteLine("STOP FINDCHARACTERISTICS");
        }

        async void GetCharacteristic()
        {
            Console.WriteLine("START GETCHARACTERISTIC");
            try
            {
                mainCharacteristic = await mainService.GetCharacteristicAsync(Guid.Parse("0000ffe1-0000-1000-8000-00805f9b34fb"));

                if (mainCharacteristic != null)
                {
                    Console.WriteLine("FOUND CHARACTERISTIC " + mainCharacteristic.Name);
                }
            }
            catch { }
            Console.WriteLine("STOP GETCHARACTERISTIC");
        }

        async void TryWrite(string command)
        {
            if (mainCharacteristic == null || writing == true)
                return;

            writing = true;
            try
            {
                await mainCharacteristic.WriteAsync(Encoding.ASCII.GetBytes(command));
            }
            catch { }
            writing = false;
        }
    }
}
