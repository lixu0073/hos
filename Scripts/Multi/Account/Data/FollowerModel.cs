using UnityEngine;
using System.Collections;
using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System;

namespace Hospital
{
    [DynamoDBTable("FollowersList2")]
    public class FollowerModel
    {
        [DynamoDBHashKey]
        public string MeSaveID;

        [DynamoDBRangeKey]
        public string SaveID;

    }
}