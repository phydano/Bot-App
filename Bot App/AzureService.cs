using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace Bot_App
{
    /* This class defines the connection to the Azure Service webserver
         */
    public class AzureService
    {
        public static AzureService instance;
        public MobileServiceClient mobileClient;
        public IMobileServiceTable<contosouserinfo> customerdata;
        public List<contosouserinfo> customerList;

        public AzureService()
        {
            this.mobileClient = new MobileServiceClient("http://contosobanklimited.azurewebsites.net");
            this.customerdata = this.mobileClient.GetTable<contosouserinfo>();
        }

        public MobileServiceClient serviceClient { get { return mobileClient; } }

        public static AzureService serviceInstance
        {
            get
            {
                if (instance == null) instance = new AzureService();
                return instance;
            }
        }

        // This method POST the data to the Easytable
        public async Task Post(contosouserinfo data)
        {
            await this.customerdata.InsertAsync(data);
        }

        // This method Get the customer
        public async Task Get(string id)
        {
             await this.customerdata.LookupAsync(id);
        }
    }
}