using UnityEngine;
using System.Collections;
using SimpleUI;
using UnityEngine.UI;

namespace Hospital
{
    public class UpdateRewardPopUp : UIElement
    {
        const int UPDATE_DIAMOND_AMOUNT = 5;

        public ScrollRect scrollRect;
        public GameObject contentStorage;
        public GameObject contentDefault;
        public Button claimButton;


        public void Open()
        {
            CoroutineInvoker.Instance.StartCoroutine(base.Open(true, true, () =>
            {
                claimButton.onClick.RemoveListener(ButtonClaimReward);
                claimButton.onClick.AddListener(ButtonClaimReward);

                //contentStorage.SetActive(true);
                //contentDefault.SetActive(false);
                //StartCoroutine(ScrollUpEffect());

                contentStorage.SetActive(false);
                contentDefault.SetActive(true);
            }));
        }

        IEnumerator ScrollUpEffect()
        {
            //Debug.LogError("ScrollUpEffect");

            float normPos = 0;
            scrollRect.verticalNormalizedPosition = normPos;
            yield return new WaitForSeconds(2f);

            while (normPos < 1)
            {
                normPos += Time.deltaTime / 3;
                scrollRect.verticalNormalizedPosition = normPos;
                yield return null;
            }
        }
        
        public void ButtonStorageOK()
        {
            contentStorage.SetActive(false);
            contentDefault.SetActive(true);
        }

        public void ButtonClaimReward()
        {
            claimButton.onClick.RemoveListener(ButtonClaimReward);
            //RewardPlayer();
            Exit();
        }

        void RewardPlayer()
        {
            int currentDiamondAmount = Game.Instance.gameState().GetDiamondAmount();
            GameState.Get().AddResource(ResourceType.Diamonds, UPDATE_DIAMOND_AMOUNT, EconomySource.UpdateReward, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Diamond, Vector3.zero, UPDATE_DIAMOND_AMOUNT, .25f, 1.75f, Vector3.one, Vector3.one, null, null, () => {
                GameState.Get().UpdateCounter(ResourceType.Diamonds, UPDATE_DIAMOND_AMOUNT, currentDiamondAmount);
            });
        }
    }
}