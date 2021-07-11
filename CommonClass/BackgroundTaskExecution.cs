using CommonClass;
using CommonClass.Model;
using CommonClass.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Globalization;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Notifications;

namespace CommonClass
{
    public class BackgroundTaskExecution
    {
        public static MainViewModel ViewModel { get; set; } = new MainViewModel();
        public const string TileBackgroudnTaskName = "BackgroudnTaskTile";
        public const string NotificationBackgroudnTaskName = "BackgroudnTaskToastNotification";
        private static string calendarType = LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "CalendarType").ToString() == "Gregorian" ? CalendarIdentifiers.Gregorian : CalendarIdentifiers.Persian;            
        private static bool dataEnabled = LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "DataPlanEnabled").ToString() == "True" ? true : false;        
        private static CustomDateTime customDateTimeNow = GlobalCalendar.ConvertDateTo(new CustomDateTime(CalendarIdentifiers.Gregorian, DateTime.Now), calendarType);

        public async static Task UpdateTile()
        {
            if (LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "Tile").ToString() == "False")
                return;

            var tileSelectedConection = LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "TileSelectedConection").ToString();
            var connectionProfile = NetworkInformation.GetConnectionProfiles().Where(conn => conn.ProfileName == tileSelectedConection).ToList();
            if (connectionProfile.Count == 0)
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                return;
            }

            var networkUsage = await GetDailyUsage(tileSelectedConection);
            if (networkUsage == null)
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                return;
            }            
            string connectionNickname = Network.GetConnectionProfileNickname(tileSelectedConection);
            string download = UsageWithUnit.CheckUsageUnit(networkUsage.DownloadDiscount).ToString();
            string upload = UsageWithUnit.CheckUsageUnit(networkUsage.UploadDiscount).ToString();
            string total = UsageWithUnit.CheckUsageUnit(networkUsage.DownloadDiscount + networkUsage.UploadDiscount).ToString();

            string content = $@"<tile>
                                      <visual>
                                        <binding template='TileMedium' hint-textStacking='center'>
                                           <text hint-style='caption'>Today Usage:</text>
                                           <text hint-style='caption'>{connectionNickname}</text>
                                           <text hint-style='caption'>U/L: {upload}</text>
                                           <text hint-style='caption'>D/L: {download}</text>                                       
                                        </binding>

                                        <binding template='TileWide'>
                                           <text hint-style='caption'>Today Usage: {connectionNickname}</text>
                                           <text hint-style='caption'>Upload: {upload}</text>
                                           <text hint-style='caption'>Download: {download}</text>
                                           <text hint-style='caption'>Total: {total}</text>
                                        </binding>
                                      </visual>
                                    </tile>";


            // Load the string into an XmlDocument
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);

            // Then create the tile notification
            var tile = new TileNotification(doc);

            // Send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tile);
        }

        public async static Task UpdateToastNotification()
        {
            if (LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "Notification").ToString() == "False")
                return;

            var dataLimitString = LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "NotificationDataLimit").ToString();
            var dailyUsageString = LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "NotificationDailyUsage").ToString();
            var notificationDataLimit = new UsageWithUnit(double.Parse(dataLimitString.Split(' ')[0]), dataLimitString.Split(' ')[1] == "MB" ? UsageUnit.MB : UsageUnit.GB);
            var notificationDailyUsage = new UsageWithUnit(double.Parse(dailyUsageString.Split(' ')[0]), dailyUsageString.Split(' ')[1] == "MB" ? UsageUnit.MB : UsageUnit.GB);

            var totalUsage = await GetUsageInDataPlan();
            if (totalUsage != null)
            {
                var remainingData = (ViewModel.DataWithUnit - totalUsage.UploadDiscount - totalUsage.DownloadDiscount);
                if (remainingData.Value < 0)
                    remainingData.Value = 0;

                if (remainingData < notificationDataLimit)
                {
                    string content = $@"<toast scenario='reminder'>
                                  <visual>
                                    <binding template='ToastGeneric'>
                                        <text>Exceed The Data Limit</text>
                                        <text>You have {notificationDataLimit.ToString()} data plan limit.</text>
                                        <text>Your remaining data is {UsageWithUnit.CheckUsageUnit(remainingData).ToString()}. Save your internet.</text>
                                    </binding>
                                  </visual>
                                </toast>";

                    // Load the string into an XmlDocument
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(content);

                    // And create the toast notification
                    var toast = new ToastNotification(doc);

                    toast.ExpirationTime = DateTime.Now.AddDays(1);

                    ToastNotificationManager.CreateToastNotifier().Show(toast);
                }
            }

            var connectionProfile = NetworkInformation.GetInternetConnectionProfile()?.ProfileName;
            if (connectionProfile == null)
                return;

            var networkUsage = await GetDailyUsage(connectionProfile);
            if (networkUsage == null)
                return;
            if (notificationDailyUsage < (networkUsage.DownloadDiscount + networkUsage.UploadDiscount))
            {
                string content = $@"<toast scenario='reminder'>
                                  <visual>
                                    <binding template='ToastGeneric'>
                                        <text>Exceed The Daily Usage Limit</text>
                                        <text>You have {notificationDailyUsage.ToString()} daily usage limit.</text>
                                        <text>Reduce you network usage because you go over your daily usage limit!</text>
                                    </binding>
                                  </visual>
                                </toast>";

                // Load the string into an XmlDocument
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);

                // And create the toast notification
                var toast = new ToastNotification(doc);

                toast.ExpirationTime = DateTime.Now.AddDays(1);

                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
        }

        private async static Task<ConnectionReport> GetUsageInDataPlan()
        {            
            var dataEnabled = LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "DataPlanEnabled").ToString() == "True" ? true : false;
            if (!dataEnabled)
                return null;           

            var fromDateTime = DateTimeOffset.Parse(LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "StartTime").ToString());
            var fromCustomDateTime = GlobalCalendar.ConvertDateTo(new CustomDateTime(CalendarIdentifiers.Gregorian, fromDateTime.Year, fromDateTime.Month, fromDateTime.Day), calendarType);
            var toCustomDateTime = Network.GetToDateTime(fromCustomDateTime);
            var toDateTime = new DateTimeOffset(toCustomDateTime.ConvertToDateTime(true));
            fromDateTime = new DateTimeOffset(fromCustomDateTime.ConvertToDateTime(true));

            try
            {
                return await Network.GetAllNetworkUsageAsync(fromDateTime, toDateTime.Add(new TimeSpan(1, 0, 0, 0)), true);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async static Task<ConnectionReport> GetDailyUsage(string connectionProfile)
        {
            TimeSpan dayTimeStartTime = TimeSpan.Parse(LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "DayTimeStartTime").ToString());
            TimeSpan dayTimeEndTime = TimeSpan.Parse(LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "DayTimeEndTime").ToString());
            var startTime = new CustomDateTime(customDateTimeNow.CalendarType, customDateTimeNow.Year, customDateTimeNow.Month, customDateTimeNow.Day, dayTimeStartTime);
            var endTime = new CustomDateTime(customDateTimeNow.CalendarType, customDateTimeNow.Year, customDateTimeNow.Month, customDateTimeNow.Day, dayTimeEndTime);
            try
            {
                var networkUsage = await Network.GetNetworkUsageAsync(connectionProfile,
                                                                 new DateTimeOffset(startTime.ConvertToDateTime(true)),
                                                                 new DateTimeOffset(endTime.ConvertToDateTime(true)),
                                                                 DataUsageGranularity.Total, dataEnabled);
                if (networkUsage == null)
                    return null;
                else
                    return networkUsage.First();
            }
            catch (Exception)
            {
                return null;
            }
        }

        //
        // taskEntryPoint: Task entry point for the background task.
        // taskName: A name for the background task.
        // trigger: The trigger for the background task.
        // condition: Optional parameter. A conditional event that must be true for the task to fire.
        //
        public static async Task RegisterBackgroundTask(string TaskName, uint TimeInterval)
        {
            //
            // Check for existing registrations of this background task.
            //
            foreach (var cur in BackgroundTaskRegistration.AllTasks.Values)
            {
                if (cur.Name == TaskName) // The task is already registered.
                    return;
            }

            //
            // Universal Windows apps must call RequestAccessAsync before registering any of the background trigger types.
            // To ensure that your Universal Windows app continues to run properly after you release an update,
            // you must call RemoveAccess and then call RequestAccessAsync when your app launches after being updated.  
            //    
            var requestAccess = await BackgroundExecutionManager.RequestAccessAsync();
            if (requestAccess == BackgroundAccessStatus.DeniedByUser ||
                requestAccess == BackgroundAccessStatus.DeniedBySystemPolicy)
                return;

            //
            // Register the background task.
            //
            var builder = new BackgroundTaskBuilder();

            builder.Name = TaskName;            
            builder.SetTrigger(new TimeTrigger(TimeInterval, false));
            //builder.SetTrigger(new SystemTrigger(SystemTriggerType.TimeZoneChange, false));
            //builder.AddCondition(condition);

            var task = builder.Register();
            
        }

        public static void UnRegisterBackgroundTask(string TaskName)
        {            
            foreach (var cur in BackgroundTaskRegistration.AllTasks.Values)
            {
                if (cur.Name == TaskName) // The task is already registered.
                    cur.Unregister(true);
            }

            if (TaskName == TileBackgroudnTaskName)
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
        }
    }
}
