using CommonClass;
using CommonClass.Model;
using CommonClass.ViewModel;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace DataUsage.Controls
{
    public sealed partial class DiscountUserControl : UserControl
    {
        public event EventHandler EditClicked;
        public event EventHandler DeleteClicked;
        public MainViewModel ViewModel { get; set; } = new MainViewModel();


        private bool _buttonVisibility;
        private bool ButtonVisibility
        {
            get { return _buttonVisibility; }
            set
            {
                _buttonVisibility = value;
                StackPanelButtons.Opacity = 1;

                if (_buttonVisibility)
                {
                    HideButtonAnimation.Stop();
                    ShowButtonAnimation.Begin();
                }                    
                else
                {
                    ShowButtonAnimation.Stop();
                    HideButtonAnimation.Begin();
                }
            }
        }

        private DiscountControl _data;
        public DiscountControl Data
        {
            get { return _data; }
            set
            {
                _data = value;
                lblConnectionName.Text = Network.GetConnectionProfileNickname(_data.ConnectionName);
                lblDiscount.Text = string.Format("{0} %", _data.Discount.ToString());
                lblStartTime.Text = _data.StartTime.ToString();
                lblEndTime.Text = _data.EndTime.ToString();
            }
        }

        public DiscountUserControl()
        {
            this.InitializeComponent();            
        }
        
        private void GridAll_Tapped(object sender, TappedRoutedEventArgs e)
        {                   
            if (ButtonVisibility == true)
            {
                DoubleAnimationShowButtons.To = StackPanelButtons.ActualHeight;
                DoubleAnimationHideButtons.From = StackPanelButtons.ActualHeight;
            }

            ButtonVisibility = ButtonVisibility == true ? false : true;            
        }

        private void AppBarEdit_Click(object sender, RoutedEventArgs e)
        {
            if (StackPanelButtons.Opacity == 1) // Check availability of buttons
                OnEditCliked();
        }

        private void OnEditCliked() => EditClicked?.Invoke(this, EventArgs.Empty);        

        private void AppBarDelete_Click(object sender, RoutedEventArgs e)
        {
            if (StackPanelButtons.Opacity == 1) // Check availability of buttons
                OnDeleteClicked();
        }
        private void OnDeleteClicked() => DeleteClicked?.Invoke(this, EventArgs.Empty);
    }
}
