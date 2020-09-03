using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace BluetoothSampleUWP
{
    class BluetoothController
    {
        public List<string> ListBluetoothID;
        public bool IsWatchingBluetoothDeviceCompleted;

        public BluetoothController()
        {
            ListBluetoothID = new List<string>();
        }
        public void SearchBluetoothDevice(TextBlock textBlock)
        {
            /*string[] requestedProperties = new string[] { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            DeviceWatcher deviceWatcher = DeviceInformation.CreateWatcher("(System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")",
                                                            requestedProperties,
                                                            DeviceInformationKind.AssociationEndpoint);*/

            DeviceWatcher deviceWatcher = DeviceInformation.CreateWatcher(BluetoothDevice.GetDeviceSelectorFromPairingState(false));

            textBlock.Text = "Available Device List";

            deviceWatcher.Added += async (DeviceWatcher sender, DeviceInformation device) =>
            {
                ListBluetoothID.Add(device.Id);
                // To update textBlock on UI thread
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync
                (CoreDispatcherPriority.Normal, () =>
                    {
                        // Your UI update code goes here!
                        textBlock.Text += "\n" + device.Id + "     " + device.Name;
                    }
                );
            };

            deviceWatcher.EnumerationCompleted += (DeviceWatcher sender, object args) =>
            {
                IsWatchingBluetoothDeviceCompleted = true;
            };

            deviceWatcher.Start();
        }
    }
}
