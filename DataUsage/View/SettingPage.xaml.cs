using CommonClass;
using CommonClass.Model;
using DataUsage.Controls;
using CommonClass.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Background;
using Windows.Globalization;
using Windows.UI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DataUsage.View
{
    public sealed partial class SettingPage : Page
    {
        public MainViewModel ViewModel { get; set; } = MainPage.MainViewModel;
        public SettingPage()
        {
            this.InitializeComponent();                                 
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                await GetSettingAsync();
                GetDiscountSettings();
            }
            catch (Exception ex)
            {
                ex.Source = ex.Source.Contains(".../") ? ex.Source : "DataUsage.View.SettingPage.OnNavigatedTo";
                ExceptionHandling.ShowErrorMessageAsync(ex);
            }
        }
        private async void BtnAddDiscount_Click(object sender, RoutedEventArgs e)
        {
            var discountContentDialog = new DiscountContentDialog();
            discountContentDialog.CreateUniqueID();
            var result = await discountContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                AddDiscountToItemControl(discountContentDialog.Data);
                SetDiscountSettings();
            }
            else
            { /*Do nothing */ }
        }
        private async void Discount_EditClickedAsync(object sender, EventArgs e)
        {
            DiscountUserControl control = sender as DiscountUserControl;
            if (control == null)
                return;

            var discountContentDialog = new DiscountContentDialog();
            discountContentDialog.PrimaryButtonText = "Edit";
            discountContentDialog.Data = control.Data;
            var result = await discountContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                control.Data = discountContentDialog.Data;
                SetDiscountSettings();
            }
            else
            { /*Do nothing */ }
        }
        private void AddDiscountToItemControl(DiscountControl discountControl)
        {
            var discountUserControl = new DiscountUserControl() { Margin = new Thickness(0, 0, 0, 5) , Data = discountControl };
            discountUserControl.EditClicked += Discount_EditClickedAsync;
            discountUserControl.DeleteClicked += Discount_DeleteClicked;
            ListDiscounts.Items.Add(discountUserControl);
        }
        private void Discount_DeleteClicked(object sender, EventArgs e)
        {
            DiscountUserControl control = sender as DiscountUserControl;
            if (control == null)
                return;

            ListDiscounts.Items.Remove(control);
            SetDiscountSettings();
        }
        private void GetDiscountSettings()
        {
            List<DiscountControl> listDiscounts = LocalSetting.GetSettingPageDiscounts();
            foreach (var item in listDiscounts)
                AddDiscountToItemControl(item);
        }
        private void SetDiscountSettings()
        {
            var listDiscount = new List<DiscountControl>();
            foreach (var item in ListDiscounts.Items)
                listDiscount.Add(((DiscountUserControl)item).Data);
            LocalSetting.SetSettingPageDiscounts(listDiscount);
        }
        private async Task GetSettingAsync()
        {
            ComboBoxCalendarType.SelectedIndex = LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "CalendarType").ToString() == "Gregorian" ? 0 : 1;
            DatePickerPeriod.CalendarIdentifier = ComboBoxCalendarType.SelectedIndex == 0 ? "GregorianCalendar" : "PersianCalendar";

            if (LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "DefaultAppColor").ToString() == "Light")
                RadioLight.IsChecked = true;
            else
                RadioDark.IsChecked = true;
            toggleTile.IsOn = (LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "Tile").ToString() == "True") ? true : false;
            toggleNotification.IsOn = (LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "Notification").ToString() == "True") ? true : false;
            string[] NotificationDataLimit = LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "NotificationDataLimit").ToString().Split(' ');
            string[] NotificationDailyUsage = LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "NotificationDailyUsage").ToString().Split(' ');
            txtDataLimit.Text = NotificationDataLimit[0];
            txtDataLimitUnit.SelectedIndex = (NotificationDataLimit[1] == "MB") ? 1 : 0;
            txtDailyUsage.Text = NotificationDailyUsage[0];
            txtDailyUsageUnit.SelectedIndex = (NotificationDailyUsage[1] == "MB") ? 1 : 0;
            TimePickerDayTimeStart.Time = TimeSpan.Parse(LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "DayTimeStartTime").ToString());
            TimePickerDayTimeEnd.Time = TimeSpan.Parse(LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "DayTimeEndTime").ToString());
            var requestAccess = await BackgroundExecutionManager.RequestAccessAsync();
            lblBackgroundTaskError.Visibility = (requestAccess == BackgroundAccessStatus.DeniedBySystemPolicy ||
                                                requestAccess == BackgroundAccessStatus.DeniedByUser) ? Visibility.Visible : Visibility.Collapsed;

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
                var selectedConnection = LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "TileSelectedConection").ToString();
                if (selectedConnection != "")
                {
                    var item = ComboBoxConnections.Items.Where(conn => ((NetworkProfile)conn).Name == selectedConnection).ToList();
                    if (item.Count == 0) // Maybe connection disabled so we can't find it in list anymore.
                        LocalSetting.SetLocalSetting(LocalSetting.SettingPageGeneralContainer, "TileSelectedConection", "");
                    else
                        ComboBoxConnections.SelectedItem = item.First();
                }
            }

            // Add Connection in the pivot tab Connections
            var networkProfiles = Network.GetConnectionProfiles(GetActivateNetwork: false);
            if (networkProfiles == null)
                ListConnections.Items.Add(new TextBlock { Text = "No Connection found", FontSize = ViewModel.MainFont });
            else
            {
                foreach (var networkProfile in networkProfiles)
                {
                    var connectionUserControl = new ConnectionUserControl
                    {
                        ConnectionName = networkProfile.Name,
                        Nickname = networkProfile.Nickname,
                        PathImageType = networkProfile.Type,
                        Active = networkProfile.Active,
                        Calculate = networkProfile.Calculate,
                        PathSize = (VisualStateGroup.CurrentState.Name == "VisualStateMin0") ? 50 : 75,
                    };
                    connectionUserControl.ConnectionValuesChanged += ListConnectionValueChanged;
                    connectionUserControl.Margin = new Thickness(0, 0, 0, 5);
                    ListConnections.Items.Add(connectionUserControl);
                }
            }

            toggleDataPlan.IsOn = LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "DataPlanEnabled").ToString() == "True" ? true : false;
            txtData.Text = LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "Data").ToString();
            listDataPlanUnit.SelectedIndex = LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "DataUnit").ToString() == "GB" ? 0 : 1;            

            var period = LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "Period").ToString();
            var periodCustom = int.Parse(LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "PeriodText").ToString());
            foreach (ComboBoxItem item in ComboBoxPeriod.Items)
            {
                if (item.Content.ToString() == period)
                {
                    ComboBoxPeriod.SelectedItem = item;
                }
            }

            var startTime = DateTimeOffset.Parse(LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "StartTime").ToString());
            var customStartTime = Network.UpdateStartTimeDate(new CustomDateTime(CalendarIdentifiers.Gregorian, startTime.Year, startTime.Month, startTime.Day), period, periodCustom, true);
            DatePickerPeriod.Date = new DateTimeOffset(customStartTime.ConvertToDateTime(false));
            DatePickerPeriod.MinDate = DateTimeOffset.Now.AddYears(-1);
            DatePickerPeriod.MaxDate = DateTimeOffset.Now;
        }

        private async void ListConnectionValueChanged(object sender, EventArgs e)
        {
            var connectionChanged = sender as ConnectionUserControl;
            List<NetworkProfile> connectionList = new List<NetworkProfile>();
            bool calculateConnectionExist = false;
            foreach (var item in ListConnections.Items)
            {
                var itemProperty = ((ConnectionUserControl)item);
                if (itemProperty.Active == true && itemProperty.Calculate == true)
                    calculateConnectionExist = true;
                connectionList.Add(new NetworkProfile { Name = itemProperty.ConnectionName, Nickname = itemProperty.Nickname, Active = itemProperty.Active, Calculate = itemProperty.Calculate, });
            }
            if (calculateConnectionExist)
                LocalSetting.SetSettingPageConnections(connectionList);
            else
            {                
                var messageDialog = new MessageDialog("At least, You must have one calculate connection.", "Warning");
                messageDialog.Commands.Add(new UICommand("OK"));
                // Show the message dialog
                await messageDialog.ShowAsync();
            }
        }

        private void ComboBoxCalendarType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LocalSetting.SetLocalSetting(LocalSetting.SettingPageGeneralContainer, "CalendarType", ComboBoxCalendarType.SelectedIndex == 0 ? "Gregorian" : "Persian");
            DatePickerPeriod.CalendarIdentifier = ComboBoxCalendarType.SelectedIndex == 0 ? "GregorianCalendar" : "PersianCalendar";
        }

        private async void RadioButtonAppMode_Click(object sender, RoutedEventArgs e)
        {
            var settingAppColor = LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "DefaultAppColor").ToString();
            var selectedAppColor = RadioLight.IsChecked == true ? "Light" : "Dark";
            if (selectedAppColor != settingAppColor)
            {
                LocalSetting.SetLocalSetting(LocalSetting.SettingPageGeneralContainer, "DefaultAppColor", selectedAppColor);

                var messageDialog = new MessageDialog("You must restart app for applying theme.", "Warning");
                messageDialog.Commands.Add(new UICommand("OK", new UICommandInvokedHandler(CommandInvokedHandler)));
                // Show the message dialog
                await messageDialog.ShowAsync();
            }
        }

        private void CommandInvokedHandler(IUICommand command) => CoreApplication.Exit();

        private async void toggleTile_Toggled(object sender, RoutedEventArgs e)
        {
            LocalSetting.SetLocalSetting(LocalSetting.SettingPageGeneralContainer, "Tile", toggleTile.IsOn.ToString());

            if (toggleTile.IsOn)
                await BackgroundTaskExecution.RegisterBackgroundTask(BackgroundTaskExecution.TileBackgroudnTaskName, 15);
            else
            {
                BackgroundTaskExecution.UnRegisterBackgroundTask(BackgroundTaskExecution.TileBackgroudnTaskName);
                ComboBoxConnections.SelectedIndex = -1;
            }
        }

        private async void toggleNotification_Toggled(object sender, RoutedEventArgs e)
        {
            LocalSetting.SetLocalSetting(LocalSetting.SettingPageGeneralContainer, "Notification", toggleNotification.IsOn.ToString());

            if (toggleNotification.IsOn)
                await BackgroundTaskExecution.RegisterBackgroundTask(BackgroundTaskExecution.NotificationBackgroudnTaskName, 12*60);
            else
                BackgroundTaskExecution.UnRegisterBackgroundTask(BackgroundTaskExecution.NotificationBackgroudnTaskName);
        }
        private void txtDataLimit_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool error = false;
            double dataLimit = 0;
            if (!(double.TryParse(txtDataLimit.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out dataLimit)))
                error = true;
            if (dataLimit <= 0)
                error = true;

            if (error)
                ShowError("invalid data limit number!", ShowErrorlblGeneralAnimation, lblGeneralError);
            else
            {
                DailyUsageAndDataLimitChanged();
                if (lblGeneralError.Text == "invalid data limit number!")
                    lblGeneralError.Text = "";
            }            
        }
        private void txtDailyUsage_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool error = false;
            double dailyUsage = 0;
            if (!(double.TryParse(txtDailyUsage.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out dailyUsage)))
                error = true;
            if (dailyUsage <= 0)
                error = true;

            if (error)
                ShowError("invalid daily usage number!", ShowErrorlblGeneralAnimation, lblGeneralError);
            else
            {
                DailyUsageAndDataLimitChanged();
                if (lblGeneralError.Text == "invalid daily usage number!")
                    lblGeneralError.Text = "";
            }
        }
        private void txtDataLimitUnit_SelectionChanged(object sender, SelectionChangedEventArgs e) => DailyUsageAndDataLimitChanged();

        private void txtDailyUsageUnit_SelectionChanged(object sender, SelectionChangedEventArgs e) => DailyUsageAndDataLimitChanged();

        private void DailyUsageAndDataLimitChanged()
        {
            LocalSetting.SetLocalSetting(LocalSetting.SettingPageGeneralContainer, "NotificationDataLimit", string.Format("{0} {1}", txtDataLimit.Text, txtDataLimitUnit.SelectedIndex == 0 ? "GB" : "MB"));
            LocalSetting.SetLocalSetting(LocalSetting.SettingPageGeneralContainer, "NotificationDailyUsage", string.Format("{0} {1}", txtDailyUsage.Text, txtDailyUsageUnit.SelectedIndex == 0 ? "GB" : "MB"));
        }
        private void TimePickerDayTimeStart_TimeChanged(object sender, TimePickerValueChangedEventArgs e) => LocalSetting.SetLocalSetting(LocalSetting.SettingPageGeneralContainer, "DayTimeStartTime", TimePickerDayTimeStart.Time.ToString());

        private void TimePickerDayTimeEnd_TimeChanged(object sender, TimePickerValueChangedEventArgs e) => LocalSetting.SetLocalSetting(LocalSetting.SettingPageGeneralContainer, "DayTimeEndTime", TimePickerDayTimeEnd.Time.ToString());

        private void toggleDataPlan_Toggled(object sender, RoutedEventArgs e) => LocalSetting.SetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "DataPlanEnabled", toggleDataPlan.IsOn.ToString());

        private void DatePickerPeriod_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            var startTime = DatePickerPeriod.Date.GetValueOrDefault(DateTimeOffset.Now).ToString("o");
            LocalSetting.SetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "StartTime", startTime);
            CheckValidDataPlan();
        }
        private void ComboBoxPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ComboBoxPeriod.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
                return;

            LocalSetting.SetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "Period", selectedItem.Content.ToString());
            txtPeriodCustom.Text = LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "PeriodText").ToString();

            if (lblDataPlanError.Text == "invalid period number! The value must lower than or equal to 60.")
                lblDataPlanError.Text = "";
            txtPeriodCustom.Visibility = selectedItem.Content.ToString() == "Custom" ? Visibility.Visible : Visibility.Collapsed;
            if (selectedItem.Content.ToString() != "Custom")
            {
                if (lblDataPlanError.Text == "invalid period number!")
                    lblDataPlanError.Text = "";
            }
            CheckValidDataPlan();
        }

        private void ComboBoxConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedConnection = (ComboBoxConnections.SelectedItem as NetworkProfile)?.Name;
            LocalSetting.SetLocalSetting(LocalSetting.SettingPageGeneralContainer, "TileSelectedConection", selectedConnection == null ? "" : selectedConnection);
        }

        private void listDataPlanUnit_SelectionChanged(object sender, SelectionChangedEventArgs e) => LocalSetting.SetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "DataUnit", ((ListViewItem)listDataPlanUnit.SelectedItem).Content.ToString());

        private void txtData_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool error = false;
            double data = 0;
            if (!(double.TryParse(txtData.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out data)))
                error = true;
            if (data <= 0)
                error = true;

            if (error)
                ShowError("invalid data number!", ShowErrorlblDataPlanAnimation, lblDataPlanError);
            else
            {
                LocalSetting.SetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "Data", txtData.Text);
                if (lblDataPlanError.Text == "invalid data number!")
                {
                    lblDataPlanError.Text = "";
                }
            }
        }

        private void ShowError(string message, Storyboard animation, TextBlock lblError)
        {
            lblError.Text = message;
            animation.Begin();
        }

        private void txtPeriodCustom_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((ComboBoxPeriod.SelectedItem as ComboBoxItem)?.Content.ToString() != "Custom")
                return;
            int periodCustom = GetTxtPeriodCustom();
            if (periodCustom <= 0 || periodCustom > 60)
                return;

            LocalSetting.SetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "PeriodText", periodCustom.ToString());
            CheckValidDataPlan();
        }

        private int GetTxtPeriodCustom()
        {
            bool error = false;
            int periodCustom = 0;
            if (!(int.TryParse(txtPeriodCustom.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out periodCustom)))
                error = true;
            if (periodCustom <= 0 || periodCustom > 50)
                error = true;
            
            if (error)
                ShowError("invalid period number! The value must lower than or equal to 50.", ShowErrorlblDataPlanAnimation, lblDataPlanError);
            else
            {
                if (lblDataPlanError.Text == "invalid period number! The value must lower than or equal to 50.")
                    lblDataPlanError.Text = "";
            }

            return periodCustom;
        }

        /// <summary>
        /// Check the data plan start time and end time of that. it must be after today time.
        /// </summary>
        private void CheckValidDataPlan()
        {
            var calendarType = (ComboBoxCalendarType.SelectedItem as ComboBoxItem)?.Content.ToString() == "Gregorian" ? CalendarIdentifiers.Gregorian : CalendarIdentifiers.Persian;
            var startTime = DatePickerPeriod.Date.GetValueOrDefault(DateTimeOffset.Now);

            var fromCustomDateTime = GlobalCalendar.ConvertDateTo(new CustomDateTime(CalendarIdentifiers.Gregorian, startTime.Year, startTime.Month, startTime.Day), calendarType);
            var toCustomDateTime = Network.GetToDateTime(fromCustomDateTime);
            var customDateTimeNow = GlobalCalendar.ConvertDateTo(new CustomDateTime(CalendarIdentifiers.Gregorian, DateTime.Now), calendarType);
            if (customDateTimeNow.GetCompareTotalDays(toCustomDateTime) < 0)
                lblDataPlanError.Text = "Your period is over. We automatically change start time based on your period.";
            else
            {
                if (lblDataPlanError.Text == "Your period is over. We automatically change start time based on your period.")
                    lblDataPlanError.Text = "";
            }
        }

        private void VisualStateGroup_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            var pathSize = (VisualStateGroup.CurrentState.Name == "VisualStateMin0") ? 50 : 75;
            foreach (ConnectionUserControl connection in ListConnections.Items)
                connection.PathSize = pathSize;
        }

        
    }   
}
