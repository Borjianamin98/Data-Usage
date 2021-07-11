using CommonClass;
using CommonClass.Model;
using CommonClass.ViewModel;
using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Graphics.Display;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DataUsage.View
{    
    public sealed partial class ChartPage : Page
    {
        public MainViewModel ViewModel { get; set; } = MainPage.MainViewModel;
        public ChartPage()
        {
            this.InitializeComponent();            

            try
            {
                GetSetting();
            }
            catch (Exception ex)
            {
                ex.Source = ex.Source.Contains(".../") ? ex.Source : "Unhandled Exception at DataUsage.View.ChartPage.ChartPage()";
                ExceptionHandling.ShowErrorMessageAsync(ex);
            }
        }

        private void GetSetting()
        {
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Calculate the compass heading offset based on the current display orientation.
            var displayInfo = DisplayInformation.GetForCurrentView();
            displayInfo.OrientationChanged += DisplayInfo_OrientationChanged;
            UpdateDisplayOrientation();
        }
        private void DisplayInfo_OrientationChanged(DisplayInformation sender, object args) => UpdateDisplayOrientation();

        private void UpdateDisplayOrientation()
        {
            // Calculate the compass heading offset based on the current display orientation.
            var displayInfo = DisplayInformation.GetForCurrentView();
            switch (displayInfo.CurrentOrientation)
            {
                case DisplayOrientations.Landscape:
                case DisplayOrientations.LandscapeFlipped:
                    SeriesDownload.IsTransposed = false;
                    SeriesUpload.IsTransposed = false;
                    break;
                case DisplayOrientations.Portrait:
                case DisplayOrientations.PortraitFlipped:
                    SeriesDownload.IsTransposed = true;
                    SeriesUpload.IsTransposed = true;
                    break;
            }
        }

        private void ToggleCrossHair_Click(object sender, RoutedEventArgs e)
        {
            ChartCrossHairBehavior behavior = new ChartCrossHairBehavior();
            if (ToggleCrossHair.IsChecked == true)
                ChartData.Behaviors.Add(behavior);
            else
                ChartData.Behaviors.RemoveAt(1); // index 0 is Zoom behavior and 1 is CrossHair behavior.
        }

        private void AppBarDownload_Click(object sender, RoutedEventArgs e) => ChartData.Series[0].IsSeriesVisible = AppBarDownload.IsChecked == true ? true : false;

        private void AppBarUpload_Click(object sender, RoutedEventArgs e) => ChartData.Series[1].IsSeriesVisible = AppBarUpload.IsChecked == true ? true : false;

        private void ComboBoxDuration_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxMore.SelectionChanged -= ComboBoxMore_SelectionChanged;
            ComboBoxMore.Items.Clear();
            GetChartData();
        }
        private void ComboBoxConnections_SelectionChanged(object sender, SelectionChangedEventArgs e) => GetChartData();
        private void AppBarRefresh_Click(object sender, RoutedEventArgs e) => GetChartData();    
        private void ComboBoxMore_SelectionChanged(object sender, SelectionChangedEventArgs e) => GetChartData();
        private void toggleDiscount_Toggled(object sender, RoutedEventArgs e) => GetChartData();
        private async void GetChartData()
        {
            var selectedConnection = (ComboBoxConnections.SelectedItem as NetworkProfile)?.Name;
            if (selectedConnection == null)
                return;
            var selectedDuration = (ComboBoxDuration.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (selectedDuration == null)
                return;

            var calendarType = LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "CalendarType").ToString() == "Gregorian"? CalendarIdentifiers.Gregorian : CalendarIdentifiers.Persian;
            var calendarTypeFirstOfWeek = calendarType == "GregorianCalendar" ? "Monday" : "Saturday";
            var period = LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "Period").ToString();
            var periodText = int.Parse(LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "PeriodText").ToString());
            var customDateTimeNow = GlobalCalendar.ConvertDateTo(new CustomDateTime(CalendarIdentifiers.Gregorian, DateTime.Now), calendarType);
            var dataPlanStartTime = DateTimeOffset.Parse(LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "StartTime").ToString());
            var dataPlanCustumStartTime = Network.UpdateStartTimeDate(GlobalCalendar.ConvertDateTo(new CustomDateTime(CalendarIdentifiers.Gregorian, dataPlanStartTime.Year, dataPlanStartTime.Month, dataPlanStartTime.Day), calendarType), period, periodText, true);
            var monthNames = calendarType == CalendarIdentifiers.Gregorian ? GlobalCalendar.gregorianMonthName : GlobalCalendar.persianMonthName;
            CustomDateTime startTime, endTime;
            List<ConnectionReport> listNetworkUsage;
            string comboBoxMoreSelected;

            try
            {
                switch (selectedDuration)
                {
                    case "This Month":
                        ComboBoxMore.Visibility = Visibility.Collapsed;
                        startTime = customDateTimeNow.GetFirstDayOfThisMonth();
                        endTime = startTime.AddDays(customDateTimeNow.Day);
                        listNetworkUsage = await Network.GetNetworkUsageAsync(selectedConnection,
                                                                                new DateTimeOffset(startTime.ConvertToDateTime(true)),
                                                                                new DateTimeOffset(endTime.ConvertToDateTime(true)),
                                                                                DataUsageGranularity.PerDay, toggleDiscount.IsOn);
                        for (int i = 1; i <= listNetworkUsage.Count; i++)
                        {
                            listNetworkUsage[i - 1].Date = calendarType == "Gregorian" ? string.Format("{0}/{1}", startTime.Month, startTime.Day) : string.Format("{0}/{1}", startTime.Day, startTime.Month);
                            listNetworkUsage[i - 1].ConvertDataToMB();
                            startTime = startTime.AddDays(1);
                        }

                        UpdateChartItemSource(listNetworkUsage);
                        break;
                    case "This Week":
                        ComboBoxMore.Visibility = Visibility.Collapsed;
                        startTime = customDateTimeNow.GetFirstDayOfThisWeek(calendarTypeFirstOfWeek);
                        endTime = startTime.AddDays((int)startTime.GetCompareTotalDays(customDateTimeNow) + 1);
                        listNetworkUsage = await Network.GetNetworkUsageAsync(selectedConnection,
                                                                                new DateTimeOffset(startTime.ConvertToDateTime(true)),
                                                                                new DateTimeOffset(endTime.ConvertToDateTime(true)),
                                                                                DataUsageGranularity.PerDay, toggleDiscount.IsOn);
                        for (int i = 1; i <= listNetworkUsage.Count; i++)
                        {
                            listNetworkUsage[i - 1].Date = startTime.GetDayOfWeekName();
                            listNetworkUsage[i - 1].ConvertDataToMB();
                            startTime = startTime.AddDays(1);
                        }

                        UpdateChartItemSource(listNetworkUsage);
                        break;
                    case "Today":
                        ComboBoxMore.Visibility = Visibility.Collapsed;
                        startTime = customDateTimeNow.GetFirstHourOfThisDay();
                        endTime = customDateTimeNow;
                        listNetworkUsage = await Network.GetNetworkUsageAsync(selectedConnection,
                                                                                new DateTimeOffset(startTime.ConvertToDateTime(true)),
                                                                                new DateTimeOffset(endTime.ConvertToDateTime(true)),
                                                                                DataUsageGranularity.PerHour, toggleDiscount.IsOn);
                        for (int i = 1; i <= listNetworkUsage.Count; i++)
                        {
                            listNetworkUsage[i - 1].Date = string.Format("{0}-{1}", startTime.Hour, startTime.Hour + 1);
                            listNetworkUsage[i - 1].ConvertDataToMB();
                            startTime = startTime.AddHours(1);
                        }

                        UpdateChartItemSource(listNetworkUsage);
                        break;
                    case "This Hour":
                        ComboBoxMore.Visibility = Visibility.Collapsed;
                        startTime = customDateTimeNow.GetFirstMinuteOfThisHour();
                        endTime = customDateTimeNow;
                        listNetworkUsage = await Network.GetNetworkUsageAsync(selectedConnection,
                                                                                new DateTimeOffset(startTime.ConvertToDateTime(true)),
                                                                                new DateTimeOffset(endTime.ConvertToDateTime(true)),
                                                                                DataUsageGranularity.PerMinute, toggleDiscount.IsOn);
                        for (int i = 1; i <= listNetworkUsage.Count; i++)
                        {
                            listNetworkUsage[i - 1].Date = string.Format("{0}-{1}", startTime.Minute, startTime.Minute + 1);
                            listNetworkUsage[i - 1].ConvertDataToMB();
                            startTime = startTime.AddMinutes(1);
                        }

                        UpdateChartItemSource(listNetworkUsage);
                        break;
                    case "24 Hour":
                        ComboBoxMore.Visibility = Visibility.Visible;
                        if (ComboBoxMore.Items.Count == 0)
                        {
                            for (int i = 0; i <= 23; i++)
                                ComboBoxMore.Items.Add(string.Format("{0}-{1}", i, i + 1));
                            ComboBoxMore.SelectionChanged += ComboBoxMore_SelectionChanged;
                        }

                        comboBoxMoreSelected = ComboBoxMore.SelectedItem as string;
                        if (comboBoxMoreSelected == null)
                            return;

                        startTime = new CustomDateTime(calendarType, customDateTimeNow.Year, customDateTimeNow.Month, customDateTimeNow.Day,
                                                       int.Parse(comboBoxMoreSelected.Split('-')[0]), 0, 0);
                        endTime = startTime.AddHours(1);

                        listNetworkUsage = await Network.GetNetworkUsageAsync(selectedConnection,
                                                                                new DateTimeOffset(startTime.ConvertToDateTime(true)),
                                                                                new DateTimeOffset(endTime.ConvertToDateTime(true)),
                                                                                DataUsageGranularity.PerMinute, toggleDiscount.IsOn);
                        for (int i = 1; i <= listNetworkUsage.Count; i++)
                        {
                            listNetworkUsage[i - 1].Date = string.Format("{0}-{1}", startTime.Minute, startTime.Minute + 1);
                            listNetworkUsage[i - 1].ConvertDataToMB();
                            startTime = startTime.AddMinutes(1);
                        }

                        UpdateChartItemSource(listNetworkUsage);
                        break;
                    case "Days":
                        ComboBoxMore.Visibility = Visibility.Visible;
                        if (ComboBoxMore.Items.Count == 0)
                        {
                            ComboBoxMore.Items.Add("7 days before");
                            ComboBoxMore.Items.Add("6 days before");
                            ComboBoxMore.Items.Add("5 days before");
                            ComboBoxMore.Items.Add("4 days before");
                            ComboBoxMore.Items.Add("3 days before");
                            ComboBoxMore.Items.Add("2 days before");
                            ComboBoxMore.Items.Add("Yesterday");
                            ComboBoxMore.SelectionChanged += ComboBoxMore_SelectionChanged;
                        }

                        comboBoxMoreSelected = ComboBoxMore.SelectedItem as string;
                        if (comboBoxMoreSelected == null)
                            return;

                        if (comboBoxMoreSelected.First() == 'Y')
                            customDateTimeNow = customDateTimeNow.AddDays(-1);
                        else
                            customDateTimeNow = customDateTimeNow.AddDays(((int)char.GetNumericValue(comboBoxMoreSelected.First())) * -1);
                            

                        startTime = new CustomDateTime(calendarType, customDateTimeNow.Year, customDateTimeNow.Month, customDateTimeNow.Day,
                                                       0, 0, 0);                        
                        endTime = startTime.AddDays(1);
                        listNetworkUsage = await Network.GetNetworkUsageAsync(selectedConnection,
                                                                                new DateTimeOffset(startTime.ConvertToDateTime(true)),
                                                                                new DateTimeOffset(endTime.ConvertToDateTime(true)),
                                                                                DataUsageGranularity.PerHour, toggleDiscount.IsOn);
                        for (int i = 1; i <= listNetworkUsage.Count; i++)
                        {
                            listNetworkUsage[i - 1].Date = string.Format("{0}-{1}", startTime.Hour, startTime.Hour + 1);
                            listNetworkUsage[i - 1].ConvertDataToMB();
                            startTime = startTime.AddHours(1);
                        }

                        UpdateChartItemSource(listNetworkUsage);
                        break;
                    case "Monthly":                        
                        ComboBoxMore.Visibility = Visibility.Visible;
                        if (ComboBoxMore.Items.Count == 0)
                        {
                            for (int i = 0; i < monthNames.Count(); i++)
                                ComboBoxMore.Items.Add(monthNames[i]);
                            ComboBoxMore.SelectionChanged += ComboBoxMore_SelectionChanged;
                        }

                        comboBoxMoreSelected = ComboBoxMore.SelectedItem as string;
                        if (comboBoxMoreSelected == null)
                            return;
                        var comboBoxMonthNumber = monthNames.IndexOf(comboBoxMoreSelected) + 1;

                        startTime = new CustomDateTime(calendarType, customDateTimeNow.Year, comboBoxMonthNumber, 1, 0, 0, 0);
                        endTime = new CustomDateTime(calendarType, customDateTimeNow.Year, comboBoxMonthNumber + 1, 1, 0, 0, 0);
                        listNetworkUsage = await Network.GetNetworkUsageAsync(selectedConnection,
                                                                                new DateTimeOffset(startTime.ConvertToDateTime(true)),
                                                                                new DateTimeOffset(endTime.ConvertToDateTime(true)),
                                                                                DataUsageGranularity.PerDay, toggleDiscount.IsOn);
                        for (int i = 1; i <= listNetworkUsage.Count; i++)
                        {
                            listNetworkUsage[i - 1].Date = calendarType == "Gregorian" ? string.Format("{0}/{1}", startTime.Month, startTime.Day) : string.Format("{0}/{1}", startTime.Day, startTime.Month);
                            listNetworkUsage[i - 1].ConvertDataToMB();
                            startTime = startTime.AddDays(1);
                        }

                        UpdateChartItemSource(listNetworkUsage);
                        break;
                    case "Data Plan":
                        ComboBoxMore.Visibility = Visibility.Collapsed;
                        startTime = dataPlanCustumStartTime;
                        endTime = Network.GetToDateTime(startTime);                                       
                        listNetworkUsage = await Network.GetNetworkUsageAsync(selectedConnection, 
                                                                                new DateTimeOffset(startTime.ConvertToDateTime(true)),
                                                                                new DateTimeOffset(endTime.ConvertToDateTime(true)),                                                                                
                                                                                DataUsageGranularity.PerDay, toggleDiscount.IsOn);
                        for (int i = 1; i <= listNetworkUsage.Count; i++)
                        {
                            listNetworkUsage[i - 1].Date = calendarType == "Gregorian" ? string.Format("{0}/{1}", startTime.Month, startTime.Day) : string.Format("{0}/{1}", startTime.Day, startTime.Month);
                            listNetworkUsage[i - 1].ConvertDataToMB();
                            startTime = startTime.AddDays(1);
                        }

                        UpdateChartItemSource(listNetworkUsage);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {                
                ex.Source = ex.Source.Contains(".../") ? ex.Source : ".../DataUsage.View.ChartPage.GetChartData";
                ExceptionHandling.ShowErrorMessageAsync(ex);
            }
        }

        private void UpdateChartItemSource(List<ConnectionReport> itemSource)
        {
            if (toggleDiscount.IsOn)
            {
                SeriesDownload.YBindingPath = "DownloadDiscount.Value";
                SeriesUpload.YBindingPath = "UploadDiscount.Value";
                SeriesDownload.TooltipTemplate = Resources["tooltipDiscountTemplate"] as DataTemplate;
                SeriesUpload.TooltipTemplate = Resources["tooltipDiscountTemplate"] as DataTemplate;
                SeriesDownload.ItemsSource = itemSource;
                SeriesUpload.ItemsSource = itemSource;
            }
            else
            {
                SeriesDownload.YBindingPath = "Download.Value";
                SeriesUpload.YBindingPath = "Upload.Value";
                SeriesDownload.TooltipTemplate = Resources["tooltipNormalTemplate"] as DataTemplate;;
                SeriesUpload.TooltipTemplate = Resources["tooltipNormalTemplate"] as DataTemplate;
                SeriesDownload.ItemsSource = itemSource;
                SeriesUpload.ItemsSource = itemSource;
            }
        }        
    }
}
