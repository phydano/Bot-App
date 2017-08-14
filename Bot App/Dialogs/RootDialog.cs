using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot_App.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // Split the currency the user wants to convert by comma
            string[] splitValue = activity.Text.Split(',');
            for (int i = 0; i < splitValue.Length; i++)
            {
                splitValue[i] = splitValue[i].Trim();
            }
            exchangerate ex = new exchangerate();

            // Called the Yahoo API to do the conversion
            string exchange = await ex.GetExchangeRate(splitValue[0], splitValue[1]);

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user
            // await context.PostAsync($"You have sent {activity.Text} which was {length} characters");
            await context.PostAsync($"Your Conversion {exchange}");
            context.Wait(MessageReceivedAsync);
        }
    }
}