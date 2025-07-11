using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Hospital.Connectors
{
    public static class InGameFriendConnector
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

        public static Task SaveAsync(InGameFriendModel inGameFriend)
        {
            return Context.SaveAsync<InGameFriendModel>(inGameFriend);
        }

        private static AsyncSearch<InGameFriendModel> FromQueryAsync(string meSaveID)
        {
            Expression expression = new Expression();
            expression.ExpressionAttributeNames["#MeSaveID"] = "MeSaveID";
            expression.ExpressionAttributeValues[":MeSaveID"] = meSaveID;
            expression.ExpressionStatement = "#MeSaveID = :MeSaveID";

            QueryOperationConfig queryConfig = new QueryOperationConfig()
            {
                IndexName = "MeSaveID-InvitationTimestamp-index",
                KeyExpression = expression,
            };

            return Context.FromQueryAsync<InGameFriendModel>(queryConfig);
        }

        private static Task<List<InGameFriendModel>> GetNextSetAsync(AsyncSearch<InGameFriendModel> search)
        {
            return search.GetNextSetAsync();
        }

        public async static Task<List<InGameFriendModel>> FromQueryAndGetNextSetAsync(string meSaveID)
        {
            var search = FromQueryAsync(meSaveID);
            if (search == null)
                return null;
            return await GetNextSetAsync(search);
        }

        public static Task DeleteAsync(string meSaveID, string saveID)
        {
            return Context.DeleteAsync<InGameFriendModel>(meSaveID, saveID);
        }
    }
}