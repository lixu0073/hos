using UnityEngine;
using SimpleUI;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace Hospital
{
    public class ExpandPopUpController : UIElement
    {
        [SerializeField] private TextMeshProUGUI cost = null;
        [SerializeField] private TextMeshProUGUI title = null;
        [SerializeField] private Button confirm = null;
        [SerializeField] private UnityEvent onPopupOpen = new UnityEvent();

        private bool isConfirmed = false;

        private bool inTutorialMode = false;

        private enum State
        {
            idle,
            creation
        }

        //private State state = State.idle;

        /// <summary>
        /// Set tutorial mode on or off. This is triggered by tutorial to give free expansion on level 6.
        /// </summary>
        [TutorialTriggerable]
        public void SetTutorialMode(bool isOn)
        {
            if (TutorialSystem.TutorialController.ShowTutorials && isOn)
                inTutorialMode = isOn;
        }

        IMapArea lastUsedAreaData;

        public void Reopen()
        {
            if (lastUsedAreaData != null)
                Open(lastUsedAreaData);
            else
                StartCoroutine(Open());
        }

        public void Open(IMapArea areaData)
        {
            gameObject.SetActive(true);
            StartCoroutine(base.Open(true, false, () =>
            {
                lastUsedAreaData = areaData;

                isConfirmed = false;
                confirm.gameObject.SetActive(true);
                confirm.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                //  HintsController.Get().TryToHideIndicator();

                //cost.text = areaData.GetCost().ToString();
                ExpansionType expansionType = areaData.GetExpansionType();

                int c = DiamondCostCalculator.GetExpansionCost(expansionType, inTutorialMode);
                if (c == 0)
                {
                    cost.text = I2.Loc.ScriptLocalization.Get("FREE");
                    TutorialUIController.Instance.BlinkImage(confirm.GetComponent<Image>(), 1.04f);
                }
                else
                {
                    cost.text = c.ToString();
                    TutorialUIController.Instance.StopBlinking();
                }

                onPopupOpen.Invoke();
                title.text = (I2.Loc.ScriptLocalization.Get("EXPAND_AREA") + " " + I2.Loc.ScriptLocalization.Get(areaData.GetAreaName()) + "!").ToUpper();
            }));
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            base.Exit(hidePopupWithShowMainUI);
            AreaMapController.Map.HideTransformBorder();
            //state = State.idle;
        }

        public bool awaitingConfirmation = false;

        public void ButtonConfirm()
        {
            if (!isConfirmed && !awaitingConfirmation)
            {
                awaitingConfirmation = true;

                isConfirmed = AreaMapController.Map.ConfirmPurchase(
                        () => { awaitingConfirmation = false; },
                        () => { awaitingConfirmation = false; }
                    );
            }
        }

        public void ButtonExit()
        {
            Exit();
        }
    }
}
