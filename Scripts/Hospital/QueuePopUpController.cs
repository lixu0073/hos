using UnityEngine;
using SimpleUI;
using UnityEngine.UI;
using TMPro;

namespace Hospital
{
    public class QueuePopUpController : UIElement
    {
#pragma warning disable 0649
        [SerializeField] private TextMeshProUGUI cost;
        [SerializeField] private TextMeshProUGUI roomName;
        [SerializeField] private Button confirm;
#pragma warning restore 0649
        private int diseaseToDiagnose = -1;
        private int diamondsCost = 0;

        private enum State
        {
            idle,
            creation
        }

        public void Open(IDiagnosePatient patient, string roomName, int cost = 245)
        {
            diseaseToDiagnose = (int)patient.GetAI().GetComponent<HospitalCharacterInfo>().DisaseDiagnoseType;

            CoroutineInvoker.Instance.StartCoroutine(base.Open(false, false, () =>
            {
                confirm.gameObject.SetActive(true);
                confirm.transform.localScale = new Vector3(1, 1, 1);

                this.roomName.text = roomName;
                //cost.text = areaData.GetCost().ToString();
                diamondsCost = cost;
                this.cost.text = diamondsCost.ToString();
            }));
        }

        public void ConfirmPurchase()
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= diamondsCost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(diamondsCost, delegate
                {
                    GameState.Get().RemoveDiamonds(diamondsCost, EconomySource.EnlargeQueue);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(Vector3.zero, diamondsCost, 0, ReferenceHolder.Get().giftSystem.particleSprites[1]);
                    NotificationCenter.Instance.BoughtWithDiamonds.Invoke(new BoughtWithDiamondsEventArgs());
                    HospitalDataHolder.Instance.EnlargeQueue(diseaseToDiagnose);
                    UIController.getHospital.PatientCard.SetCurrentPatientToDiagnose();
                    UIController.getHospital.PatientCard.ShowCurrentPatientDiagnosticRoom();
                    Exit();
                }, this);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            base.Exit(hidePopupWithShowMainUI);
        }

        public void ButtonExit()
        {
            Exit();
        }
    }
}
