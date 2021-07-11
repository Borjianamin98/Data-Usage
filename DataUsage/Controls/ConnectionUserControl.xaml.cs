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
    public sealed partial class ConnectionUserControl : UserControl
    {
        public event EventHandler ConnectionValuesChanged;
        public MainViewModel ViewModel { get; set; } = new MainViewModel();
        public ConnectionUserControl()
        {
            this.InitializeComponent();
        }

        public string Nickname
        {
            get { return (string)GetValue(NicknameProperty); }
            set { SetValue(NicknameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Nickname.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NicknameProperty =
            DependencyProperty.Register("Nickname", typeof(string), typeof(ConnectionUserControl), new PropertyMetadata("", NicknameChanged));

        private static void NicknameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var userControl = d as ConnectionUserControl;
            var newNickname = e.NewValue.ToString();
            userControl.lblNickname.Text = newNickname.Substring(0, newNickname.Length > 25 ? 25 : newNickname.Length);
            if (e.NewValue.ToString() != e.OldValue.ToString())
                userControl.OnConnectionValuesChanged();
        }

        public ConnectionType PathImageType
        {
            get { return (ConnectionType)GetValue(PathImageTypeProperty); }
            set { SetValue(PathImageTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PathImageType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PathImageTypeProperty =
            DependencyProperty.Register("PathImageType", typeof(ConnectionType), typeof(ConnectionUserControl), new PropertyMetadata(ConnectionType.WiFi));

        public string ConnectionName
        {
            get { return (string)GetValue(ConnectionNameProperty); }
            set { SetValue(ConnectionNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnectionName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectionNameProperty =
            DependencyProperty.Register("ConnectionName", typeof(string), typeof(ConnectionUserControl), new PropertyMetadata(""));

        public bool Active
        {
            get { return (bool)GetValue(ActiveProperty); }
            set { SetValue(ActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Active.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveProperty =
            DependencyProperty.Register("Active", typeof(bool), typeof(ConnectionUserControl),
                new PropertyMetadata(false, new PropertyChangedCallback((d, e) =>
                {
                    if ((bool)e.NewValue != (bool)e.OldValue)
                        (d as ConnectionUserControl)?.OnConnectionValuesChanged();
                })));

        public bool Calculate
        {
            get { return (bool)GetValue(CalculateProperty); }
            set { SetValue(CalculateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Calculate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CalculateProperty =
            DependencyProperty.Register("Calculate", typeof(bool), typeof(ConnectionUserControl),
                new PropertyMetadata(false, new PropertyChangedCallback((d, e) => 
                {
                    if ((bool)e.NewValue != (bool)e.OldValue)
                        (d as ConnectionUserControl)?.OnConnectionValuesChanged();
                })));

        private double _pathSize;

        public double PathSize
        {
            get { return _pathSize; }
            set
            {
                _pathSize = value;
                PathImage.Width = _pathSize;
                PathImage.Height = _pathSize;
            }
        }

        private void OnConnectionValuesChanged()
        {
            ConnectionValuesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
