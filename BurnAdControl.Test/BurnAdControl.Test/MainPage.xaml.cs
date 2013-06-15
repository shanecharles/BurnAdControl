using System;
using System.Windows;
using BurnAdControls;
using BurnAdControls.Commands;
using BurnAdControls.ViewModels;
using Microsoft.Phone.Controls;

namespace BurnAdControlTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        private BurnAdControl _BurnAdControl;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_BurnAdControl == null)
            {
                var highlight = new BurnAdViewModel
                    {
                        Logo = new Uri("/ApplicationIcon.png", UriKind.RelativeOrAbsolute),
                        Text1Line1 = "New Other App",
                        Text1Line2 = "you should download it",
                        Text2Line1 = "Best App Ever",
                        Text2Line2 = "really, help me",
                        DefaultCommand = new BuyAppCommand("the app id of the app you are highlighting")
                    };

                _BurnAdControl = new BurnAdControl(this, AdPanel, highlight);
                _BurnAdControl.AdKeyWords = "my,keywords,for,the,ad,server";
                _BurnAdControl.AdServerRefreshSeconds = 40d;

                _BurnAdControl.AddInternalAd(new BurnAdViewModel
                    {
                        Text1Line1 = "Company Name",
                        Text1Line2 = "how are we doing?",
                        Text2Line1 = "how can we improve?",
                        Text2Line2 = "send us feedback",
                        DisplayRateThisApp = true,
                        DefaultCommand = new SendFeedbackCommand()
                    });
                _BurnAdControl.AddInternalAd(new BurnAdViewModel
                    {
                        Text1Line1 = "My Other App",
                        Text1Line2 = "small details",
                        Text2Line1 = "Cheeky Catchphrase",
                        Text2Line2 = "the punchline to hook them in",
                        DefaultCommand = new BuyAppCommand("My Other App Id")
                    });
            }
            base.OnNavigatedFrom(e);
        }

        private void NextPage(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Page2.xaml", UriKind.Relative));
        }
    }
}