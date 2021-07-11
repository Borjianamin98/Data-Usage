using CommonClass.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Email;
using Windows.Globalization;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;

namespace CommonClass
{
    public class Network
    {
        private static string calendarType = LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "CalendarType").ToString() == "Gregorian" ? CalendarIdentifiers.Gregorian : CalendarIdentifiers.Persian;

        // Returns the amount of time between each period of network usage for a given granularity
        private static TimeSpan GranularityToTimeSpan(DataUsageGranularity granularity)
        {
            switch (granularity)
            {
                case DataUsageGranularity.PerMinute:
                    return new TimeSpan(0, 1, 0);
                case DataUsageGranularity.PerHour:
                    return new TimeSpan(1, 0, 0);
                case DataUsageGranularity.PerDay:
                    return new TimeSpan(1, 0, 0, 0);
                default:
                    return new TimeSpan(0); // It's mean Total.
            }
        }

        public async static Task<List<ConnectionReport>> GetNetworkUsageAsync(string ConnectionName, DateTimeOffset StartTime, DateTimeOffset EndTime, DataUsageGranularity Granularity, bool GetDiscount)
        {            
            var granularityTimeSpan = GranularityToTimeSpan(Granularity);
            var startTimeCopy = StartTime;            
            try
            {
                var connectionDiscount = LocalSetting.GetSettingPageDiscounts().Where(connection => connection.ConnectionName == ConnectionName).ToList();
                var connectionProfile = GetConnectionProfile(ConnectionName);
                var networkUsages = await connectionProfile.GetNetworkUsageAsync(StartTime, EndTime, Granularity, new NetworkUsageStates());
                List<ConnectionReport> listNetworkUsage = new List<ConnectionReport>();

                var networkUsageStartTime = startTimeCopy;
                var networkUsageEndTime = granularityTimeSpan == new TimeSpan(0) ? EndTime : startTimeCopy.Add(granularityTimeSpan);
                var networkUsagePeriod = new Period(networkUsageStartTime, networkUsageEndTime);

                foreach (var networkUsage in networkUsages)
                {                    
                    var connectionReport = new ConnectionReport()
                    {
                        Download = new UsageWithUnit(networkUsage.BytesReceived, UsageUnit.Byte),
                        Upload = new UsageWithUnit(networkUsage.BytesSent, UsageUnit.Byte),
                        DownloadDiscount = new UsageWithUnit(networkUsage.BytesReceived, UsageUnit.Byte),
                        UploadDiscount = new UsageWithUnit(networkUsage.BytesSent, UsageUnit.Byte),
                    };
                    if (GetDiscount)
                    {
                        foreach (var discount in connectionDiscount)
                        {
                            var discountStartTime = new DateTimeOffset(networkUsageStartTime.DateTime.Date).Add(discount.StartTime);
                            var discountEndTime = new DateTimeOffset(networkUsageStartTime.DateTime.Date).Add(discount.EndTime);
                            var discountPeriod = new Period(discountStartTime, discountEndTime);

                            while (true) // loop until get all intersection of discount time and network usage period
                            {
                                var intersectionPeriod = Period.IntersectionTwoPeriod(discountPeriod, networkUsagePeriod);
                                if (intersectionPeriod != null)
                                {
                                    var intersectionPeroidUsage = await GetNetworkUsageAsync(connectionProfile, intersectionPeriod.Start, intersectionPeriod.End);
                                    connectionReport.DownloadDiscount = connectionReport.DownloadDiscount - ((discount.Discount * intersectionPeroidUsage.Item1) / 100);
                                    connectionReport.UploadDiscount = connectionReport.UploadDiscount - ((discount.Discount * intersectionPeroidUsage.Item2) / 100);
                                }

                                discountStartTime = discountStartTime.AddDays(1);
                                discountEndTime = discountEndTime.AddDays(1);
                                discountPeriod = new Period(discountStartTime, discountEndTime);
                                if (discountPeriod.Start >= networkUsagePeriod.End)
                                    break;
                            }

                            if (connectionReport.DownloadDiscount < new UsageWithUnit(0, UsageUnit.Byte))
                                connectionReport.DownloadDiscount = new UsageWithUnit(0, UsageUnit.Byte);
                            if (connectionReport.UploadDiscount < new UsageWithUnit(0, UsageUnit.Byte))
                                connectionReport.UploadDiscount = new UsageWithUnit(0, UsageUnit.Byte);
                        }
                    }

                    // Update Units
                    connectionReport.Download = UsageWithUnit.CheckUsageUnit(connectionReport.Download);
                    connectionReport.Upload = UsageWithUnit.CheckUsageUnit(connectionReport.Upload);
                    connectionReport.DownloadDiscount = UsageWithUnit.CheckUsageUnit(connectionReport.DownloadDiscount);
                    connectionReport.UploadDiscount = UsageWithUnit.CheckUsageUnit(connectionReport.UploadDiscount);

                    listNetworkUsage.Add(connectionReport);

                    networkUsageStartTime = networkUsageEndTime;
                    networkUsageEndTime = granularityTimeSpan == new TimeSpan(0) ? EndTime : networkUsageEndTime.Add(granularityTimeSpan);
                    networkUsagePeriod = new Period(networkUsageStartTime, networkUsageEndTime);
                }
                return listNetworkUsage;
            }
            catch (Exception ex)
            {               
                ex.Source = ex.Source.Contains(".../") ? ex.Source : string.Format(".../CommonClass.Network.GetNetworkUsageAsync(ConnectionName, {0}, {1}, {2}, {3}", StartTime, EndTime, Granularity.ToString(), GetDiscount);
                if (ex.Message.Contains("Object reference not set to an instance of an object.") ||
                    ex.Message.Contains("{Application Error} The exception") ||
                    ex.Message.Contains("Value does not fall within the expected range.") ||
                    ex.Message.Contains("Arg_ArgumentException"))
                    throw new Exception(ex.Message + "\nThis error maybe happen for getting network usage in home page or chart page. This is a Windows 10 known issue. Ignore it.") { Source = ex.Source };
                throw ex;
            }            
        }

