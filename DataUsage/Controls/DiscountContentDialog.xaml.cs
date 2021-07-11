using CommonClass;
using CommonClass.Model;
using CommonClass.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DataUsage.Controls
{
    public sealed partial class DiscountContentDialog : ContentDialog
    {
        public MainViewModel ViewModel { get; set; } = new MainViewModel();

        public int DiscountID { get; set; }

        public DiscountControl Data
        {
            get
            {
                var data = new DiscountControl() {
                    ID = DiscountID,
                    Discount = double.Parse(txtDiscount.Text.Split(' ')[0], CultureInfo.InvariantCulture),
                    ConnectionName = ((NetworkProfile)(ComboBoxConnections.SelectedItem)).Name,
                    StartTime = TimePickerStart.Time,
                    EndTime = TimePickerEnd.Time,
                };
                return data;
            }
            set
            {
                AddConnections();
                var connectionProfile = ComboBoxConnections.Items.Where(conn => ((NetworkProfile)conn).Name == value.ConnectionName).ToList();
                if (connectionProfile.Count != 0)
                    ComboBoxConnections.SelectedItem = connectionProfile.First();
                DiscountID = value.ID;
                txtDiscount.Text = value.Discount.ToString();
                TimePickerStart.Time = value.StartTime;
                TimePickerEnd.Time = value.EndTime;
            }
        }

        public DiscountContentDialog()
        {
            this.InitializeComponent();
        }

        public void CreateUniqueID()
        {
            // Create Unique ID
            var sameConnections = LocalSetting.GetSettingPageDiscounts().Select(x => x.ID).ToList();
            int temp = 1;
            while (true)
            {
                if (!sameConnections.Contains(temp))
                {
                    DiscountID = temp;
                    break;
                }
                temp++;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            bool error = false;
            double discount = 0;
            if (!(double.TryParse(txtDiscount.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out discount)))
                error = true;
            if (discount <= 0 || discount > 100)
                error = true;

            if(error)
            {
                lblError.Text = "invalid discount";
                args.Cancel = true;
            }
            if (ComboBoxConnections.SelectedIndex == -1)
            {
                lblError.Text = "Select a connection from list";
                args.Cancel = true;
            }
            if (TimeSpan.Compare(TimePickerStart.Time, TimePickerEnd.Time) >= 0)
            {
                lblError.Text = "Start Time must before Finish Time";
                args.Cancel = true;
            }

            // Check valid time based other same connections (with this name) and their times.
            var sameConnections = LocalSetting.GetSettingPageDiscounts().Where(x => x.ConnectionName == ((NetworkProfile)(ComboBoxConnections.SelectedItem)).Name).ToList();
            foreach (var conn in sameConnections)
            {
                if (conn.ID == DiscountID)
                    continue;
                var contentdialogPeriod = new Period(new DateTimeOffset(DateTime.Now.Date).Add(TimePickerStart.Time), new DateTimeOffset(DateTime.Now.Date).Add(TimePickerEnd.Time));
                var connPeriod = new Period(new DateTimeOffset(DateTime.Now.Date).Add(conn.StartTime), new DateTimeOffset(DateTime.Now.Date).Add(conn.EndTime));
                var intersectionPeriods = Period.IntersectionTwoPeriod(contentdialogPeriod, connPeriod);
                if (intersectionPeriods != null)
                {
                    lblError.Text = "this period is intersected with period of another same connection.";
                    args.Cancel = true;
                }
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Nothing
        }

        private void DiscountDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (ComboBoxConnections.ItemsSource == null)
                AddConnections();
        }

        private void AddConnections()
        {
            //
            // Add connection to ComboBoxConnections
            //
            var connectionList = Network.GetConnectionProfiles(GetActivateNetwork: true);
            if (connectionList == null)
            {
                ComboBoxConnections.PlaceholderText = "No Connection found";
                ComboBoxConnections.IsEnabled = false;
            }
            else
            {
                ComboBoxConnections.PlaceholderText = "Choose Connection";
                ComboBoxConnections.ItemsSource = connectionList;
            }
        }
    }
}
