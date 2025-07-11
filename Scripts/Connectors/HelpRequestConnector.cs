using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Hospital.Connectors
{
    public static class HelpRequestConnector
    {
        private static CognitoAWSCredentials credentials;
        public static CognitoAWSCredentials Credentials
        {
            get
            {
                if (credentials == null)
                {
                    credentials = new CognitoAWSCredentials(CognitoEntry.GetIdentityPoolId(), RegionEndpoint.EUWest1);
                }
                return credentials;
            }
        }

        private static IAmazonDynamoDB client;
        public static IAmazonDynamoDB Client
        {
            get
            {
                if (client == null)
                    client = new AmazonDynamoDBClient(Credentials, RegionEndpoint.EUWest1);
                return client;
            }
        }

        private static DynamoDBContext context;
        private static DynamoDBContext Context
        {
            get
            {
                if (context == null)
                    context = new DynamoDBContext(Client, new DynamoDBContextConfig() { Conversion = DynamoDBEntryConversion.V2 });
                return context;
            }
        }

        public static Task SaveAsync(HelpRequest request)
        {
            return Context.SaveAsync<HelpRequest>(request);
        }

        public static Task<HelpRequest> LoadAsync(string saveID, string placeID)
        {
            return Context.LoadAsync<HelpRequest>(saveID, placeID);
        }

        private static AsyncSearch<HelpRequest> QueryAsync(string saveID)
        {
            return Context.QueryAsync<HelpRequest>(saveID);
        }

        private static Task<List<HelpRequest>> GetRemainingAsync(AsyncSearch<HelpRequest> search)
        {
            return search.GetRemainingAsync();
        }

        public async static Task<List<HelpRequest>> QueryAndGetRemainingAsync(string saveID)
        {
            var search = QueryAsync(saveID);
            return await GetRemainingAsync(search);
        }

        public static Task DeleteAsync(string saveID, string placeID)
        {
            return Context.DeleteAsync<HelpRequest>(saveID, placeID);
        }
    }
}