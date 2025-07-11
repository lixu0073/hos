using System.Collections.Generic;
using Hospital.TreatmentRoomHelpRequest.Backend;
using Hospital.TreatmentRoomHelpRequest;
using Hospital;

namespace Transactions
{

    public class AddDonationInTreatmentRoomTransactionController : BaseTransactionController<TreatmentHelpCureModel>
    {
        public override void Load(Save save)
        {
            unparsedTransactions = MapTransactions(save.AddDonationInTreatmentRoomTransactions);
        }

        public override void ResendAction(string key, string unparsedModel)
        {
            TreatmentHelpCureModel model = new TreatmentHelpCureModel().Parse(unparsedModel);
            ReferenceHolder.GetHospital().treatmentHelpAPI.AddHelpersLocal(new List<TreatmentHelpCure>() { TreatmentRoomHelpMapper.Map(model) });
            TransactionManager.Instance.addDonationInTreatmentRoomTransactionController.AddTransaction(model);
            ReferenceHolder.GetHospital().treatmentHelpAPI.PostHelpInMedicine(model, () => {
                TransactionManager.Instance.addDonationInTreatmentRoomTransactionController.CompleteTransaction(model);
            });
        }

        public override void Save(Save save)
        {
            save.AddDonationInTreatmentRoomTransactions = MapTransactions(unparsedTransactions);
        }
    }

}
