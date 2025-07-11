using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Linq;

namespace Hospital.Connectors
{
    public static class RecompensationGiftConnector
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

        private static AsyncSearch<RecompensationGiftModel> QueryAsync(string saveID)
        {
            return Context.QueryAsync<RecompensationGiftModel>(saveID);
        }

        private static Task<List<RecompensationGiftModel>> GetRemainingAsync(AsyncSearch<RecompensationGiftModel> search)
        {
            return search.GetRemainingAsync();
        }

        public async static Task<List<RecompensationGiftModel>> QueryAndGetRemainingAsync(string saveID)
        {
            var search = QueryAsync(saveID);
            return await GetRemainingAsync(search);
        }

        // CV: TODO improve performance (this can be simplyfied a lot)
        public static Task DeleteAsync(List<RecompensationGiftModel> gifts)
        {
            // To avoid trying to delete duplicates we need to create as many Batches as needed
            var noDupes = gifts.Where(p => p.TransactionID != null).GroupBy(p => new { p.TransactionID, p.UserID }).Select(grp => grp.FirstOrDefault());            
            var rest = gifts.Except(noDupes);            

            List<BatchWrite> batchWritesList = new List<BatchWrite>();
            var batchWrite = Context.CreateBatchWrite<RecompensationGiftModel>();
            batchWrite.AddDeleteItems(noDupes);
            batchWritesList.Add(batchWrite);
            IEnumerable<RecompensationGiftModel> otherNoDupes;

            foreach (var item in rest)
            {
                otherNoDupes = rest.Where(p => p.TransactionID != null).GroupBy(p => new { p.TransactionID, p.UserID }).Select(grp => grp.FirstOrDefault());
                rest = gifts.Except(otherNoDupes);
                batchWrite = Context.CreateBatchWrite<RecompensationGiftModel>();
                batchWrite.AddDeleteItems(otherNoDupes);
                batchWritesList.Add(batchWrite);
            }            

            var Tasks = new List<Task>();
            foreach (var item in batchWritesList)
            {
                Tasks.Add(item.ExecuteAsync());
            }
            var ToWaitFor = Tasks.ToArray();

            return Task.WhenAll(ToWaitFor);

            //var batchWrite = Context.CreateBatchWrite<RecompensationGiftModel>();
            //batchWrite.AddDeleteItems(/*gifts*/noDupes);
            //return batchWrite.ExecuteAsync();
        }
    }
}