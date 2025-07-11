using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Hospital.Connectors
{
    public static class PackageHelpRequestConnector
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

        public static Task SaveAsync(PackageHelpRequest request)
        {
            return Context.SaveAsync<PackageHelpRequest>(request);
        }

        public static Task<PackageHelpRequest> LoadAsync(string saveID, string packageID)
        {
            return Context.LoadAsync<PackageHelpRequest>(saveID, packageID);
        }

        private static AsyncSearch<PackageHelpRequest> QueryAsync(string saveID)
        {
            return Context.QueryAsync<PackageHelpRequest>(saveID);
        }

        private static Task<List<PackageHelpRequest>> GetRemainingAsync(AsyncSearch<PackageHelpRequest> search)
        {
            return search.GetRemainingAsync();
        }

        public async static Task<List<PackageHelpRequest>> QueryAndGetRemainingAsync(string saveID)
        {
            var search = QueryAsync(saveID);
            return await GetRemainingAsync(search);
        }

        public static Task DeleteAsync(string saveID, string packageID)
        {
            return Context.DeleteAsync<PackageHelpRequest>(saveID, packageID);
        }
    }
}