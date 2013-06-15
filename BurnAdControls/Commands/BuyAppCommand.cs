using System;
using System.Windows.Input;
using Microsoft.Phone.Tasks;

namespace BurnAdControls.Commands
{
    public class BuyAppCommand: ICommand
    {
        private readonly string _AppId;

        public BuyAppCommand(string appId)
        {
            _AppId = appId;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var marketplace = new MarketplaceDetailTask();
            marketplace.ContentIdentifier = _AppId;
            marketplace.Show();
        }

        public event EventHandler CanExecuteChanged;
    }
}
