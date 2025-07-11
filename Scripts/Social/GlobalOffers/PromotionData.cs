using Amazon.DynamoDBv2.DataModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DynamoDBTable("Promotion")]
public class PromotionData
{
    [DynamoDBHashKey]
    public string PromotionID;

    [DynamoDBRangeKey]
    public string UUID;

    [DynamoDBProperty]
    public int Level;

    [DynamoDBProperty]
    public string UserID;

    [DynamoDBProperty]
    public string OrderID;

    [DynamoDBProperty]
    public long ExpirationTime;

}
