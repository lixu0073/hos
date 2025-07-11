using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Hospital.Connectors
{
    public static class PublicSaveConnector
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

        public static Task SaveAsync(PublicSaveModel save)
        {
            return Context.SaveAsync<PublicSaveModel>(save);
        }

        public static Task<PublicSaveModel> LoadAsync(string hash)
        {
            return Context.LoadAsync<PublicSaveModel>(hash);
        }

        public static async Task<List<PublicSaveModel>> BatchGetAsync(List<string> keys)
        {
            var batch = Context.CreateBatchGet<PublicSaveModel>();
            foreach(string key in keys)
                batch.AddKey(key);
            await batch.ExecuteAsync();
            return batch.Results;
        }
    }
}