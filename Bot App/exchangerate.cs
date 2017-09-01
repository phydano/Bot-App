using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Bot_App
{
    /* The Exchange Rate class where the Yahoo API is used */
    public class exchangerate
    {
        public exchangerate(){} // the constructor

        // Convert the rate using yahoo API
        public String conversionRate(string fromCur, string toCur)
        {
            try
            {
                WebClient web = new WebClient();
                const string yahooAPI = "http://finance.yahoo.com/d/quotes.csv?s={0}{1}=X&f=l1";
                string formatUrl = String.Format(yahooAPI, fromCur, toCur);

                // Get the response back as a $1 value exchange rate
                return new WebClient().DownloadString(formatUrl);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString()); // print out the error to console if there is any
                return "unknown"; // return unknown to the user 
            }
        }
    }
}