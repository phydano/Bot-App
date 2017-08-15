using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bot_App.Dialogs
{
    // My Luis App ID and key
    [LuisModel("f3a1812c-928d-49d9-ab90-1a2f5f5d51e7", "7b7d6bb5711449699b2e07de9cf204e3")]
    [Serializable]
    public class LUISDialog : LuisDialog<object>
    {
        // Currency Conversion Intent
        [LuisIntent("CurrencyConversion")]
        public async Task currencyConversion(IDialogContext context, LuisResult result)
        {
            string fromCur=""; // store the currency the user wants to convert from
            string toCur=""; // store the currency the user wants to convert to
            EntityRecommendation fromEnt; // Entity LUIS determines
            EntityRecommendation toEnt;

            // Find out the Entity is there
            if (result.TryFindEntity("FromCurrency", out fromEnt)) fromCur = fromEnt.Entity;
            else await context.PostAsync($"No such currency");

            // Find out the Entity is there
            if (result.TryFindEntity("ToCurrency", out toEnt)) toCur = toEnt.Entity;
            else await context.PostAsync($"No such currency");

            // Calculate using Yahoo API and give the result back to the user
            exchangerate ex = new exchangerate();
            await context.PostAsync($"Your Conversion {await ex.GetExchangeRate(fromCur, toCur)}");
            
            // TODO: out of place...post the info to the database
            contosouserinfo user = new contosouserinfo();
            user.name = "Test";
            await AzureService.serviceInstance.Post(user);
            //
            context.Wait(MessageReceived);
        }

        // If the intent does not matched
        [LuisIntent("")]
        public async Task notMatched(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Sorry I could not interpret what you are saying");
            context.Wait(MessageReceived);
        }
    }
}