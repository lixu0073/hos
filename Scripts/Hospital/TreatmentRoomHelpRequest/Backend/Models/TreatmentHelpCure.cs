using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    namespace TreatmentRoomHelpRequest
    {

        /// <summary>
        /// This class holds data about single donation by player (HelperSaveID)
        /// </summary>
        public class TreatmentHelpCure
        {
            public long ID { get; private set; }
            public long PatientID { get; private set; }
            public string SaveID { get; private set; }
            public MedicineAmount MedicineInfo { get; private set; }
            public bool IsFbFriend = false;
            public bool SendPush = false;

            /// <summary>
            /// Its helper's public SaveID.
            /// </summary>
            public string HelperSaveID { get; private set; }

            public PublicSaveModel HelperPublicModel = null;

            public TreatmentHelpCure(long ID, long PatientID, string SaveID, MedicineAmount MedicineInfo, string HelperSaveID = null, bool IsFbFriend = false, bool SendPush = false)
            {
                this.ID = ID;
                this.PatientID = PatientID;
                this.SaveID = SaveID;
                this.MedicineInfo = MedicineInfo;
                this.HelperSaveID = HelperSaveID;
                this.IsFbFriend = IsFbFriend;
                this.SendPush = SendPush;
            }

        }
    }
}