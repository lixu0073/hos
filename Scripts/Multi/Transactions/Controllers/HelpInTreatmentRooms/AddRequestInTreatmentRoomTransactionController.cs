using UnityEngine;
using Hospital.TreatmentRoomHelpRequest.Backend;
using Hospital;

namespace Transactions
{

    public class AddRequestInTreatmentRoomTransactionController : BaseTransactionController<TreatmentHelpPackageModel>
    {
        public override void Load(Save save)
        {
            unparsedTransactions = MapTransactions(save.AddRequestInTreatmentRoomTransactions);
        }

        public override void ResendAction(string key, string unparsedModel)
        {
            TreatmentHelpPackageModel model = new TreatmentHelpPackageModel().Parse(unparsedModel);

            ReferenceHolder.GetHospital().treatmentHelpAPI.AddRequestLocal(TreatmentRoomHelpMapper.Map(model));

            TransactionManager.Instance.addRequestInTreatmentRoomTransactionController.AddTransaction(model);
            ReferenceHolder.GetHospital().treatmentHelpAPI.PostHelpRequest(model, () => {
                TransactionManager.Instance.addRequestInTreatmentRoomTransactionController.CompleteTransaction(model);
            }, (ex) => {
                Debug.LogError(ex.Message);
            });
        }

        public override void Save(Save save)
        {
            save.AddRequestInTreatmentRoomTransactions = MapTransactions(unparsedTransactions);
        }
    }
}
