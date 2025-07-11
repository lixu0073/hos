using System;
using System.Collections;
using System.Collections.Generic;
using Hospital;
using UnityEngine;
using Hospital.TreatmentRoomHelpRequest.Backend;

namespace Transactions
{

    public class RemoveRequestInTreatmentRoomTransactionController : BaseTransactionController<TreatmentHelpPackageModel>
    {
        public override void Load(Save save)
        {
            unparsedTransactions = MapTransactions(save.RemoveRequestInTreatmentRoomTransaction);
        }

        public override void ResendAction(string key, string unparsedModel)
        {
            TreatmentHelpPackageModel model = new TreatmentHelpPackageModel().Parse(unparsedModel);

            ReferenceHolder.GetHospital().treatmentHelpAPI.RemoveRequestLocal(TreatmentRoomHelpMapper.Map(model));

            TransactionManager.Instance.removeRequestInTreatmentRoomTransactionController.AddTransaction(model);
            ReferenceHolder.GetHospital().treatmentHelpAPI.DeleteHelpRequest(model.SaveID, model.PatientID, () => {
                TransactionManager.Instance.removeRequestInTreatmentRoomTransactionController.CompleteTransaction(model);
            });
        }

        public override void Save(Save save)
        {
            save.RemoveRequestInTreatmentRoomTransaction = MapTransactions(unparsedTransactions);
        }
    }

}
