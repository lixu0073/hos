using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Hospital.Connectors
{
    public static class FriendsConnector
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

        public static Task SaveFollowingAsync(FollowingModel following)
        {
            return Context.SaveAsync<FollowingModel>(following);
        }

        public static Task<FollowingModel> LoadFollowingAsync(string saveID, string targetSaveID)
        {
            return Context.LoadAsync<FollowingModel>(saveID, targetSaveID);
        }

        private static AsyncSearch<FollowingModel> QueryFollowingAsync(string saveID)
        {
            return Context.QueryAsync<FollowingModel>(saveID);
        }

        private static Task<List<FollowingModel>> GetRemainingFollowingAsync(AsyncSearch<FollowingModel> search)
        {
            return search.GetRemainingAsync();
        }

        public async static Task<List<FollowingModel>> QueryAndGetRemainingFollowingAsync(string saveID)
        {
            var search = QueryFollowingAsync(saveID);
            return await GetRemainingFollowingAsync(search);
        }

        public static Task DeleteFollowingAsync(string saveID, string visitingSaveID)
        {
            return Context.DeleteAsync<FollowingModel>(saveID, visitingSaveID);
        }

        public static Task SaveFollowerAsync(FollowerModel follower)
        {
            return Context.SaveAsync<FollowerModel>(follower);
        }

        public static Task DeleteFollowerAsync(string visitingSaveID, string saveID)
        {
            return Context.DeleteAsync<FollowerModel>(visitingSaveID, saveID);
        }
    }
}