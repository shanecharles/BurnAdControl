using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using BurnAdControls.ViewModels;

namespace BurnAdControls.Views
{
    public partial class InternalAdView : UserControl
    {
        private readonly DispatcherTimer _Timer;
        private readonly TimeSpan _TextRotationTime = TimeSpan.FromSeconds(5);
        private readonly Duration _Duration = new Duration(TimeSpan.FromSeconds(0.8));
        private int _RotationCount;

        private readonly Storyboard _RateIn, _RateOut;

        public InternalAdView()
        {
            InitializeComponent();
            _Timer = new DispatcherTimer {Interval = _TextRotationTime};
            _Timer.Tick += RotateTextTick;

            _RateIn = Resources["RateThisAppInAnimation"] as Storyboard;
            _RateOut = Resources["RateThisAppOutAnimation"] as Storyboard;
            _RotationCount = 0;
        }

        public void StartTimer()
        {
            _Timer.Start();
        }

        public void StopTimer()
        {
            _Timer.Stop();
        }

        public UserControl GetAdControl()
        {
            return this;
        }

        private void RotateTextTick(object sender, EventArgs e)
        {
            RotateAds();
        }

        private void RotateAds()
        {
            var vm = DataContext as BurnAdViewModel;

            if (vm != null && vm.DisplayRateThisApp)
            {
                _RotationCount += 1;

                if (_RotationCount > 3)
                {
                    _RateOut.Begin();
                    _RotationCount = 0;
                }
                else if (_RotationCount > 2)
                {
                    _RateIn.Begin();
                }
                else
                {
                    RotateText();
                }
            }
            else
            {
                RotateText();
            }
        }

        private void RotateText()
        {
            var fadeOut = TextStack1.Opacity > 0.5 ? TextStack1 : TextStack2;
            var fadeIn = TextStack1.Opacity < 0.5 ? TextStack1 : TextStack2;

            var storyboard = new Storyboard();
            storyboard.Children.Add(CreateAnimation(fadeIn, 0.0d, 1.0d));
            storyboard.Children.Add(CreateAnimation(fadeOut, 1.0d, 0.0d));
            storyboard.Begin();
        }

        private DoubleAnimation CreateAnimation(DependencyObject target, double from, double to)
        {
            var animation = new DoubleAnimation {Duration = _Duration, From = @from, To = to};
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            Storyboard.SetTarget(animation, target);
            return animation;
        }
    }

  
}
