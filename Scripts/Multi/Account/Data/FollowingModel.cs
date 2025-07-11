using UnityEngine;
using System.Collections;
using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System;

namespace Hospital
{
    [DynamoDBTable("FollowingList2")]
    public class FollowingModel
    {
        [DynamoDBHashKey]
        public string MeSaveID;

        [DynamoDBRangeKey]
        public string SaveID;
    }
}