using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DynamoDBv2.DataModel;

namespace Hospital
{
    [DynamoDBTable("Analitics")]
    public class AnalyticsModel
    {
        [DynamoDBHashKey]
        public string CognitoID;

        [DynamoDBProperty]
        public int type;

        [DynamoDBProperty]
        public string CurrentTime;

        [DynamoDBProperty]
        public float AssetBundleProgressStatus;

        [DynamoDBProperty]
        public bool AssetBundleDownloading;

        [DynamoDBProperty]
        public float AssetBundleDownloadingTime;

        [DynamoDBProperty]
        public bool FirstLaunch;

        [DynamoDBProperty]
        public long DiskSpaceInBytes;

        [DynamoDBProperty]
        public int MemorySize;

        [DynamoDBProperty]
        public string OSVersion;

        [DynamoDBProperty]
        public string DeviceModel;

        [DynamoDBProperty]
        public string LanguageCode;

        [DynamoDBProperty]
        public string CountryCode;

        [DynamoDBProperty]
        public bool ViaWiFi;

        [DynamoDBProperty]
        public int GameVersion;

        [DynamoDBProperty]
        public bool Reported;

        [DynamoDBProperty]
        public bool Launched;

        [DynamoDBProperty]
        public bool EnteredMainScene;

    }
}
