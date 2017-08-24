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

        // This methods perform the text analysis and return the score
        public static async Task<string> TextAnalysis(string message)
        {
            client = new SentimentClient(ConfigurationManager.AppSettings["TextAnalyticsApiKey"]); // my API Key
            var request = new SentimentRequest();
            var text = new SentimentDocument()
            {
                Id = "1",
                Text = message,
                Language = "en"
            };
            request.Documents.Add(text);
            var response = ((await client.GetSentimentAsync(request)).Documents.FirstOrDefault()).Score; // retrieve the score
            return string.Format("{0:N2}", response); // return the result in string (2 Decimal Place)
        }
    }
}