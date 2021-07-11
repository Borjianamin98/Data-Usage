using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CommonClass
{
    public class ColorModel
    {

        public static Color SystemAccentColor => HexColorToRGB(((SolidColorBrush)Application.Current.Resources["SystemControlBackgroundAccentBrush"]).Color.ToString());
        

        public static Color AdjustBrightness(Color color, double factor)
        {
            if (factor <= 1)
                return Color.FromArgb(color.A, (byte)(color.R * factor), (byte)(color.G * factor), (byte)(color.B * factor));
            else
            {
                
                factor -= 1;
                var ReverseR = (255 - color.R) * factor;
                var ReverseG = (255 - color.G) * factor;
                var ReverseB = (255 - color.B) * factor;
                return Color.FromArgb(color.A, (byte)(color.R + ReverseR), (byte)(color.G + ReverseG), (byte)(color.B + ReverseB));
            }
        }

        public static Color HexColorToRGB(string color)
        {
            return Color.FromArgb(0xFF, Convert.ToByte(color.Substring(3, 2), 16), 
                                                    Convert.ToByte(color.Substring(5, 2), 16), 
                                                    Convert.ToByte(color.Substring(7, 2), 16));
        }
    }
}
