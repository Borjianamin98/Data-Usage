using CommonClass;
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
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DataUsage.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IntroductionPage : Page
    {
        MainPage rootPage = MainPage.Current;
        public MainViewModel ViewModel { get; set; } = MainPage.MainViewModel;
        private int CurrentIntroductionState = 1;

        public IntroductionPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            StoryboardStart.Begin();
            StoryboardStart.Completed += (objectSender, arg) => btnNext.Click += btnNext_Click;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            btnNext.Click -= btnNext_Click;
            switch (CurrentIntroductionState)
            {
                case 1:
                    CurrentIntroductionState = 2;
                    StoryboardInfo1.Begin();
                    StoryboardInfo1.Completed += (objectSender, arg) => btnNext.Click += btnNext_Click;
                    break;
                case 2:
                    CurrentIntroductionState = 3;
                    StoryboardInfo2.Begin();
                    StoryboardInfo2.Completed += (objectSender, arg) => btnNext.Click += btnNext_Click;
                    break;
                case 3:
                    CurrentIntroductionState = 4;
                    StoryboardInfo3.Begin();
                    StoryboardInfo3.Completed += (objectSender, arg) => btnNext.Click += btnNext_Click;                    
                    btnNext.Content = "Start";
                    break;
                case 4:
                    LocalSetting.SetLocalSetting(LocalSetting.SettingPageGeneralContainer, "FirstTime", "False");
                    rootPage.Start();
                    break;
                default:
                    break;
            }
        }
    }
}
