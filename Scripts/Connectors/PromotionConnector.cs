using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using UnityEngine;

namespace Hospital.Connectors
{
    public static class PromotionConnector
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

        private static AsyncSearch<PromotionData> search;

        // Start a new search
        public static void FromQueryAsync(string uuid, bool uuidLessOrEqual, int maxLevel, int limit)
        {
            var operationConfig = new DynamoDBOperationConfig()
            {
                IndexName = "PromotionID-UUID-index",
            };

            var expression = new Expression();
            expression.ExpressionAttributeNames["#UUID"] = "UUID";
            expression.ExpressionAttributeNames["#PROMOTION_ID"] = "PromotionID";
            expression.ExpressionAttributeValues[":uuid"] = uuid;
            expression.ExpressionAttributeValues[":promotionID"] = "Promotion";
            string uuidOperator = uuidLessOrEqual ? "<=" : ">";
            expression.ExpressionStatement = "#PROMOTION_ID = :promotionID AND #UUID " + uuidOperator + " :uuid";

            var filterExpression = new Expression();
            filterExpression.ExpressionAttributeNames["#LEVEL"] = "Level";
            filterExpression.ExpressionAttributeValues[":level"] = maxLevel;
            filterExpression.ExpressionStatement = "#LEVEL <= :level";

            var queryConfig = new QueryOperationConfig()
            {
                IndexName = "PromotionID-UUID-index",
                KeyExpression = expression,
                FilterExpression = filterExpression
            };
            if (limit != 0)
                queryConfig.Limit = limit;

            search = Context.FromQueryAsync<PromotionData>(queryConfig, operationConfig);
        }

        public static bool IsSearchDone()
        {
            if (search == null)
            {
                Debug.LogError("No active search");
                return false;
            }
            return search.IsDone;
        }

        // Get results from the search started by FromQueryAsync
        public static Task<List<PromotionData>> GetNextSetAsync()
        {
            if (search == null)
            {
                Debug.LogError("No active search");
                return null;
            }

            return search.GetNextSetAsync();
        }

        // Should be called when the current search is no longer needed
        public static void ClearSearch()
        {
            search = null;
        }
    }
}