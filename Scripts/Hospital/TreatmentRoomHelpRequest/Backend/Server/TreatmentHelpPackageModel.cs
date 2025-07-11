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
            /// Simple DOA for Treatment Help Package Entity.
            /// </summary>
            [DynamoDBTable("TreatmentHelpPackage1")]
            public class TreatmentHelpPackageModel : ITransaction<TreatmentHelpPackageModel>
            {
                [DynamoDBHashKey]
                public string SaveID;

                [DynamoDBRangeKey]
                public long PatientID;

                [DynamoDBProperty]
                public string UnparsedMedicinesGoals;

                [DynamoDBProperty]
                public List<string> FriendsIds = new List<string>();

                public string toString()
                {
                    return "SaveID: " + SaveID + ", PatientID: " + PatientID + ", UnparsedMedicinesGoals: " + UnparsedMedicinesGoals;
                }

                public string GetKey()
                {
                    return SaveID + PatientID;
                }

                public string Stringify()
                {
                    return JsonUtility.ToJson(this);
                }

                public TreatmentHelpPackageModel Parse(string unparsedData)
                {
                    return JsonUtility.FromJson<TreatmentHelpPackageModel>(unparsedData);
                }
            }
        }
    }
}