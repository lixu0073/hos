using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Hospital.Connectors
{
    public static class ContributorConnector
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

        public static Task SaveAsync(ContributorModel contributor)
        {
            return Context.SaveAsync<ContributorModel>(contributor);
        }

        public static Task<ContributorModel> LoadAsync(string eventID, string saveID)
        {
            return Context.LoadAsync<ContributorModel>(eventID, saveID);
        }

        private static AsyncSearch<ContributorModel> FromQueryAsync(string eventID, int limit)
        {
            var operationConfig = new DynamoDBOperationConfig()
            {
                IndexName = "EventID-amount-index"
            };

            var expression = new Expression();
            expression.ExpressionAttributeNames["#EVENT_ID"] = "EventID";
            expression.ExpressionAttributeValues[":event_id"] = eventID;
            expression.ExpressionStatement = "#EVENT_ID = :event_id";

            var queryConfig = new QueryOperationConfig()
            {
                IndexName = "EventID-amount-index",
                KeyExpression = expression,
                Limit = limit,
                BackwardSearch = true
            };

            return Context.FromQueryAsync<ContributorModel>(queryConfig, operationConfig);
        }

        private static Task<List<ContributorModel>> GetNextSetAsync(AsyncSearch<ContributorModel> search)
        {
            return search.GetNextSetAsync();
        }

        public async static Task<List<ContributorModel>> FromQueryAndGetNextSetAsync(string eventID, int limit)
        {
            var search = FromQueryAsync(eventID, limit);
            return await GetNextSetAsync(search);
        }
    }
}
