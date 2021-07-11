using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Media.Imaging;

namespace CommonClass.Model
{
    public enum ConnectionType { WiFi = 1, Cellular = 2, Ethernet = 3 }

    public class NetworkProfile
    {
        public string Name { get; set; }

        private string _nickname = "";
        public string Nickname
        {
            get
            {
                if (_nickname == "")
                    return Name;
                else
                    return _nickname;
            }
            set { _nickname = value; }
        }

        public ConnectionType Type { get; set; }

        public NetworkAuthenticationType AuthenticationType { get; set; }

        public NetworkEncryptionType EncryptionType { get; set; }

        public NetworkConnectivityLevel ConnectivityLevel { get; set; }

        public bool Calculate { get; set; } = true;

        public bool Active { get; set; } = true;
    }
}
