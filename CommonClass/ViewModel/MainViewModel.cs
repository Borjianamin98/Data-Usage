using CommonClass.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CommonClass.ViewModel
{
    public class MainViewModel : NotifyProperty
    {
        private List<string> listPieChartColors = new List<string> { "#FF9812b0", "#FF0063b1", "#FF9760f3" };

        public string DefaultAppColor => LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "DefaultAppColor").ToString();

        public BitmapImage LogoImage => new BitmapImage(new Uri("ms-appx:///Assets/Logo/Logo-" + DefaultAppColor + ".png"));

        public List<Brush> PieChartColors 
        {
            get
            {
                List<Brush> listBrushs = new List<Brush>();
                for (int i = 0; i < listPieChartColors.Count; i++)
                    listBrushs.Add(new SolidColorBrush(ColorModel.HexColorToRGB(listPieChartColors[i])));
                return listBrushs;
            }
        }
        public SolidColorBrush BoxSolidBrushColor => new SolidColorBrush(BoxColor);

        public Color BoxColor => DefaultAppColor == "Light" ? ColorModel.HexColorToRGB("#FFF2F2F2") : ColorModel.HexColorToRGB("#FF2E2E2E");

        public SolidColorBrush AppColorBrush => DefaultAppColor == "Light" ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);

        public SolidColorBrush OpposedAppColorBrush => DefaultAppColor == "Light" ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);

        public SolidColorBrush LightAccentColor => new SolidColorBrush(ColorModel.AdjustBrightness(ColorModel.SystemAccentColor, 1.2));

        public double TopicFont => MainFont + 2;

        public double MainFont
        {
            get
            {
                switch (SelectedFontSize)
                {
                    case "Small":
                        return 13;                        
                    case "Medium":
                        return 15;
                    case "Big":
                        return 17;
                    default:
                        return 15;
                }
            }
        }

        public UsageWithUnit DataWithUnit
        {
            get
            {
                var data = double.Parse(LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "Data").ToString());
                var dataUnit = LocalSetting.GetLocalSetting(LocalSetting.SettingPageDataPlanContainer, "DataUnit").ToString() == "MB" ? UsageUnit.MB : UsageUnit.GB;
                return new UsageWithUnit(data, dataUnit);
            }
        }

        public string SelectedFontSize
        {
            get { return LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "FontSize").ToString(); }
            set
            {
                if (LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer, "FontSize").ToString() != value)
                {
                    LocalSetting.SetLocalSetting(LocalSetting.SettingPageGeneralContainer, "FontSize", value);                         
                    OnPropertyChanged("MainFont");
                }
            }
        }


        public double FontRatio(double fontSize, double ratio) => fontSize * ratio;

        public double AddValue(double value, double add) => value + add;
    }
}
