using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
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
        public string fullName = ""; // fullname of the person

        // Currency Conversion Intent
        [LuisIntent("CurrencyConversion")]
        public async Task CurrencyConversion(IDialogContext context, LuisResult result)
        {
            string fromCur = ""; // store the currency the user wants to convert from
            string toCur = ""; // store the currency the user wants to convert to
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
            // contosouserinfo user = new contosouserinfo();
            // user.name = "Test";
            // await AzureService.serviceInstance.Post(user);
            // Example of updating
            // contosouserinfo user2 = new contosouserinfo();
            // user2.ID = "2d49d3b6-9785-4377-b510-49a67af72661";
            // user2.name = "Test2";
            // await AzureService.serviceInstance.Update(user2);
            // Example of getting
            // Boolean test = await AzureService.serviceInstance.Get("2d49d3b6-9785-4377-b510-49a67af72661");
            // await context.PostAsync($"Your Result is {test}");
            // Example of deleting 
            // contosouserinfo user2 = new contosouserinfo();
            // user2.ID = "2d49d3b6-9785-4377-b510-49a67af72661";
            // await AzureService.serviceInstance.Delete(user2);
            context.Wait(MessageReceived);
        }

        // If the intent is adding the customer to the bank
        [LuisIntent("Register")]
        public async Task Add(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Please tell me your full name");
            context.Wait(MessageReceived);
        }

        // If the intent is adding the customer to the bank
        [LuisIntent("FullName")]
        public async Task Name(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Your fullname is: {result.Dialog.ToString()}");
            await context.PostAsync($"Great, now please tell me your 4 digits secret code");
            context.Wait(MessageReceived);
        }

        // If the intent does not matched (None)
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Sorry I could not interpret what you are saying");
            context.Wait(MessageReceived);
        }
    }
}