        public async static Task<Tuple<UsageWithUnit,UsageWithUnit>> GetNetworkUsageAsync(ConnectionProfile ConnectionName, DateTimeOffset StartTime, DateTimeOffset EndTime)                                                      
        {
            try
            {
                if (DateTimeOffset.Compare(StartTime, EndTime) > 0)
                    throw new Exception("Start time must be shorter than end time!");
                else if (DateTimeOffset.Compare(StartTime, EndTime) == 0) // No Usage between two same time
                    return Tuple.Create(new UsageWithUnit(0, UsageUnit.Byte), new UsageWithUnit(0, UsageUnit.Byte)); //Item1 is Download and Item2 is Upload

                var networkUsages = (await ConnectionName.GetNetworkUsageAsync(StartTime, EndTime, DataUsageGranularity.Total, new NetworkUsageStates())).ToList();
                var bytesRecieved = new UsageWithUnit(networkUsages.First().BytesReceived, UsageUnit.Byte);
                var bytesSent = new UsageWithUnit(networkUsages.First().BytesSent, UsageUnit.Byte);
                return Tuple.Create(bytesRecieved, bytesSent); //Item1 is Download and Item2 is Upload
            }
            catch (Exception ex)
            {
                ex.Source = ex.Source.Contains(".../") ? ex.Source : string.Format(".../CommonClass.Network.GetNetworkUsageAsync(ConnectionName, {0}, {1})", StartTime, EndTime) ;
                throw ex;
            }
        }       

        public async static Task<ConnectionReport> GetAllNetworkUsageAsync(DateTimeOffset StartTime, DateTimeOffset EndTime, bool GetDiscount)
        {
            var listConnections = GetConnectionProfiles(GetActivateNetwork: true);
            var calculatedConnection = listConnections.Where(x => x.Calculate).Select(x => x.Name).ToList();
            var connectionProfiles = NetworkInformation.GetConnectionProfiles();
            ConnectionReport allConnectionReport = new ConnectionReport();
            foreach (var connection in connectionProfiles)
            {
                if (!calculatedConnection.Contains(connection.ProfileName))
                    continue;
                var connectioReport = (await GetNetworkUsageAsync(connection.ProfileName, StartTime, EndTime, DataUsageGranularity.Total, GetDiscount)).First();
                allConnectionReport.Download += connectioReport.Download;
                allConnectionReport.Upload += connectioReport.Upload;
                allConnectionReport.DownloadDiscount += connectioReport.DownloadDiscount;
                allConnectionReport.UploadDiscount += connectioReport.UploadDiscount;
            }
            return allConnectionReport;
        }

        private static ConnectionProfile GetConnectionProfile(string ConnectionName)
        {
            var connectionProfiles = NetworkInformation.GetConnectionProfiles();
            foreach (var connection in connectionProfiles)
            {
                if (connection.ProfileName == ConnectionName)
                    return connection;
            }
            throw new Exception("Connection not found!");
        }

