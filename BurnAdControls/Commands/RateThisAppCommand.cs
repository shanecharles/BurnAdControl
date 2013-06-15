using System;
using System.Windows.Input;
using Microsoft.Phone.Tasks;

namespace BurnAdControls.Commands
{
    public class RateThisAppCommand: ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var reviewer = new MarketplaceReviewTask();
            reviewer.Show();
        }

        public event EventHandler CanExecuteChanged;
    }
}
