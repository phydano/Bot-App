using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Linq;

namespace Bot_App
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                // Save state : Name of the user
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                userData.SetProperty<string>("username", activity.From.Name);
                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                await Conversation.SendAsync(activity, () => new Dialogs.LUISDialog());
            }
            
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
                // The below code is just to give out the welcome message to the user who first connected to the bot
                var connection = new ConnectorClient(new Uri(message.ServiceUrl), new MicrosoftAppCredentials());
                if (message.MembersAdded != null && message.MembersAdded.Any())
                {
                    foreach (var member in message.MembersAdded)
                    {
                        if (member.Id != message.Recipient.Id)
                            connection.Conversations.ReplyToActivityAsync(message.CreateReply("Welcome to Contoso Online Chat Bot!"));
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
                // If the bot is added to the conversation
                if (message.Action == "add")
                {
                    var reply = message.CreateReply("Welcome to Contoso Online Chat Bot!");
                    new ConnectorClient(new Uri(message.ServiceUrl)).Conversations.ReplyToActivityAsync(reply);
                }

            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        } 
    }
}