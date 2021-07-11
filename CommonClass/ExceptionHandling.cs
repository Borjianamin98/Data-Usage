using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Popups;

namespace CommonClass
{
    public class ExceptionHandling
    {
        private static string exFormatMessage;
        public static async void ShowErrorMessageAsync(Exception ex)
        {
            var messageDialog = new MessageDialog(ex.Message, "Error");

            messageDialog.Commands.Add(new UICommand("Report", new UICommandInvokedHandler(CommandInvokedHandler)));
            messageDialog.Commands.Add(new UICommand("Restart Setting", new UICommandInvokedHandler(CommandInvokedHandler)));
            exFormatMessage = $"Exception Message: {ex.Message} \n\n" +
                              $"Exception Source: {ex.Source} \n\n" +
                              $"Exception StackTrace: {ex.StackTrace}";

            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        private async static void CommandInvokedHandler(IUICommand command)
        {
            if (command.Label == "Report")
                await Network.ComposeEmail(exFormatMessage, "Data Usage - Report");
            else if (command.Label == "Restart Setting")
                LocalSetting.RestartLocalSetting();

            CoreApplication.Exit();
        }
    }
}

