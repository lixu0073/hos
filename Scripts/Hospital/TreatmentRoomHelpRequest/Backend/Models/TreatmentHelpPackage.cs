using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    namespace TreatmentRoomHelpRequest
    {

        /// <summary>
        /// This class holds information about help request in treatment rooms for specific patient, like:
        /// target medicines goal, current status of donation for each medicine...
        /// </summary>
        public class TreatmentHelpPackage
        {

            /// <summary>
            /// Treatment Patient ID
            /// </summary>
            public long PatientID { get; private set; }


            /// <summary>
            /// Player saveID, which has (probably) help requests.
            /// In case of creation instance of this class for put'ing to AWS - saveID should be get from CognitoEntry.saveID
            /// </summary>
            public string SaveID { get; private set; }


            /// <summary>
            /// List of medicines (with target quantity) to donate by other players.
            /// </summary>
            public readonly List<MedicineAmount> MedicinesGoals;

            
            /// <summary>
            /// List of players who helped patient in treatment room for specific medicine.
            /// </summary>
            public List<TreatmentHelpCure> Helpers { get; private set; }


            /// <summary>
            /// Reference to patient in treatment room who has help request.
            /// </summary>
            public HospitalCharacterInfo patient;

            /// <summary>
            /// Friends saves ids list - used to do push notifiaction after create new Help Request
            /// </summary>
            public List<string> FriendsSavesIDs = new List<string>();

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="PatientID">Treatment Patient ID</param>
            /// <param name="SaveID">Player saveID, which has (probably) help requests.
            /// In case of creation instance of this class for put'ing to AWS - saveID should be get from CognitoEntry.saveID</param>
            /// <param name="MedicinesGoals">List of medicines (with target quantity) to donate by other players.</param>
            /// <param name="Helpers">List of players who helped patient in treatment room for specific medicine.</param>
            public TreatmentHelpPackage(long PatientID, string SaveID, List<MedicineAmount> MedicinesGoals, List<TreatmentHelpCure> Helpers = null, List<string> FriendsSavesIDs = null)
            {
                this.PatientID = PatientID;
                this.SaveID = SaveID;
                this.MedicinesGoals = MedicinesGoals;
                this.Helpers = Helpers;
                this.FriendsSavesIDs = FriendsSavesIDs == null ? new List<string>() : FriendsSavesIDs;
            }

        }
    }
}
