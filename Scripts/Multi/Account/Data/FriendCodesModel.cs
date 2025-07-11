using Amazon.DynamoDBv2.DataModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{

    [DynamoDBTable("FriendCodes")]
    public class FriendCodesModel
    {
        [DynamoDBHashKey]
        public string code;

        [DynamoDBProperty]
        public string SaveID;

    }

}