        public static ObservableCollection<NetworkProfile> GetConnectionProfiles(bool GetActivateNetwork)
        {
            List<NetworkProfile> settingPageConnections = LocalSetting.GetSettingPageConnections();            
            var listNetworkProfiles = new ObservableCollection<NetworkProfile>();

            try
            {
                var connectionProfiles = NetworkInformation.GetConnectionProfiles();
                if (connectionProfiles.Count == 0)
                    return null; // We check for null when get information.

                foreach (var connectionProfile in connectionProfiles)
                {
                    var networkProfile = new NetworkProfile();

                    foreach (var settingPageConnection in settingPageConnections)
                        if (settingPageConnection.Name == connectionProfile.ProfileName)
                        {
                            networkProfile.Calculate = settingPageConnection.Calculate;
                            networkProfile.Active = settingPageConnection.Active;
                            networkProfile.Nickname = settingPageConnection.Nickname;
                            break;
                        }
                    if (GetActivateNetwork && networkProfile.Active == false)
                        continue;

                    networkProfile.Name = connectionProfile.ProfileName;
                    if (connectionProfile.IsWlanConnectionProfile) //Wifi
                        networkProfile.Type = ConnectionType.WiFi;
                    else if (connectionProfile.IsWwanConnectionProfile) // Cellular
                        networkProfile.Type = ConnectionType.Cellular;
                    else
                        networkProfile.Type = ConnectionType.Ethernet;

                    networkProfile.AuthenticationType = connectionProfile.NetworkSecuritySettings.NetworkAuthenticationType;
                    networkProfile.EncryptionType = connectionProfile.NetworkSecuritySettings.NetworkEncryptionType;
                    networkProfile.ConnectivityLevel = connectionProfile.GetNetworkConnectivityLevel();

                    listNetworkProfiles.Add(networkProfile);
                }
            }
            catch (Exception ex)
            {
                ex.Source = ".../CommonClass.Network.GetConnectionProfile(" + GetActivateNetwork + ")";
                throw;                                 
            }
            
            return listNetworkProfiles;
        }        

        public static string GetConnectionProfileNickname(string connectionProfileName)
        {
            List<NetworkProfile> settingPageConnections = LocalSetting.GetSettingPageConnections();

            foreach (var settingPageConnection in settingPageConnections)
            {
                if (settingPageConnection.Name == connectionProfileName)
                    return settingPageConnection.Nickname;
            }

            return connectionProfileName;
        }

        public async static Task ComposeEmail(string messageBody, string subject)
        {
            var emailMessage = new EmailMessage();
            emailMessage.Body = messageBody;
            emailMessage.Subject = subject;

            var emailRecipient = new EmailRecipient("borjianamin1998@outlook.com");
            emailMessage.To.Add(emailRecipient);

            await EmailManager.ShowComposeNewEmailAsync(emailMessage);            
        }
        
        /// <summary>
        /// Return end date time depend on date plan period and date time sented to method.
        /// </summary>
        /// <param name="dateTime">date time for calculate period for that</param>
        public static CustomDateTime GetToDateTime(CustomDateTime dateTime)
        {
            var period = LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "Period").ToString();
            var periodText = int.Parse(LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "PeriodText").ToString());            

            switch (period)
            {
                case "Daily":
                    return dateTime.AddDays(1);
                case "Monthly":
                    return dateTime.AddMonths(1);
                case "Custom":
                    return dateTime.AddDays(periodText);
                default:
                    throw new Exception("Invalid Period" + period) { Source = "DataUsage.HomePage.GetSettingAsync" };
            }
        }

        /// <summary>
        /// Reurn updated start time depend on period. Local Setting StartTime will be updated
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="period"></param>
        /// <param name="periodCustom">if period is Custom, then value of this used.</param>
        /// <returns></returns>
        public static CustomDateTime UpdateStartTimeDate(CustomDateTime StartTime, string Period, int PeriodCustom, bool UpdateLocalSetting = false)
        {
            var toCustomDateTime = GetToDateTime(StartTime);
            var customDateTimeNow = GlobalCalendar.ConvertDateTo(new CustomDateTime(CalendarIdentifiers.Gregorian, DateTime.Now), calendarType);
            while (customDateTimeNow.GetCompareTotalDays(toCustomDateTime) < 0)
            {
                switch (Period)
                {
                    case "Daily":
                        StartTime = StartTime.AddDays(1);
                        toCustomDateTime = toCustomDateTime.AddDays(1);
                        break;
                    case "Monthly":
                        StartTime = StartTime.AddMonths(1);
                        toCustomDateTime = toCustomDateTime.AddMonths(1);
                        break;
                    case "Custom":
                        StartTime = StartTime.AddDays(PeriodCustom);
                        toCustomDateTime = toCustomDateTime.AddDays(PeriodCustom);
                        break;
                    default:
                        throw new Exception("Invalid Period" + Period) { Source = "DataUsage.SettingPage.CheckValidDataPlan" };
                }
            }
            if (UpdateLocalSetting)
                LocalSetting.SetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "StartTime", (new DateTimeOffset(StartTime.ConvertToDateTime(true))).ToString("o"));
            return StartTime;
        }
    }
}
