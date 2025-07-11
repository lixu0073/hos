using Amazon.DynamoDBv2.DataModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    [DynamoDBTable("RecompensationGifts")]
    public class RecompensationGiftModel
    {
        [DynamoDBHashKey]
        public string UserID;

        [DynamoDBRangeKey]
        public string TransactionID;

        [DynamoDBProperty]
        public string Reward;

        [DynamoDBProperty]
        public string RewardType;
    }
}

