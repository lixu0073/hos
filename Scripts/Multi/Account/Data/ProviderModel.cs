using UnityEngine;
using System.Collections;
using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System;

namespace Hospital
{

    [DynamoDBTable("Providers")]
    public class ProviderModel
    {
        [DynamoDBHashKey]
        public string ProviderID;

        [DynamoDBProperty]
        public string SaveID;

        [DynamoDBProperty]
        public int Level = 1;
    }
}