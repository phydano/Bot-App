using System.Linq;
using Microsoft.ProjectOxford.Text.Sentiment;
using System.Configuration;
using System.Threading.Tasks;

namespace Bot_App
{
    /*
     * This class calls the Microsoft Cognitive Service Text Analytics API to get the sentiment score. 
     * This score is use to determine whether the user is happy with our service or not. 
     */
    public class TextAnalytics
    {
        private static SentimentClient client;
        private static int count = 1; // count to keep the ID

        // This methods perform the text analysis and return the score
        public static async Task<string> TextAnalysis(string message)
        {
            client = new SentimentClient(ConfigurationManager.AppSettings["TextAnalyticsApiKey"]); // my API Key
            var request = new SentimentRequest();
            count++; // increment 
            var text = new SentimentDocument()
            {
                Id = count.ToString(),
                Text = message,
                Language = "en"
            };
            request.Documents.Add(text);
            var response = ((await client.GetSentimentAsync(request)).Documents.FirstOrDefault()).Score; // retrieve the score
            return string.Format("{0:N2}", response); // return the result in string (2 Decimal Place)
        }

        // Returns to the comment back to the user based on the raing they give us. 
        public static async Task<string> Sentiment(string score)
        {
            if(double.Parse(score) <= 0.7)
                return "Sorry that our bot service does not meet your expectation. We will continue to improve on it.";
            else // If the score is above 0.7
                return "Thank you for your feedback. We really glad you like our bot service.";  
        }
    }
}