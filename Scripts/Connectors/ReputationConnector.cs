using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ReputationSystem;

namespace Hospital.Connectors
{
    public static class ReputationConnector
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

        public static Task SaveAsync(ReputationModel reputation)
        {
            return Context.SaveAsync<ReputationModel>(reputation);
        }

        public static Task<ReputationModel> LoadAsync(string eventID, string saveID)
        {
            return Context.LoadAsync<ReputationModel>(eventID, saveID);
        }

        private static AsyncSearch<ReputationModel> FromQueryAsync(string categoryID, int limit)
        {
            var operationConfig = new DynamoDBOperationConfig()
            {
                IndexName = "EventID-amount-index"
            };

            var expression = new Expression();
            expression.ExpressionAttributeNames["#CATEGORY_ID"] = "CategoryID";
            expression.ExpressionAttributeValues[":category_id"] = categoryID;
            expression.ExpressionStatement = "#CATEGORY_ID = :category_id";

            var queryConfig = new QueryOperationConfig()
            {
                IndexName = "CategoryID-Amount-index",
                KeyExpression = expression,
                Limit = limit,
                BackwardSearch = true
            };

            return Context.FromQueryAsync<ReputationModel>(queryConfig, operationConfig);
        }

        private static Task<List<ReputationModel>> GetNextSetAsync(AsyncSearch<ReputationModel> search)
        {
            return search.GetNextSetAsync();
        }

        public async static Task<List<ReputationModel>> FromQueryAndGetNextSetAsync(string categoryID, int limit)
        {
            var search = FromQueryAsync(categoryID, limit);
            return await GetNextSetAsync(search);
        }
    }
}
