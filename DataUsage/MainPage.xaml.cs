using CommonClass;
using DataUsage.View;
using CommonClass.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DataUsage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        public static MainViewModel MainViewModel { get; private set; } = new MainViewModel();
        public MainViewModel ViewModel { get; set; } = MainViewModel;
        public MainPage()
        {
            this.InitializeComponent();
            Current = this;            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Initialization            
            if (LocalSetting.GetLocalSetting(LocalSetting.SettingPageGeneralContainer,"FirstTime").ToString() == "True") //First Time
            {
                MainSplitView.Visibility = Visibility.Collapsed;
                GridIntroduction.Visibility = Visibility.Visible;
                IntroductionFrame.Navigate(typeof(IntroductionPage));
            }
            else
            {
                Start();
            }
        }

        public void Start()
        {
            MainSplitView.Visibility = Visibility.Visible;
            GridIntroduction.Visibility = Visibility.Collapsed;
            SplitViewHome.IsChecked = true;
        }

        private void SplitViewbtnMenu_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = MainSplitView.IsPaneOpen == true ? false : true;
        }

        public void MainProgressActive(bool active)
        {           
            ContentFrame.Visibility = active == true ? Visibility.Collapsed : Visibility.Visible;
            MainProgress.Visibility = active == true ? Visibility.Visible : Visibility.Collapsed;
            MainProgress.IsActive = active;
        }

        private void SplitViewButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButtonClicked = sender as RadioButton;
            if (radioButtonClicked == null)
                return;

            MainProgressActive(false);
            MainSplitView.IsPaneOpen = false;

            switch (radioButtonClicked.Tag)
            {
                case "Home":
                    ContentFrame.Navigate(typeof(HomePage));
                    break;
                case "Chart":
                    ContentFrame.Navigate(typeof(ChartPage));
                    break;
                case "About":
                    ContentFrame.Navigate(typeof(AboutPage));
                    break;
                case "Setting":
                    ContentFrame.Navigate(typeof(SettingPage));
                    break;
                default:
                    break;
            }
        }

        private async void SplitViewFeedback_Click(object sender, RoutedEventArgs e)
        {
            await Network.ComposeEmail("", "Data Usage - Feedback");
        }

        private async void SplitViewRateUs_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?PFN={Package.Current.Id.FamilyName}"));
        }
    }
}
