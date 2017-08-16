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
        public IMobileServiceTable<contosouserinfo> customerdata; // this is our database 
        private contosouserinfo user = new contosouserinfo(); // user for which we manipulate for CRUD operations

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

        // This method POST the a new user to the Easytable SQL database
        public async Task Post(string id, string name, string secretcode)
        {
            user.ID = id;
            user.name = name;
            user.secretcode = secretcode;
            await this.customerdata.InsertAsync(user);
        }

        // This method UPDATE the data to the Easytable
        // Assuming the given id, name, or secretcode matched
        public async Task Update(string id, string name, string secretcode)
        {
            user.ID = id;
            user.name = name;
            user.secretcode = secretcode;
            await this.customerdata.UpdateAsync(user);
        }

        // This method DELETE the data in the Easytable
        // We only need the ID to delete the customer from the database 
        public async Task Delete(string id)
        {
            user.ID = id;
            await this.customerdata.DeleteAsync(user);
        }

        // This method matched the name of the user to find out if that user exists in the database
        public async Task<contosouserinfo> Get(string name)
        {   
            List<contosouserinfo> users = await this.customerdata.ToListAsync();
            foreach(contosouserinfo user in users)
            {
                if (user.name.Equals(name)) return user; // return the user that is matched 
                else return null; // no user is matched
            }
            return null; // nothing is matched 
        }
    }
}