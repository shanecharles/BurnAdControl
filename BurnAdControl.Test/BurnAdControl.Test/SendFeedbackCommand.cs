using System;
using System.Windows.Input;
using Microsoft.Phone.Tasks;

namespace BurnAdControlTest
{
    public class SendFeedbackCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var email = new EmailComposeTask();
            email.To = "feedback@company.email";
            email.Subject = "My App: Feedback";
            email.Show();
        }

        public event EventHandler CanExecuteChanged;
    }
}