using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Hospital.Connectors
{
    public static class PharmacyOrderConnector
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

        public static Task SaveStandardAsync(PharmacyOrderStandard order)
        {
            return Context.SaveAsync<PharmacyOrderStandard>(order);
        }

        public static Task<PharmacyOrderStandard> LoadStandardAsync(PharmacyOrderStandard order)
        {
            return Context.LoadAsync<PharmacyOrderStandard>(order);
        }

        private static AsyncSearch<PharmacyOrderStandard> QueryStandardAsync(string saveID)
        {
            return Context.QueryAsync<PharmacyOrderStandard>(saveID);
        }

        private static Task<List<PharmacyOrderStandard>> GetRemainingStandardAsync(AsyncSearch<PharmacyOrderStandard> search)
        {
            return search.GetRemainingAsync();
        }

        public async static Task<List<PharmacyOrderStandard>> QueryAndGetRemainingStandardAsync(string saveID)
        {
            var search = QueryStandardAsync(saveID);
            return await GetRemainingStandardAsync(search);
        }

        private static AsyncSearch<PharmacyOrderStandard> FromQueryStandardAsync(string saveID, int maxLevel)
        {
            var expression = new Expression();
            expression.ExpressionAttributeNames["#USER_ID"] = "UserID";
            expression.ExpressionAttributeValues[":userid"] = saveID;
            expression.ExpressionStatement = "#USER_ID = :userid";

            var filterExpression = new Expression();
            filterExpression.ExpressionAttributeNames["#LEVEL"] = "requiredLevel";
            filterExpression.ExpressionAttributeValues[":level"] = maxLevel;

            filterExpression.ExpressionAttributeNames["#BOUGHT"] = "bought";
            filterExpression.ExpressionAttributeValues[":bought"] = false;

            filterExpression.ExpressionAttributeNames["#BOUGHT_BY_WISE"] = "bougthBuyWise";
            filterExpression.ExpressionAttributeValues[":bougthBuyWise"] = false;

            filterExpression.ExpressionStatement = "#LEVEL <= :level AND #BOUGHT = :bought AND #BOUGHT_BY_WISE = :bougthBuyWise";

            var queryConfig = new QueryOperationConfig()
            {
                KeyExpression = expression,
                FilterExpression = filterExpression,
                Limit = 50
            };

            return Context.FromQueryAsync<PharmacyOrderStandard>(queryConfig);
        }

        private static Task<List<PharmacyOrderStandard>> GetNextSetStandardAsync(AsyncSearch<PharmacyOrderStandard> search)
        {
            return search.GetNextSetAsync();
        }

        public async static Task<List<PharmacyOrderStandard>> FromQueryAndGetNextSetStandardAsync(string saveID, int limit)
        {
            var search = FromQueryStandardAsync(saveID, limit);
            return await GetNextSetStandardAsync(search);
        }

        public static Task DeleteStandardAsync(PharmacyOrderStandard order)
        {
            return Context.DeleteAsync<PharmacyOrderStandard>(order);
        }

        public static Task SaveAdvertisedAsync(PharmacyOrderAdvertised order)
        {
            return Context.SaveAsync<PharmacyOrderAdvertised>(order);
        }

        public static Task<PharmacyOrderAdvertised> LoadAdvertisedAsync(PharmacyOrderAdvertised order)
        {
            return Context.LoadAsync<PharmacyOrderAdvertised>(order);
        }

        public static async Task<List<PharmacyOrderAdvertised>> BatchGetAdvertisedAsync(List<(string saveID, string orderID)> keys)
        {
            var batch = Context.CreateBatchGet<PharmacyOrderAdvertised>();
            foreach(var keyPair in keys)
                batch.AddKey(keyPair.saveID, keyPair.orderID);
            await batch.ExecuteAsync();
            return batch.Results;
        }

        private static AsyncSearch<PharmacyOrderAdvertised> QueryAdvertisedAsync(string saveID)
        {
            return Context.QueryAsync<PharmacyOrderAdvertised>(saveID);
        }

        private static Task<List<PharmacyOrderAdvertised>> GetRemainingAdvertisedAsync(AsyncSearch<PharmacyOrderAdvertised> search)
        {
            return search.GetRemainingAsync();
        }

        public async static Task<List<PharmacyOrderAdvertised>> QueryAndGetRemainingAdvertisedAsync(string saveID)
        {
            var search = QueryAdvertisedAsync(saveID);
            return await GetRemainingAdvertisedAsync(search);
        }

        private static AsyncSearch<PharmacyOrderAdvertised> FromQueryAdvertisedAsync(string saveID, int maxLevel)
        {
            var expression = new Expression();
            expression.ExpressionAttributeNames["#USER_ID"] = "UserID";
            expression.ExpressionAttributeValues[":userid"] = saveID;
            expression.ExpressionStatement = "#USER_ID = :userid";

            var filterExpression = new Expression();
            filterExpression.ExpressionAttributeNames["#LEVEL"] = "requiredLevel";
            filterExpression.ExpressionAttributeValues[":level"] = maxLevel;

            filterExpression.ExpressionAttributeNames["#BOUGHT"] = "bought";
            filterExpression.ExpressionAttributeValues[":bought"] = false;

            filterExpression.ExpressionAttributeNames["#BOUGHT_BY_WISE"] = "bougthBuyWise";
            filterExpression.ExpressionAttributeValues[":bougthBuyWise"] = false;

            filterExpression.ExpressionStatement = "#LEVEL <= :level AND #BOUGHT = :bought AND #BOUGHT_BY_WISE = :bougthBuyWise";

            var queryConfig = new QueryOperationConfig()
            {
                KeyExpression = expression,
                FilterExpression = filterExpression,
                Limit = 50
            };

            return Context.FromQueryAsync<PharmacyOrderAdvertised>(queryConfig);
        }

        private static Task<List<PharmacyOrderAdvertised>> GetNextSetAdvertisedAsync(AsyncSearch<PharmacyOrderAdvertised> search)
        {
            return search.GetNextSetAsync();
        }

        public async static Task<List<PharmacyOrderAdvertised>> FromQueryAndGetNextSetAdvertisedAsync(string saveID, int limit)
        {
            var search = FromQueryAdvertisedAsync(saveID, limit);
            return await GetNextSetAdvertisedAsync(search);
        }

        public static Task DeleteAdvertisedAsync(PharmacyOrderAdvertised order)
        {
            return Context.DeleteAsync<PharmacyOrderAdvertised>(order);
        }

        public static Task PutAdvertisedAsync(List<PharmacyOrderAdvertised> orders)
        {
            var batchWrite = Context.CreateBatchWrite<PharmacyOrderAdvertised>();
            batchWrite.AddPutItems(orders);
            return batchWrite.ExecuteAsync();
        }

        private static AsyncSearch<OldPharmacyOrderAdvertised> QueryOldAdvertisedAsync(string saveID)
        {
            return Context.QueryAsync<OldPharmacyOrderAdvertised>(saveID);
        }

        private static Task<List<OldPharmacyOrderAdvertised>> GetRemainingOldAdvertisedAsync(AsyncSearch<OldPharmacyOrderAdvertised> search)
        {
            return search.GetRemainingAsync();
        }

        public async static Task<List<OldPharmacyOrderAdvertised>> QueryAndGetRemainingOldAdvertisedAsync(string saveID)
        {
            var search = QueryOldAdvertisedAsync(saveID);
            return await GetRemainingOldAdvertisedAsync(search);
        }

        public static Task DeleteOldAdvertisedAsync(List<OldPharmacyOrderAdvertised> orders)
        {
            var batchWrite = Context.CreateBatchWrite<OldPharmacyOrderAdvertised>();
            batchWrite.AddDeleteItems(orders);
            return batchWrite.ExecuteAsync();
        }
    }
}