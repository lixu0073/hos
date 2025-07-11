using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Hospital.TreatmentRoomHelpRequest.Backend;

namespace Hospital.Connectors
{
    public static class TreatmentHelpConnector
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

        public static Task SavePackageAsync(TreatmentHelpPackageModel package)
        {
            return Context.SaveAsync<TreatmentHelpPackageModel>(package);
        }

        private static AsyncSearch<TreatmentHelpPackageModel> QueryPackageAsync(string saveID)
        {
            return Context.QueryAsync<TreatmentHelpPackageModel>(saveID);
        }

        private static Task<List<TreatmentHelpPackageModel>> GetRemainingPackageAsync(AsyncSearch<TreatmentHelpPackageModel> search)
        {
            return search.GetRemainingAsync();
        }

        public async static Task<List<TreatmentHelpPackageModel>> QueryAndGetRemainingPackageAsync(string saveID)
        {
            var search = QueryPackageAsync(saveID);
            return await GetRemainingPackageAsync(search);
        }

        public static Task DeletePackageAsync(string saveID, long patientID)
        {
            return Context.DeleteAsync<TreatmentHelpPackageModel>(saveID, patientID);
        }

        public static Task SaveCureAsync(TreatmentHelpCureModel cure)
        {
            return Context.SaveAsync<TreatmentHelpCureModel>(cure);
        }

        private static AsyncSearch<TreatmentHelpCureModel> QueryCureAsync(string saveID)
        {
            return Context.QueryAsync<TreatmentHelpCureModel>(saveID);
        }

        private static Task<List<TreatmentHelpCureModel>> GetRemainingCureAsync(AsyncSearch<TreatmentHelpCureModel> search)
        {
            return search.GetRemainingAsync();
        }

        public async static Task<List<TreatmentHelpCureModel>> QueryAndGetRemainingCureAsync(string saveID)
        {
            var search = QueryCureAsync(saveID);
            return await GetRemainingCureAsync(search);
        }
    }
}