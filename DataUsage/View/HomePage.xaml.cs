using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CommonClass;
using Windows.UI;
using System.Collections.ObjectModel;
using CommonClass.Model;
using Windows.Globalization;
using Windows.Networking.Connectivity;
using CommonClass.ViewModel;

namespace DataUsage.View
{    
    public sealed partial class HomePage : Page
    {
        MainPage rootPage = MainPage.Current;
        public MainViewModel ViewModel { get; set; } = MainPage.MainViewModel;
        public HomePage()
        {
            this.InitializeComponent();       
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                rootPage.MainProgressActive(true);
                await GetSettingAsync();
                rootPage.MainProgressActive(false);
            }
            catch (Exception ex)
            {
                ex.Source = ex.Source.Contains(".../") ? ex.Source : "Unhandled Exception at DataUsage.View.HomePage.OnNavigatedTo";
                ExceptionHandling.ShowErrorMessageAsync(ex);
            }
        }

        private async Task GetSettingAsync()
        {
            //
            // Get data from local setting
            //
            var calendarType = LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "CalendarType").ToString() == "Gregorian" ? CalendarIdentifiers.Gregorian : CalendarIdentifiers.Persian;
            var period = LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "Period").ToString();
            var periodText = int.Parse(LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "PeriodText").ToString());
            var dataEnabled = LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "DataPlanEnabled").ToString() == "True" ? true : false;            
            var fromDateTime = DateTimeOffset.Parse(LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "StartTime").ToString());

            var customDateTimeNow = GlobalCalendar.ConvertDateTo(new CustomDateTime(CalendarIdentifiers.Gregorian, DateTime.Now), calendarType);
            var fromCustomDateTime = Network.UpdateStartTimeDate(GlobalCalendar.ConvertDateTo(new CustomDateTime(CalendarIdentifiers.Gregorian, fromDateTime.Year, fromDateTime.Month, fromDateTime.Day), calendarType), period, periodText);
            var toCustomDateTime = Network.GetToDateTime(fromCustomDateTime);            
            var totalDaysBetweenFromAndToDateTime = fromCustomDateTime.GetCompareTotalDays(toCustomDateTime);

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
            if (dataEnabled)
            {
                ListReport.ItemTemplate = Resources["ReportWithDataPlanTemplate"] as DataTemplate;
                StackPanelRectDiscount.Visibility = Visibility.Visible;
            }                
            else
            {
                ListReport.ItemTemplate = Resources["ReportWithoutDataPlanTemplate"] as DataTemplate;
                StackPanelRectDiscount.Visibility = Visibility.Collapsed;
            }


            //
            // Add information of data plan to first column
            //
            lblBandwidthCap.Text = dataEnabled ? ViewModel.DataWithUnit.ToString() : "Infinity";
            FromDateUserControl.Day = fromCustomDateTime.Day;
            FromDateUserControl.Month = fromCustomDateTime.GetMonthName();
            FromDateUserControl.Year = fromCustomDateTime.Year;
            ToDateUserControl.Day = toCustomDateTime.Day;
            ToDateUserControl.Month = toCustomDateTime.GetMonthName();
            ToDateUserControl.Year = toCustomDateTime.Year;
            lblPieRemainingDays.Text = Math.Ceiling(customDateTimeNow.GetCompareTotalDays(toCustomDateTime)).ToString();

            //
            // Add pie chart data
            //
            fromDateTime = new DateTimeOffset(fromCustomDateTime.ConvertToDateTime(true));
            var toDateTime = new DateTimeOffset(toCustomDateTime.ConvertToDateTime(true));           
            var totalNetworkUsage = await Network.GetAllNetworkUsageAsync(fromDateTime, toDateTime.Add(new TimeSpan(1, 0, 0, 0)), dataEnabled);
            lblPieDailyAvg.Text = UsageWithUnit.CheckUsageUnit((totalNetworkUsage.UploadDiscount + totalNetworkUsage.DownloadDiscount) / totalDaysBetweenFromAndToDateTime).ToString();
            lblPieUpload.Text = UsageWithUnit.CheckUsageUnit(totalNetworkUsage.UploadDiscount).ToString();
            lblPieDownload.Text = UsageWithUnit.CheckUsageUnit(totalNetworkUsage.DownloadDiscount).ToString();
            UsageWithUnit remainingData = null;
            if (dataEnabled)
            {
                remainingData = (ViewModel.DataWithUnit - totalNetworkUsage.UploadDiscount - totalNetworkUsage.DownloadDiscount);
                if (remainingData.Value < 0)
                    remainingData.Value = 0;
                lblPieRemaining.Text = UsageWithUnit.CheckUsageUnit(remainingData).ToString();
            }
            else
                StackPanelPieChartRemaining.Visibility = Visibility.Collapsed;

            var pieChartList = new ObservableCollection<PieChartData>
            {
                new PieChartData {Name="Upload", Value = totalNetworkUsage.UploadDiscount.Value},
                new PieChartData {Name="Download", Value = totalNetworkUsage.DownloadDiscount.Value},
                new PieChartData {Name="Remaining", Value = dataEnabled ? remainingData.Value : 0},
            };
            PieChartColors.CustomBrushes = ViewModel.PieChartColors;
            if (pieChartList[0].Value == 0 && pieChartList[1].Value == 0 && pieChartList[2].Value == 0)
                GridNoPieChartData.Visibility = Visibility.Visible;
            else
            {
                PieUsage.ItemsSource = pieChartList;
                GridNoPieChartData.Visibility = Visibility.Collapsed;
            }
        }

        private async void ComboBoxConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedConnection = (ComboBoxConnections.SelectedItem as NetworkProfile)?.Name;
            if (selectedConnection != null)
                await GetReportData(selectedConnection);
        }

        private async void btnRefreshReport_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewerReport.ChangeView(0, 0, 1);
            var selectedConnection = (ComboBoxConnections.SelectedItem as NetworkProfile)?.Name;
            if (selectedConnection != null)
                await GetReportData(selectedConnection);
        }

        private async Task GetReportData(string selectedConnection)
        {
            var calendarType = LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "CalendarType").ToString() == "Gregorian" ? "GregorianCalendar" : "PersianCalendar";
            List<ConnectionReport> listNetworkUsages = new List<ConnectionReport>();

            GridReport.Visibility = Visibility.Collapsed;
            lblPlaceholderReport.Visibility = Visibility.Collapsed;
            ProgressReport.Visibility = Visibility.Visible;
            ProgressReport.IsActive = true;

            try
            {
                //Get first day of month according to calendar type.
                var convertedDate = GlobalCalendar.ConvertDateTo(new CustomDateTime("GregorianCalendar", DateTime.Now), calendarType);
                var firstDayOfMonth = convertedDate.GetFirstDayOfThisMonth();
                listNetworkUsages = await Network.GetNetworkUsageAsync(selectedConnection,
                                                                        new DateTimeOffset(firstDayOfMonth.ConvertToDateTime(true)), DateTimeOffset.Now,
                                                                        DataUsageGranularity.PerDay, true);
                ProgressReport.Visibility = Visibility.Collapsed;
                ProgressReport.IsActive = false;
                GridReport.Visibility = Visibility.Visible;

                int temp = 0;
                for (int i = 1; i <= listNetworkUsages.Count; i++)
                {
                    if (calendarType == "GregorianCalendar")
                        listNetworkUsages[i - 1].Date = string.Format("{0}/{1}", convertedDate.Month, i);
                    else
                        listNetworkUsages[i - 1].Date = string.Format("{0}/{1}", i, convertedDate.Month);
                    if (temp == 1)
                        listNetworkUsages[i - 1].TableColor = ViewModel.BoxColor;
                    else if (temp == 0)
                        listNetworkUsages[i - 1].TableColor = Colors.Transparent;
                    temp = temp == 0 ? 1 : 0; // For Change 0 to 1 and 1 to 0
                }
            }
            catch (Exception ex)
            {
                ex.Source = ex.Source.Contains(".../") ? ex.Source : "Unhandled Exception at DataUsage.View.HomePage.ComboBoxConnections_SelectionChanged";
                ExceptionHandling.ShowErrorMessageAsync(ex);
            }
            ListReport.ItemsSource = listNetworkUsages;
        }
    }
}
