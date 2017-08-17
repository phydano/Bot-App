using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_App
{
    public class contosouserinfo
    {
        // The id coloumn, where we get or set the id
        [JsonProperty(PropertyName = "Id")]
        public string ID { get; set; }

        // The name coloumn, where we get or set the name of the customer
        [JsonProperty(PropertyName = "Name")]
        public string name { get; set; }

        // The code coloumn, where we get or set the customer secret code to verify their identity
        [JsonProperty(PropertyName = "Code")]
        public string secretcode { get; set; }

        // The balance coloumn, where we get or set the customer's balance
        [JsonProperty(PropertyName = "Balance")]
        public double balance { get; set; }
    }
}