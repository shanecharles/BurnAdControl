using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BurnAdControls.Exceptions;
using BurnAdControls.ViewModels;
using BurnAdControls.Views;
using Microsoft.Advertising;
using Microsoft.Advertising.Mobile.UI;
using Microsoft.Phone.Controls;

namespace BurnAdControls
{
    public class BurnAdControl
    {
        private AdControl _AdControl;
        private readonly string _ApplicationId;
        private readonly string _AdUnitId;
        private readonly PhoneApplicationPage _Page;
        private readonly Grid _PageGrid;
        private Grid _PlaceHolder;
        private string _AdKeyWords;
        private double _Longitude;
        private double _Latitude;

        private const double AppHighlightSeconds = 10d;
        private const double AdServerWaitSeconds = 30d;
        public const double MinAdServerRefreshSeconds = 30d;

        private BurnAdViewModel _LastViewModel;
        private readonly BurnAdStartupParams _BurnAdStartupParams;

        private InternalAdView _InternalAdView;
        private DispatcherTimer _ErrorTimer;
        private DispatcherTimer _AdServerRefreshTimer;
        private DispatcherTimer _DelayStartTimer;
        private double _AdServerRefreshSeconds = 40d;
        
        public bool HideOnFailure { get; set; }

        private readonly IList<BurnAdViewModel> _InternalAds = new List<BurnAdViewModel>();

        public BurnAdControl(PhoneApplicationPage page, Grid content)
        {
            _ApplicationId = "test_client";
           _AdUnitId = "TextAd";
            _Page = page;
            _PageGrid = content;
            _BurnAdStartupParams = null;
            Setup();
        }

        public BurnAdControl(PhoneApplicationPage page, Grid content, BurnAdStartupParams startupParams)
        {
            _ApplicationId = "test_client";
           _AdUnitId = "TextAd";
            _Page = page;
            _PageGrid = content;
            _BurnAdStartupParams = startupParams;
            Setup(startupParams);
        }
        
        public BurnAdControl(PhoneApplicationPage page, Grid content, string adApplicaiontId, string adUnitId)
        {
            _ApplicationId = adApplicaiontId;
            _AdUnitId = adUnitId;
            _Page = page;
            _PageGrid = content;

            Setup();
        }

        public BurnAdControl(PhoneApplicationPage page, Grid content, string adApplicaiontId, string adUnitId, BurnAdStartupParams startupParams)
        {
            _ApplicationId = adApplicaiontId;
            _AdUnitId = adUnitId;
            _Page = page;
            _PageGrid = content;

            Setup(startupParams);
        }

        private void Setup()
        {
            CommonSetup();

            StartAdControl();
        }

        private void Setup(BurnAdStartupParams startupParams)
        {
            CommonSetup();

            if (startupParams.DelayStartSeconds <= 0.0d)
            {
                if (startupParams.AppHighlight != null)
                {
                    _ErrorTimer.Interval = TimeSpan.FromSeconds(AppHighlightSeconds);
                    StartAppHighlight(startupParams.AppHighlight);
                }
                else
                {
                    StartAdControl();
                }
            }
            else
            {
                _DelayStartTimer = new DispatcherTimer{Interval = TimeSpan.FromSeconds(startupParams.DelayStartSeconds)};
                _DelayStartTimer.Tick += DelayStartTick;
                _DelayStartTimer.Start();
            }
        }

        private void DelayStartTick(object sender, EventArgs e)
        {
            _DelayStartTimer.Stop();
            if(_BurnAdStartupParams.AppHighlight==null)
                StartAdControl();
            else
                StartAppHighlight(_BurnAdStartupParams.AppHighlight);
        }

        private void CommonSetup()
        {
            HideOnFailure = false;
            SetupGrid();
            SetupErrorTimer();
            _InternalAdView = new InternalAdView();
            _Page.Unloaded += PageUnloaded;
            _AdServerRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(_AdServerRefreshSeconds) };
            _AdServerRefreshTimer.Tick += AdServerRefreshTimerTick;
        }

        private void StartAdControl()
        {
            AddAdControl();
        }

        private void StartAppHighlight(BurnAdViewModel appHighlight)
        {
            _InternalAds.Add(appHighlight);
            RunInternalAds();
            _ErrorTimer.Start();
        }

        public double Latitude
        {
            get { return _Latitude; }
            set
            {
                _Latitude = value;
                SetAdLocation();
            }
        }

        public double Longitude
        {
            get { return _Longitude; }
            set
            {
                _Longitude = value;
                SetAdLocation();
            }
        }

        private void SetAdLocation()
        {
            if (_AdControl != null)
            {
                _AdControl.Latitude = _Latitude;
                _AdControl.Longitude = _Longitude;
            }
        }

