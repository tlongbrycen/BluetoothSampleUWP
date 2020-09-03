using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BluetoothSampleUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        BluetoothModel bteModel;
        BluetoothController bteCotroller;
        public MainPage()
        {
            this.InitializeComponent();
            bteModel = new BluetoothModel();
            bteCotroller = new BluetoothController();
        }

        private void btnInitServer_Click(object sender, RoutedEventArgs e)
        {
            bteModel.InitializeServer();
        }

        private void btnInitClient_Click(object sender, RoutedEventArgs e)
        {
            bteModel.InitializeClient();
        }

        private void btnWatching_Click(object sender, RoutedEventArgs e)
        {
            bteCotroller.SearchBluetoothDevice(this.txtBlk1);
        }
    }
}
