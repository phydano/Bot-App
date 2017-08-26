using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Data.SqlClient;

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
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(); // authentication to the SQL database

        public AzureService()
        {
            this.mobileClient = new MobileServiceClient("https://contosobanklimited.azurewebsites.net"); // default none-auth URL
            //this.mobileClient = new MobileServiceClient("https://contosobanklimited.azurewebsites.net/.auth/login/aad/callback");
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

        // This is a method to authenticate myself as an admin to the SQL database
        public void Authentication()
        {
            this.builder.DataSource = "contosobankltd.database.windows.net";
            this.builder.UserID = "phydano";
            this.builder.Password = "BleachBraveSOul01";
            this.builder.InitialCatalog = "BankUsers";
            SqlConnection connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
        }

        // This method POST the a new user to the Easytable SQL database
        public async Task Post(string id, string name, string secretcode, double balance)
        {
            user = new contosouserinfo();
            user.ID = id;
            user.name = name;
            user.secretcode = secretcode;
            user.balance = balance;
            await this.customerdata.InsertAsync(user);
        }

        // Get the current user 
        public contosouserinfo getCurrentUser()
        {
            return user;
        }

        // This method UPDATE the balance of the user to the Easytable
        // Assuming the given id, name, or secretcode matched
        public async Task<Boolean> Update(string id, double balance)
        {
            List<contosouserinfo> users = await this.customerdata.ToListAsync();
            foreach (contosouserinfo user in users)
            {
                if (user.ID.Equals(id))
                {
                    user.balance += balance;
                    await this.customerdata.UpdateAsync(user);
                    return true;
                }
            }
            return false; // no user is matched  
        }

        // This method DELETE the data in the Easytable
        // We only need the ID to delete the customer from the database 
        public async Task<Boolean> Delete(string id)
        {
            List<contosouserinfo> users = await this.customerdata.ToListAsync();
            foreach (contosouserinfo user in users)
            {
                if (user.ID.Equals(id))
                {
                    await this.customerdata.DeleteAsync(user);
                    return true;
                }
            }
            return false; // no user is matched   
        }

        // This method matched the name of the user to find out if that user exists in the database
        public async Task<contosouserinfo> Get(string id)
        {   
            List<contosouserinfo> users = await this.customerdata.ToListAsync();
            foreach(contosouserinfo user in users)
            {
                if (user.ID.Equals(id)) return user; // return the user that is matched 
            }
            return null; // nothing is matched 
        }
    }
}