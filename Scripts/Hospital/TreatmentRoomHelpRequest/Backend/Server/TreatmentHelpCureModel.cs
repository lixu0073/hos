using Amazon.DynamoDBv2.DataModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Transactions;
using System;

namespace Hospital
{
    namespace TreatmentRoomHelpRequest
    {
        namespace Backend
        {

            /// <summary>
            /// Simple DOA for Treatment Help Cure Entity.
            /// </summary>
            [DynamoDBTable("TreatmentHelpCure1")]
            public class TreatmentHelpCureModel : ITransaction<TreatmentHelpCureModel>
            {

                [DynamoDBHashKey]
                public string SaveID;

                [DynamoDBRangeKey]
                public long ID;

                [DynamoDBProperty]
                public long PatientID;

                [DynamoDBProperty(typeof(MedicineRefConverter))]
                public MedicineRef Medicine;

                [DynamoDBProperty]
                public int Amount;

                [DynamoDBProperty]
                public string HelperSaveID;

                [DynamoDBProperty]
                public bool IsFbFriend = false;

                [DynamoDBProperty]
                public bool SendPush = false;

                public string toString()
                {
                    return ID + ", " + SaveID + ", " + PatientID + ", " + Amount + ", " + HelperSaveID + ", " + SendPush;
                }

                public string GetKey()
                {
                    return SaveID + PatientID + ID;
                }

                public string Stringify()
                {
                    return JsonUtility.ToJson(this);
                }

                public TreatmentHelpCureModel Parse(string unparsedData)
                {
                    return JsonUtility.FromJson<TreatmentHelpCureModel>(unparsedData);
                }
            }
        }
    }
}
