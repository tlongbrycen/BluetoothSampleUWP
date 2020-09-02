using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace BluetoothSampleUWP
{
    class BluetoothModel
    {
        //Send a file as a client
        Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService _service;
        Windows.Networking.Sockets.StreamSocket _socket;

        //Receive File as a Server
        Windows.Devices.Bluetooth.Rfcomm.RfcommServiceProvider _provider;

        //Send a file as a client
        public async void InitializeClient()
        {
            // Enumerate devices with the object push service
            var services =
                await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(
                    RfcommDeviceService.GetDeviceSelector(
                        RfcommServiceId.ObexObjectPush));

            if (services.Count > 0)
            {
                // Initialize the target Bluetooth BR device
                var service = await RfcommDeviceService.FromIdAsync(services[0].Id);

                bool isCompatibleVersion = await IsCompatibleVersionAsync(service);

                // Check that the service meets this App's minimum requirement
                if (SupportsProtection(service) && isCompatibleVersion)
                {
                    _service = service;

                    // Create a socket and connect to the target
                    _socket = new StreamSocket();
                    await _socket.ConnectAsync(
                        _service.ConnectionHostName,
                        _service.ConnectionServiceName,
                        SocketProtectionLevel
                            .BluetoothEncryptionAllowNullAuthentication);

                    // The socket is connected. At this point the App can wait for
                    // the user to take some action, for example, click a button to send a
                    // file to the device, which could invoke the Picker and then
                    // send the picked file. The transfer itself would use the
                    // Sockets API and not the Rfcomm API, and so is omitted here for
                    // brevity.
                }
            }
        }

        // This App requires a connection that is encrypted but does not care about
        // whether it's authenticated.
        bool SupportsProtection(RfcommDeviceService service)
        {
            switch (service.ProtectionLevel)
            {
                case SocketProtectionLevel.PlainSocket:
                    if ((service.MaxProtectionLevel == SocketProtectionLevel
                            .BluetoothEncryptionWithAuthentication)
                        || (service.MaxProtectionLevel == SocketProtectionLevel
                            .BluetoothEncryptionAllowNullAuthentication))
                    {
                        // The connection can be upgraded when opening the socket so the
                        // App may offer UI here to notify the user that Windows may
                        // prompt for a PIN exchange.
                        return true;
                    }
                    else
                    {
                        // The connection cannot be upgraded so an App may offer UI here
                        // to explain why a connection won't be made.
                        return false;
                    }
                case SocketProtectionLevel.BluetoothEncryptionWithAuthentication:
                    return true;
                case SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication:
                    return true;
            }
            return false;
        }

        // This App relies on CRC32 checking available in version 2.0 of the service.
        const uint _SERVICE_VERSION_ATTRIBUTE_ID = 0x0300;
        const byte _SERVICE_VERSION_ATTRIBUTE_TYPE = 0x0A;   // UINT32
        const uint MINIMUM_SERVICE_VERSION = 200;
        async Task<bool> IsCompatibleVersionAsync(RfcommDeviceService service)
        {
            var attributes = await service.GetSdpRawAttributesAsync(
                BluetoothCacheMode.Uncached);
            var attribute = attributes[SERVICE_VERSION_ATTRIBUTE_ID];
            var reader = DataReader.FromBuffer(attribute);

            // The first byte contains the attribute's type
            byte attributeType = reader.ReadByte();
            if (attributeType == SERVICE_VERSION_ATTRIBUTE_TYPE)
            {
                // The remainder is the data
                uint version = reader.ReadUInt32();
                return version >= MINIMUM_SERVICE_VERSION;
            }
            else return false;
        }

        //Receive File as a Server
        public async void InitializeServer()
        {
            // Initialize the provider for the hosted RFCOMM service
            _provider =
                await Windows.Devices.Bluetooth.Rfcomm.RfcommServiceProvider.CreateAsync(
                    RfcommServiceId.ObexObjectPush);

            // Create a listener for this service and start listening
            StreamSocketListener listener = new StreamSocketListener();
            listener.ConnectionReceived += OnConnectionReceivedAsync;
            await listener.BindServiceNameAsync(
                _provider.ServiceId.AsString(),
                SocketProtectionLevel
                    .BluetoothEncryptionAllowNullAuthentication);

            // Set the SDP attributes and start advertising
            InitializeServiceSdpAttributes(_provider);
            _provider.StartAdvertising(listener);
        }

        const uint SERVICE_VERSION_ATTRIBUTE_ID = 0x0300;
        const byte SERVICE_VERSION_ATTRIBUTE_TYPE = 0x0A;   // UINT32
        const uint SERVICE_VERSION = 200;

        void InitializeServiceSdpAttributes(RfcommServiceProvider provider)
        {
            Windows.Storage.Streams.DataWriter writer = new Windows.Storage.Streams.DataWriter();

            // First write the attribute type
            writer.WriteByte(SERVICE_VERSION_ATTRIBUTE_TYPE);
            // Then write the data
            writer.WriteUInt32(MINIMUM_SERVICE_VERSION);

            IBuffer data = writer.DetachBuffer();
            provider.SdpRawAttributes.Add(SERVICE_VERSION_ATTRIBUTE_ID, data);
        }

        void OnConnectionReceivedAsync(
            StreamSocketListener listener,
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            // Stop advertising/listening so that we're only serving one client
            _provider.StopAdvertising();
            listener.Dispose();
            _socket = args.Socket;

            // The client socket is connected. At this point the App can wait for
            // the user to take some action, for example, click a button to receive a file
            // from the device, which could invoke the Picker and then save the
            // received file to the picked location. The transfer itself would use
            // the Sockets API and not the Rfcomm API, and so is omitted here for
            // brevity.
        }
    }
}
