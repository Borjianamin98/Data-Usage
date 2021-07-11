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
    public sealed partial class DateUserControl : UserControl
    {
        public MainViewModel ViewModel { get; set; } = MainPage.MainViewModel;

        public DateUserControl()
        {
            this.InitializeComponent();
        }

        private double _daySize;

        public double DaySize
        {
            get { return _daySize; }
            set
            {
                _daySize = value;
                ViewBoxDay.Height = _daySize;
            }
        }

        private int _day;
        public int Day
        {
            get { return _day; }
            set
            {
                _day = value;
                lblDay.Text = _day.ToString();
            }
        }

        private string _month;
        public string Month
        {
            get { return _month; }
            set
            {
                _month = value;
                lblMoreInfo.Text = string.Format("{0} {1}", _month, Year.ToString());
            }
        }

        private int _year;
        public int Year
        {
            get { return _year; }
            set
            {
                _year = value;
                lblMoreInfo.Text = string.Format("{0} {1}", Month, _year.ToString());
            }
        }
    }
}
