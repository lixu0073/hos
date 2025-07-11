using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Hospital.Connectors
{
    public static class FriendCodesConnector
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

        private static AsyncSearch<FriendCodesModel> QueryAsync(string code)
        {
            return Context.QueryAsync<FriendCodesModel>(code);
        }

        private static Task<List<FriendCodesModel>> GetRemainingAsync(AsyncSearch<FriendCodesModel> search)
        {
            return search.GetRemainingAsync();
        }

        public async static Task<List<FriendCodesModel>> QueryAndGetRemainingAsync(string code)
        {
            var search = QueryAsync(code);
            if (search == null)
                return null;
            return await GetRemainingAsync(search);
        }
    }
}