        private void SetupErrorTimer()
        {
            _ErrorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(AdServerRefreshSeconds) };
            _ErrorTimer.Tick += EnableAdControl;
        }

        private void AdServerRefreshTimerTick(object sender, EventArgs e)
        {
            RefreshAdControl();
        }

        private void RefreshAdControl()
        {
            if (_AdControl != null)
            {
                _AdControl.Refresh();
            }
            else
            {
                _AdServerRefreshTimer.Stop();
            }
        }

        public double AdServerRefreshSeconds
        {
            get { return _AdServerRefreshSeconds; }
            set
            {
                if (MinAdServerRefreshSeconds > value)
                {
                    _AdServerRefreshSeconds = MinAdServerRefreshSeconds;
                }
                else
                {
                    _AdServerRefreshSeconds = value;
                }

                _AdServerRefreshTimer.Interval = TimeSpan.FromSeconds(_AdServerRefreshSeconds);
            }
        }

        public void AddInternalAd(BurnAdViewModel burnAd)
        {
            _InternalAds.Add(burnAd);
        }

        public void RemoveInternalAd(BurnAdViewModel burnAd)
        {
            _InternalAds.Remove(burnAd);
        }

        public string AdKeyWords
        {
            get { return _AdKeyWords; }
            set
            {
                _AdKeyWords = value;
                SetAdKeyWords();
            }
        }

        private void SetAdKeyWords()
        {
            if (_AdControl != null && !string.IsNullOrEmpty(_AdKeyWords))
            {
                _AdControl.Keywords = _AdKeyWords;
            }
        }

        private void SetupGrid()
        {
            if (_PageGrid.RowDefinitions.Count == 0)
            {
                _PageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });    
            }
            _PageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            _ErrorTimer.Stop();
            RemoveAdControl();
            _Page.Unloaded -= PageUnloaded;
            _Page.Loaded += PageLoaded;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            _Page.Unloaded += PageUnloaded;
            _Page.Loaded -= PageLoaded;
            EnableAdControl();
        }

        private void EnableAdControl(object sender, EventArgs e)
        {
            EnableAdControl();
        }

        private void EnableAdControl()
        {
            _ErrorTimer.Stop();
            _ErrorTimer.Interval = TimeSpan.FromSeconds(AdServerWaitSeconds);
            RemoveAdControl();
            AddAdControl();
        }

        private void RemoveAdControl()
        {
            if (_AdControl != null)
            {
                _PageGrid.Children.Remove(_AdControl);
                _AdControl.ErrorOccurred -= AdControlErrorOccurred;
                _AdControl = null;
            }
        }

        private void AddAdControl()
        {
            _AdControl = new AdControl(_ApplicationId, _AdUnitId, false) { Height = 80, Width = 480, IsAutoCollapseEnabled = true };

            SetAdLocation();

            _AdControl.ErrorOccurred += AdControlErrorOccurred;
            SetAdKeyWords();
            _AdServerRefreshTimer.Start();

            Grid.SetRow(_AdControl, _PageGrid.RowDefinitions.Count-1);
            _PageGrid.Children.Add(_AdControl);

            if (_PlaceHolder != null)
            {
                _PageGrid.Children.Remove(_PlaceHolder);
                _PlaceHolder.Children.Clear();
                _PlaceHolder = null;
                StopAdTimer();
            }
        }

        private void AdControlErrorOccurred(object sender, AdErrorEventArgs e)
        {
            _AdServerRefreshTimer.Stop();

            CheckRequiredCapabilities(e);

            if (!_ErrorTimer.IsEnabled)
            {
                if (!HideOnFailure)
                {
                    RunInternalAds();
                }
                _ErrorTimer.Start();
            }
        }

        private void RunInternalAds()
        {
            DisplayPlaceHolder();

            var nextAd = GetNextAdToDisplay();

            if (nextAd != null)
            {
                _InternalAdView.DataContext = nextAd;
                _PlaceHolder.Children.Add(_InternalAdView);
            }

            StartAdTimer();
        }

        private BurnAdViewModel GetNextAdToDisplay()
        {
            var nextAd = _InternalAds.FirstOrDefault();
            if (nextAd != null)
            {
                _InternalAds.Remove(nextAd);
                _InternalAds.Add(nextAd);
                if (_LastViewModel == nextAd)
                {
                    nextAd = _InternalAds.FirstOrDefault();
                }
                _LastViewModel = nextAd;
            }
            return nextAd;
        }

        private void DisplayPlaceHolder()
        {
            if (_PlaceHolder == null)
            {
                _PlaceHolder = new Grid {Height = 80, Width = 480};
                Grid.SetRow(_PlaceHolder, _PageGrid.RowDefinitions.Count - 1);
                _PageGrid.Children.Add(_PlaceHolder);
            }
        }

        private void StartAdTimer()
        {
            _InternalAdView.StartTimer();
        }

        private void StopAdTimer()
        {
            if(_InternalAdView!=null)
                _InternalAdView.StopTimer();
        }

        [Conditional("DEBUG")]
        private static void CheckRequiredCapabilities(AdErrorEventArgs adErrorEventArgs)
        {
            if (adErrorEventArgs.ErrorCode == ErrorCode.ClientConfiguration)
            {
                throw new MissingRequirementException(adErrorEventArgs.Error.Message);
            }
        }
    }
}
