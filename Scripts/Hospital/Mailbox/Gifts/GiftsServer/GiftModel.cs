using Amazon.DynamoDBv2.DataModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    [DynamoDBTable("Gift")]
    public class GiftModel
    {
        [DynamoDBHashKey]
        public string TargetSaveID;

        [DynamoDBRangeKey]
        public string ID;

        [DynamoDBProperty]
        public string SaveID;

        [DynamoDBProperty]
        public bool IsThankYouGift = false;

        public string toString()
        {
            return TargetSaveID + "," + ID + "," + SaveID + "," + IsThankYouGift;
        }
    }
}