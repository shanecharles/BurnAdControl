using System;
using System.ComponentModel;
using System.Windows.Input;
using BurnAdControls.Commands;

namespace BurnAdControls.ViewModels
{
    public class BurnAdViewModel: INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _Text1Line1;
        private string _Text1Line2;
        private string _Text2Line1;
        private string _Text2Line2;
        private ICommand _DefaultCommand;
        private Uri _Logo;
        private bool _DisplayRateThisApp;

        public bool DisplayRateThisApp
        {
            get { return _DisplayRateThisApp; }
            set
            {
                _DisplayRateThisApp = value;
                RaisePropertyChanged("DisplayRateThisApp");
            }
        }

        public string Text1Line1
        {
            get { return _Text1Line1; }
            set
            {
                _Text1Line1 = value;
                RaisePropertyChanged("Text1Line1");
            }
        }

        public string Text2Line2
        {
            get { return _Text2Line2; }
            set
            {
                _Text2Line2 = value;
                RaisePropertyChanged("Text2Line2");
            }
        } 

        public string Text2Line1
        {
            get { return _Text2Line1; }
            set
            {
                _Text2Line1 = value;
                RaisePropertyChanged("Text2Line1");
            }
        } 

        public string Text1Line2
        {
            get { return _Text1Line2; }
            set
            {
                _Text1Line2 = value;
                RaisePropertyChanged("Text1Line2");
            }
        }

        public ICommand DefaultCommand
        {
            get { return _DefaultCommand; }
            set
            {
                _DefaultCommand = value;
                RaisePropertyChanged("DefaultCommand");
            }
        }

        public ICommand RateThisAppCommand
        {
            get { return new RateThisAppCommand();}
        }

        public Uri Logo
        {
            get { return _Logo; }
            set
            {
                _Logo = value;
                RaisePropertyChanged("Logo");
            }
        }


        protected void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
