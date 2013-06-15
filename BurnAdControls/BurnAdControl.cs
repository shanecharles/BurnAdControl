using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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

        private const double AppHighlightSeconds = 10d;
        private const double AdServerWaitSeconds = 30d;
        public const double MinAdServerRefreshSeconds = 30d;

        private BurnAdViewModel _LastViewModel;

        private InternalAdView _InternalAdView;
        private DispatcherTimer _ErrorTimer;
        private DispatcherTimer _AdServerRefreshTimer;
        private double _AdServerRefreshSeconds = 45d;
        
        public bool HideOnFailure { get; set; }

        private readonly IList<BurnAdViewModel> _InternalAds = new List<BurnAdViewModel>();

        public BurnAdControl(PhoneApplicationPage page, Grid content)
        {
            _ApplicationId = "test_client";
           _AdUnitId = "TextAd";
            _Page = page;
            _PageGrid = content;
            Setup();
        }

        public BurnAdControl(PhoneApplicationPage page, Grid content, BurnAdViewModel appHighlight)
        {
            _ApplicationId = "test_client";
           _AdUnitId = "TextAd";
            _Page = page;
            _PageGrid = content;

            Setup(appHighlight);
        }
        
        public BurnAdControl(PhoneApplicationPage page, Grid content, string adApplicaiontId, string adUnitId)
        {
            _ApplicationId = adApplicaiontId;
            _AdUnitId = adUnitId;
            _Page = page;
            _PageGrid = content;

            Setup();
        }

        public BurnAdControl(PhoneApplicationPage page, Grid content, string adApplicaiontId, string adUnitId, BurnAdViewModel appHighlight)
        {
            _ApplicationId = adApplicaiontId;
            _AdUnitId = adUnitId;
            _Page = page;
            _PageGrid = content;

            Setup(appHighlight);
        }

        private void Setup()
        {
            HideOnFailure = false;
            _InternalAdView = new InternalAdView();
            SetupTimers(AdServerWaitSeconds);
            _Page.Unloaded += PageUnloaded;

             SetupGrid();
            AddAdControl();
        }

        private void Setup(BurnAdViewModel appHighlight)
        {
            HideOnFailure = false;
            _InternalAdView = new InternalAdView();
            SetupTimers(AppHighlightSeconds);
            _Page.Unloaded += PageUnloaded;

            SetupGrid();
            _InternalAds.Add(appHighlight);

            RunInternalAds();
            _ErrorTimer.Start();
        }

        private void SetupTimers(double errorTimerSeconds)
        {
            _ErrorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(errorTimerSeconds) };
            _ErrorTimer.Tick += EnableAdControl;

            _AdServerRefreshTimer = new DispatcherTimer{Interval = TimeSpan.FromSeconds(_AdServerRefreshSeconds)};
            _AdServerRefreshTimer.Tick += AdServerRefreshTimerTick;

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
    }
}
