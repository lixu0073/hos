using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Hospital.Connectors
{
    public static class GiftConnector
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

        public static Task SaveAsync(GiftModel gift)
        {
            return Context.SaveAsync<GiftModel>(gift);
        }

        private static AsyncSearch<GiftModel> QueryAsync(string saveID)
        {
            return Context.QueryAsync<GiftModel>(saveID);
        }

        private static Task<List<GiftModel>> GetRemainingAsync(AsyncSearch<GiftModel> search)
        {
            return search.GetRemainingAsync();
        }

        public async static Task<List<GiftModel>> QueryAndGetRemainingAsync(string saveID)
        {
            var search = QueryAsync(saveID);
            return await GetRemainingAsync(search);
        }

        public static Task DeleteAsync(List<GiftModel> gifts)
        {
            var batchWrite = Context.CreateBatchWrite<GiftModel>();
            batchWrite.AddDeleteItems(gifts);
            return batchWrite.ExecuteAsync();
        }
    }
}