using CommonClass.Model;
using CommonClass.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace DataUsage.Controls
{
    public sealed partial class ConnectionTypeImageUserControl : UserControl
    {
        public MainViewModel ViewModel { get; set; } = new MainViewModel();

        public ConnectionTypeImageUserControl()
        {
            this.InitializeComponent();
        }

        private ConnectionType _type;

        public ConnectionType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                switch (_type)
                {
                    case ConnectionType.WiFi:
                        PathImageWiFi.Visibility = Visibility.Visible;
                        break;
                    case ConnectionType.Cellular:
                        PathImageCellular.Visibility = Visibility.Visible;
                        break;
                    case ConnectionType.Ethernet:
                        PathImageEthernet.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

    }
}
