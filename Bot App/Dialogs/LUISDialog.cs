using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot_App.Dialogs
{
    // My Luis App ID and key
    [LuisModel("f3a1812c-928d-49d9-ab90-1a2f5f5d51e7", "7b7d6bb5711449699b2e07de9cf204e3")]
    [Serializable]
    public class LUISDialog : LuisDialog<object>
    {
        private string fullName = ""; // fullname of the person
        private string usercode = ""; // the four secret digits code of the user
        private bool welcome; // whether the use has been previosuly with us or not 
        private string accountNum; // remember the user acc number 
        private string howToUse = "To use the Bot: " +
            "\n * For Deposit: Type 'Deposit' to your account number (E.g. Deposit $10 to RX12345)." +
                    $"\n * For Balance: Type 'Balance' on your account number (E.g. Balance on RX12345)." +
                    $"\n * For Register: Type 'Register' (E.g. Register me to the bank)." +
                    $"\n * For De-register: Type 'Remove' your account number (E.g. Remove RX12345)." +
            $"\n * For Currency Conversion: Type 'CurrencyX' to 'CurrencyY' (E.g. NZD to USD)";


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
            else
            {
                await context.PostAsync($"The query is not correct. Please try again.");
                return;
            }

            // Find out the Entity is there
            if (result.TryFindEntity("ToCurrency", out toEnt)) toCur = toEnt.Entity;
            else
            {
                await context.PostAsync($"The query is not correct. Please try again.");
                return;
            }

            // Calculate using Yahoo API and give the result back to the user
            exchangerate ex = new exchangerate();
            await context.PostAsync($"Your Conversion base on a $1 value is: {await ex.GetExchangeRate(fromCur, toCur)}");
            PromptDialog.Confirm(context, Continuation, $"Is there anything else I can help you with?");
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

            if (result.TryFindEntity("Amount", out thisAmount)) amount = Double.Parse(thisAmount.Entity);
            else
            {
                await context.PostAsync($"The query is not correct. Please try again.");
                return;
            }

            if (result.TryFindEntity("Account", out thisAccount)) account = thisAccount.Entity.ToUpper();
            else
            {
                await context.PostAsync($"The query is not correct. Please try again.");
                return;
            }

            if (await AzureService.serviceInstance.Update(account, amount))
            {
                accountNum = account;
                await context.PostAsync($"{fullName} have deposited ${amount} to your account");
                PromptDialog.Confirm(context, MoreDeposit, $"Do you want to do any more deposit?");
            }
            else
            {
                await context.PostAsync($"The account number is invalid. Please try again");
                context.Wait(MessageReceived);
            }
            
        }

        // Confirm whether the user wants to do any more deposit 
        private async Task MoreDeposit(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                PromptDialog.Text(context, DepositMoreMoney, $"Ok, please tell me how much more you want to deposit?");
            }
            else PromptDialog.Confirm(context, Continuation, $"Is there anything else I can help you with?");
        }

        // Deposit the money to the user account without repeatingly asking for the account number 
        private async Task DepositMoreMoney(IDialogContext context, IAwaitable<string> result)
        {
            var depositAmount = await result;
            double amount; // the amount in double
            // First of all check whether the amount to deposit has any '$' sign in it or not
            if (depositAmount.Contains("$")) {
                string[] part = depositAmount.Split('$'); // strip the '$' sign away
                depositAmount = string.Join("", part); // join back the string together
            }
            // If the input is not a $ value
            if (Double.TryParse(depositAmount, out amount))
            {
                if (await AzureService.serviceInstance.Update(accountNum, amount))
                {
                    await context.PostAsync($"{fullName} have deposited ${amount} more to your account");
                    PromptDialog.Confirm(context, MoreDeposit, $"Do you want to do any more deposit?");
                }
            }
            else // now the value is definitely unreadable - hence not a number 
            {
                PromptDialog.Text(context, DepositMoreMoney, $"The balance input is incorrect, please try again:");
                return;
            }
        }

        // If the intent is adding the customer to the bank
        [LuisIntent("GetBalance")]
        public async Task Retrieve(IDialogContext context, LuisResult result)
        {
            string account = "";
            EntityRecommendation thisAccount;

            if (result.TryFindEntity("Account", out thisAccount)) account = thisAccount.Entity.ToUpper();
            else
            {
                await context.PostAsync($"The query is not correct. Please try again.");
                return;
            }

            contosouserinfo user = await AzureService.serviceInstance.Get(account);
            // If the user exists
            if (user != null)
            {
                await context.PostAsync($"Your total balance is: ${user.balance}");
                PromptDialog.Confirm(context, Continuation, $"Is there anything else I can help you with?");
            }
            // Otherwise
            else
            {
                await context.PostAsync($"The account number is invalid. Please try again");
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
            else // If it cannot find the entity
            {
                await context.PostAsync($"The query is not correct. Please try again.");
                context.Wait(MessageReceived);
                return;
            }

            if (await AzureService.serviceInstance.Delete(account)) // if the user exists 
                PromptDialog.Text(context, feedback, $"Sorry to hear you go. We have removed you from our system. \r\n Please tell me how I performed. Your feedback is really appreciated:");
            else // If the user does not exists 
            {
                await context.PostAsync($"The account number is invalid. Please try again");
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
                context.UserData.SetValue("username", fullName); // we will remember this registerd user
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
            try
            {
                await AzureService.serviceInstance.Post(randomIDGenerator(), fullName, userinput, 0);
                accountNum = AzureService.serviceInstance.getCurrentUser().ID;
            }
            catch (Exception e)
            {
                Console.Write(e.ToString()); // output the error to the console 
            }
            finally
            {
                // If the current UserID is empty or null, we know there is a problem so we report the error 
                if (accountNum.Equals("") || accountNum == null) await context.PostAsync($"Sorry there was a problem during the registration"); // output the message if there is something wrong
                else await context.PostAsync($"Great your are now registered with us. Your ID is: {accountNum}");
                welcome = true; // remember that the user is registered
                PromptDialog.Confirm(context, Continuation, $"Is there anything else I can help you with?");
            }
        }

        // Do the tasks depend on whether the user wants to continue or not
        private async Task Continuation(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                await context.PostAsync($"Here is some tips to help you use the chat bot. {howToUse}");
                context.Wait(MessageReceived);
            }
            else PromptDialog.Text(context, feedback, $"Thank you for using the Bot service. Please tell me how I did:");  
        }

        // This method is to get the feedback from the user and generated the Score base on the Text Analytics API
        private async Task feedback(IDialogContext context, IAwaitable<string> result)
        {
            var score = await TextAnalytics.TextAnalysis(await result);
            await context.PostAsync($"You rate us as {double.Parse(score)*100}%. \r\n {await TextAnalytics.Sentiment(score)}");
            context.Wait(MessageReceived);
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

        // If the intent is anywords relate to greetings. We should display options to users
        [LuisIntent("Greetings")]
        public async Task Greetings(IDialogContext context, LuisResult result)
        {
            // If the user is already with us
            if (!context.UserData.TryGetValue("username", out fullName))
            {
                await context.PostAsync($"Hello there. With the bot service, you register/de-register, deposit, check balance with us.");
                PromptDialog.Confirm(context, OptionsSelection, $"Would you like to register?");
                return;
            }
            if(!welcome){
                welcome = true;
                await context.PostAsync($"Welcome Back {fullName}!");
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync($"Reminder: {howToUse}");
                context.Wait(MessageReceived);
            }
        }

        // Depends on the user selection, this method can direct user to the registration process
        private async Task OptionsSelection(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result) PromptDialog.Text(context, username, $"Pease tell me your full name"); // add the user by going through the registration process
            else // otherwise give users some help options on how to use the bot 
            {
                await context.PostAsync($"{howToUse}");
                context.Wait(MessageReceived);
            }
        }

        // If the intent does not matched (None), reply back some help to the users 
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"{howToUse} {Environment.NewLine}\r\n For further help, please type 'Help'.");
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