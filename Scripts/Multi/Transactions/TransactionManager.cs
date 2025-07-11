using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;

namespace Transactions
{
    public class TransactionManager : MonoBehaviour
    {

        #region Static

        private static TransactionManager instance;

        public static TransactionManager Instance
        {
            get
            {
                if (instance == null)
                    Debug.LogWarning("There is no TransactionManager instance on scene!");
                return instance;
            }
        }
        void Awake()
        {
            if (instance != null)
                Debug.LogWarning("There are possibly multiple instances of TransactionManager on scene!");
            instance = this;
        }

        #endregion

        #region Treatment Room Help Requests

        public AddRequestInTreatmentRoomTransactionController addRequestInTreatmentRoomTransactionController = new AddRequestInTreatmentRoomTransactionController();
        public RemoveRequestInTreatmentRoomTransactionController removeRequestInTreatmentRoomTransactionController = new RemoveRequestInTreatmentRoomTransactionController();
        public AddDonationInTreatmentRoomTransactionController addDonationInTreatmentRoomTransactionController = new AddDonationInTreatmentRoomTransactionController();

        #endregion

        public void Save(Save save)
        {
            addRequestInTreatmentRoomTransactionController.Save(save);
            removeRequestInTreatmentRoomTransactionController.Save(save);
            addDonationInTreatmentRoomTransactionController.Save(save);
        }

        public void Load(Save save)
        {
            addRequestInTreatmentRoomTransactionController.Load(save);
            removeRequestInTreatmentRoomTransactionController.Load(save);
            addDonationInTreatmentRoomTransactionController.Load(save);
        }
 
    }
}
