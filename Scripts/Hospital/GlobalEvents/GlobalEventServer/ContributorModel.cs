using Amazon.DynamoDBv2.DataModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Hospital
{
    [DynamoDBTable("Contributor")]
    public class ContributorModel
    {
        [DynamoDBHashKey]
        public string EventID;

        [DynamoDBRangeKey]
        public string SaveID;

        [DynamoDBProperty]
        public int amount;

        public string toString()
        {
            return EventID + "," + SaveID + "," + amount;
        }

        public static ContributorModel GetInstance(string data)
        {
            string[] array = data.Split(',');
            if (array.Length != 3)
                return null;
            return new ContributorModel()
            {
                EventID = array[0],
                SaveID = array[1],
                amount = int.Parse(array[2], System.Globalization.CultureInfo.InvariantCulture)
            };
        }

    }
}