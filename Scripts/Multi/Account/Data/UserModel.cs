using Amazon.DynamoDBv2.DataModel;

namespace Hospital
{

    [DynamoDBTable("Users")]
    public class UserModel
    {

        public enum Providers
        {
            DEFAULT = 1,
            FACEBOOK = 2,
            GAMECENTER = 3 // This is also used for GOOGLE PLAY
        };

        [DynamoDBHashKey] public string ID;

        [DynamoDBProperty] public string SaveID;

        [DynamoDBProperty] public string FacebookID;

        [DynamoDBProperty] public string GameCenterID;

        [DynamoDBProperty] public int CurrentProvider;

        public string Preety()
        {
            return "ID: " + ID + ", SaveID: " + SaveID + ", FacebookID: " + FacebookID + ", GameCenterID: " + GameCenterID + ", CurrentProvider: " + CurrentProvider;
        }
    }
}