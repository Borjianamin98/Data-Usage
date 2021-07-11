using CommonClass.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CommonClass
{
    public class LocalSetting
    {        
        static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private static readonly string LocalSettingName = "LocalSettingV10";
        public static readonly string SettingPageGeneralContainer = "SettingPageGeneralContainer";
        public static readonly string SettingPageDataPlanContainer = "SettingPageDataPlanContainer";
        public static readonly string SettingPageDiscountContainer = "SettingPageDiscountContainer";
        public static readonly string settingPageConnectionContainer = "SettingPageConnectionContainer";        

        public static void CheckLocalSetting()
        {
            if (!localSettings.Values.ContainsKey(LocalSettingName)) // If this is first use of app ...
            {
                localSettings.Values[LocalSettingName] = "true";

                // Create Containers
                localSettings.DeleteContainer(SettingPageGeneralContainer);
                localSettings.DeleteContainer(settingPageConnectionContainer);
                localSettings.DeleteContainer(SettingPageDiscountContainer);
                localSettings.DeleteContainer(SettingPageDataPlanContainer);
                localSettings.CreateContainer(SettingPageGeneralContainer, ApplicationDataCreateDisposition.Always);
                localSettings.CreateContainer(settingPageConnectionContainer, ApplicationDataCreateDisposition.Always);
                localSettings.CreateContainer(SettingPageDiscountContainer, ApplicationDataCreateDisposition.Always);
                localSettings.CreateContainer(SettingPageDataPlanContainer, ApplicationDataCreateDisposition.Always);

                //Set Local Values 
                localSettings.Containers[SettingPageGeneralContainer].Values["FirstTime"] = "True";
                localSettings.Containers[SettingPageGeneralContainer].Values["CalendarType"] = "Gregorian";
                localSettings.Containers[SettingPageGeneralContainer].Values["FontSize"] = "Medium";
                localSettings.Containers[SettingPageGeneralContainer].Values["DefaultAppColor"] = "Light";
                localSettings.Containers[SettingPageGeneralContainer].Values["Tile"] = "False";
                localSettings.Containers[SettingPageGeneralContainer].Values["TileSelectedConection"] = "";
                localSettings.Containers[SettingPageGeneralContainer].Values["Notification"] = "False";
                localSettings.Containers[SettingPageGeneralContainer].Values["NotificationDataLimit"] = "100 MB";
                localSettings.Containers[SettingPageGeneralContainer].Values["NotificationDailyUsage"] = "100 MB";
                localSettings.Containers[SettingPageGeneralContainer].Values["DayTimeStartTime"] = new TimeSpan(0,0,0).ToString();
                localSettings.Containers[SettingPageGeneralContainer].Values["DayTimeEndTime"] = new TimeSpan(23,59,59).ToString();
                localSettings.Containers[SettingPageDataPlanContainer].Values["DataPlanEnabled"] = "False";
                localSettings.Containers[SettingPageDataPlanContainer].Values["Data"] = "6";
                localSettings.Containers[SettingPageDataPlanContainer].Values["DataUnit"] = "GB";
                localSettings.Containers[SettingPageDataPlanContainer].Values["StartTime"] = DateTimeOffset.Now.ToString("o");
                localSettings.Containers[SettingPageDataPlanContainer].Values["Period"] = "Daily";
                localSettings.Containers[SettingPageDataPlanContainer].Values["PeriodText"] = "30";
            }
        }

        public static void RestartLocalSetting() => localSettings.Values.Remove(LocalSettingName);
        public static object GetLocalSetting(string Container, string Name)
        {
            CheckLocalSetting();
            if (!localSettings.Containers.ContainsKey(Container))
                throw new Exception(string.Format("Container {0} not found in local setting.", Container)) { Source = ".../CommonClass.LocalSetting.GetSettingPageConnections()" };
            if (localSettings.Containers[Container].Values[Name] == null)
                throw new Exception(string.Format("Can't find {0} of {1} in local setting.", Name, Container)) { Source = ".../CommonClass.LocalSetting.GetSettingPageConnections()" };
            return localSettings.Containers[Container].Values[Name];
        }

        public static void SetLocalSetting(string Container, string Name, object value)
        {
            CheckLocalSetting();
            if (!localSettings.Containers.ContainsKey(Container))
                throw new Exception(string.Format("Container {0} not found in local setting.", Container)) { Source = ".../CommonClass.LocalSetting.GetSettingPageConnections()" };
            localSettings.Containers[Container].Values[Name] = value;
        }

        public static void SetSettingPageConnections(List<NetworkProfile> connectionList)
        {
            CheckLocalSetting();
            if (!localSettings.Containers.ContainsKey(settingPageConnectionContainer))
                throw new Exception(string.Format("Container {0} not found in local setting.", settingPageConnectionContainer)) { Source = ".../CommonClass.LocalSetting.GetSettingPageConnections()" };

            localSettings.DeleteContainer(settingPageConnectionContainer); // Make empty our list.
            localSettings.CreateContainer(settingPageConnectionContainer, ApplicationDataCreateDisposition.Always);

            int counter = 0;
            foreach (var connection in connectionList)
            {
                var composite = new ApplicationDataCompositeValue();
                composite["Name"] = connection.Name;
                composite["Nickname"] = connection.Nickname;
                composite["Active"] = connection.Active;
                composite["Calculate"] = connection.Calculate;
                
                SetLocalSetting(settingPageConnectionContainer, counter.ToString(), composite);                
                counter += 1;
            }
        }

        public static List<NetworkProfile> GetSettingPageConnections()
        {
            CheckLocalSetting();
            if (!localSettings.Containers.ContainsKey(settingPageConnectionContainer))
                throw new Exception(string.Format("Container {0} not found in local setting.", settingPageConnectionContainer)) { Source = ".../CommonClass.LocalSetting.GetSettingPageConnections()" };

            var connectionList = new List<NetworkProfile>();
            foreach (var item in localSettings.Containers[settingPageConnectionContainer].Values.Values)
            {
                var composite = (ApplicationDataCompositeValue)item;
                connectionList.Add(new NetworkProfile
                {
                    Name = composite["Name"].ToString(),
                    Nickname = composite["Nickname"].ToString(),
                    Active = (bool)composite["Active"],
                    Calculate = (bool)composite["Calculate"]
                });                
            }
            return connectionList;
        }
        public static void SetSettingPageDiscounts(List<DiscountControl> discountList)
        {
            CheckLocalSetting();
            if (!localSettings.Containers.ContainsKey(SettingPageDiscountContainer))
                throw new Exception(string.Format("Container {0} not found in local setting.", SettingPageDiscountContainer)) { Source = ".../CommonClass.LocalSetting.GetSettingPageConnections()" };

            localSettings.DeleteContainer(SettingPageDiscountContainer); // Make empty our list.
            localSettings.CreateContainer(SettingPageDiscountContainer, ApplicationDataCreateDisposition.Always);

            for (int i = 0; i < discountList.Count; i++)
            {
                var composite = new ApplicationDataCompositeValue();
                composite["ID"] = discountList[i].ID.ToString();
                composite["ConnectionName"] = discountList[i].ConnectionName;
                composite["Discount"] = discountList[i].Discount.ToString();
                composite["StartTime"] = discountList[i].StartTime.ToString();
                composite["EndTime"] = discountList[i].EndTime.ToString();
                SetLocalSetting(SettingPageDiscountContainer, i.ToString(), composite);                
            }
        }

        public static List<DiscountControl> GetSettingPageDiscounts()
        {
            CheckLocalSetting();
            if (!localSettings.Containers.ContainsKey(SettingPageDiscountContainer))
                throw new Exception(string.Format("Container {0} not found in local setting.", SettingPageDiscountContainer)) { Source = ".../CommonClass.LocalSetting.GetSettingPageConnections()" };

            var connectionList = new List<DiscountControl>();
            foreach (var item in localSettings.Containers[SettingPageDiscountContainer].Values.Values)
            {
                var composite = (ApplicationDataCompositeValue)item;
                connectionList.Add(new DiscountControl
                {
                    ID = int.Parse(composite["ID"].ToString()),
                    ConnectionName = composite["ConnectionName"].ToString(),
                    Discount = double.Parse(composite["Discount"].ToString()),
                    StartTime = TimeSpan.Parse(composite["StartTime"].ToString()),
                    EndTime = TimeSpan.Parse(composite["EndTime"].ToString())
                });
            }
            return connectionList;
        }
    }
}
