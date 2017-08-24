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
        public string usercode = ""; // the four secret digits code of the user

        // Currency Conversion Intent
        [LuisIntent("CurrencyConversion")]
        public async Task CurrencyConversion(IDialogContext context, LuisResult result)
        {
            var message = context.MakeMessage();
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

            context.Wait(MessageReceived);
        }

        // If the intent is adding the customer to the bank
        [LuisIntent("Register")]
        public async Task Add(IDialogContext context, LuisResult result)
        {
            PromptDialog.Text(context, username, $"Pease tell me your full name");
        }

        // If the intent is updating the customer's balance
        [LuisIntent("AddMoney")]
        public async Task Update(IDialogContext context, LuisResult result)
        {
            double amount = 0;
            string account = "";
            EntityRecommendation thisAmount;
            EntityRecommendation thisAccount;

            if(!fullName.Equals("")) context.UserData.SetValue<string>("username", fullName); // we will remember this registerd user
            var username = context.UserData.GetValue<string>("username"); // get the name of the user 

            if (result.TryFindEntity("Amount", out thisAmount)) amount = Double.Parse(thisAmount.Entity);
            else await context.PostAsync($"Incorrect Amount");

            if (result.TryFindEntity("Account", out thisAccount)) account = thisAccount.Entity.ToUpper();
            else await context.PostAsync($"Incorrect Account");

            // await context.PostAsync($"Account is: {account}");

            if (await AzureService.serviceInstance.Update(account, amount))
            {
                await context.PostAsync($"{username} have deposited ${amount} to your account");
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync($"Incorrect Account Number");
                context.Wait(MessageReceived);
            }
        }

        // If the intent is adding the customer to the bank
        [LuisIntent("GetBalance")]
        public async Task Retrieve(IDialogContext context, LuisResult result)
        {
            string account = "";
            EntityRecommendation thisAccount;

            if (result.TryFindEntity("Account", out thisAccount)) account = thisAccount.Entity.ToUpper();
            else await context.PostAsync($"Incorrect Account");

            contosouserinfo user = await AzureService.serviceInstance.Get(account);
            if(user != null) // if the user exists 
            {
                await context.PostAsync($"Your balance is: ${user.balance}");
                context.Wait(MessageReceived);
            }
            else // if the user does not exists 
            {
                await context.PostAsync($"Incorrect Account");
                context.Wait(MessageReceived);
            }
        }

        // If the intent is adding the customer to the bank
        [LuisIntent("Delete")]
        public async Task RemoveUser(IDialogContext context, LuisResult result)
        {
            string account = "";
            EntityRecommendation thisAccount;

            if (result.TryFindEntity("Account", out thisAccount)) account = thisAccount.Entity.ToUpper();
            else await context.PostAsync($"Incorrect Account");

            if (await AzureService.serviceInstance.Delete(account)) // if the user exists 
            {
                await context.PostAsync($"Sorry to hear you go. We have removed you from our system");
                context.Wait(MessageReceived);
            }
            else // if the user does not exists 
            {
                await context.PostAsync($"Incorrect Account");
                context.Wait(MessageReceived);
            }
        }

        // Ask the user for their name and their four digit secret code (a PIN)
        private async Task username(IDialogContext context, IAwaitable<string> result)
        {
            string userinput = await result;
            if (userinput != null || userinput != "")
            {
                fullName = userinput;
                await context.PostAsync($"Your fullname is: {userinput}");
                PromptDialog.Text(context, secretcode, $"Please Enter the 4 digits PIN code: ");
            }
        }

        // Post the users to the Database 
        private async Task secretcode(IDialogContext context, IAwaitable<string> result)
        {
            var activity = context.MakeMessage();
            var userinput = await result;
            bool digitsOnly = true; // assuming that the 4 digits are actually digits
            // Check that thse user must only input digits
            if (userinput.Length != 4)
            { // this will be an infinite loop until the user enters a 4-digit PIN
                PromptDialog.Text(context, secretcode, $"Sorry the numbers you entered are not 4 digits, Please re-enter:");
                return;
            }
            else // now if the character length is 4, we need to check if it contains only Digits number 
            {
                foreach (char c in userinput)
                {
                    if (Char.IsDigit(c)) digitsOnly = true;
                    else digitsOnly = false;
                }
                if (!digitsOnly) // If it contains non-digits
                { // an infinite loop until the users enter a 4 digits PIN code. 
                    PromptDialog.Text(context, secretcode, $"Sorry the numbers you entered are not 4 digits, Please re-enter:");
                    return;
                }
            }
            // Now we going to push the new user to the database 
            usercode = userinput; // remember the usercode (PIN)
            var userid = ""; // to store the userid
            await context.PostAsync($"Your code is: {userinput}"); // confirm this code back to the user
            try
            {
                await AzureService.serviceInstance.Post(randomIDGenerator(), fullName, userinput, 0);
                userid = AzureService.serviceInstance.getCurrentUser().ID;
                context.Wait(MessageReceived);
            }
            catch (Exception e)
            {
                Console.Write(e.ToString()); // output the error to the console 
            }
            finally
            {
                // If the current UserID is empty or null, we know there is a problem so we report the error 
                if (userid.Equals("") || userid == null)
                    await context.PostAsync($"Sorry there was a problem during the registration"); // output the message if there is something wrong
                else
                {
                    await context.PostAsync($"Great your are now registered with us");
                    await context.PostAsync($"Your ID is: {userid}");
                }
            }
        }

        // If the intent is Help, give the users a redirect URL to the website 
        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            var messageReply = context.MakeMessage(); // this is the reply
            messageReply.Attachments = new List<Attachment>(); 
            List<CardImage> image = new List<CardImage>(); // the card image 
            image.Add(new CardImage("https://support.content.office.net/en-us/media/8b3b440b-e110-4127-9fea-691ebd8bc33e.png"));

            // The card action which is a redirect to the specified URL
            CardAction cardAction = new CardAction()
            {
                Title = "Contoso Bank Support",
                Type = "openUrl",
                Value = "https://www.microsoft.com/en-nz"
            };

            // This is our thumbnail displaying the text
            ThumbnailCard thumnail = new ThumbnailCard()
            {
                Title = "Help?",
                Subtitle = "For more help, please visit our support site",
                Images = image,
                Buttons = new List<CardAction>()
            };
            thumnail.Buttons.Add(cardAction);
            messageReply.Attachments.Add(thumnail.ToAttachment());

            await context.PostAsync(messageReply);
            context.Wait(MessageReceived);
        }

        // If the intent does not matched (None)
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Sorry I could not interpret what you are saying. For any help, please type 'Help'");
            context.Wait(MessageReceived);
        }

        /* A simple randomly geneated number for the user ID
         * The first 2 digits are characters randomly generated from Cap A-Z
         * The following 5 digits are randomly generated numbers from 10000 - 99999
         */
        public string randomIDGenerator()
        {
            Random random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // A string of characters 
            int firstChar = random.Next(0, chars.Length - 1); // randomly select a character
            int secondChar = random.Next(0, chars.Length - 1); 
            int randomNumber = random.Next(10000, 99999); // randomly select a number
            return (chars[firstChar].ToString()+ chars[secondChar].ToString() + randomNumber.ToString());
        }
    }
}