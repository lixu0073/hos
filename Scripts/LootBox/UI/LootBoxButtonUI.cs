using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hospital
{
    namespace LootBox
    {
        public class LootBoxButtonUI : MonoBehaviour
        {
            private LootBoxData CurrentLootBox;
            //public static DecisionPointCalss decisionPointCalss;
#pragma warning disable 0649
            [SerializeField]
            TextMeshProUGUI timerText;

            [SerializeField]
            Image badgeImage;
#pragma warning restore 0649
            Coroutine timerCoroutine;

            private bool IsInCooldown;
            public bool IsLootBoxInCooldown
            {
                get { return IsInCooldown; }
                private set { IsInCooldown = value; }
            }

            public void Initialize()
            {
                ReferenceHolder.Get().lootBoxManager.OnLootBoxUpdated -= LootBoxManager_OnLootBoxUpdated;
                ReferenceHolder.Get().lootBoxManager.OnLootBoxUpdated += LootBoxManager_OnLootBoxUpdated;
            }

            private void OnDisable()
            {
                StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
            }

            void OnDestroy()
            {
                try
                {
                    ReferenceHolder.Get().lootBoxManager.OnLootBoxUpdated -= LootBoxManager_OnLootBoxUpdated;
                }
                catch (Exception) { }
            }

            void LootBoxManager_OnLootBoxUpdated(LootBoxData lootBoxData)
            {
                CurrentLootBox = lootBoxData;
                if (lootBoxData == null || !lootBoxData.visibleOnMainUI)
                    Hide();
                else
                {
                    Show(lootBoxData);
                }
            }

            public void OnClick()
            {
                StartCoroutine(OnClickCalled());
            }

            private IEnumerator OnClickCalled()
            {
                //if (decisionPointCalss != null)
                //{
                //    decisionPointCalss.ShowWithText();
                //}else if
                if (CurrentLootBox != null)
                {
                    UIController.get.preloaderView.Open(PreloaderView.PreloadViewMode.DeltaDnaPopup);
                    AnalyticsController.instance.ReportLootBoxOpen();
                    //UIController.get.gameEventButton.enabled = false;
                }
                UIController.get.gameEventButton.enabled = false;
                UIController.get.SetEventButtonVisible(false);
                IsLootBoxInCooldown = true;
                yield return new WaitForSeconds(60);
                IsLootBoxInCooldown = false;
                UIController.get.SetEventButtonVisible(true);
                UIController.get.gameEventButton.enabled = true;
            }

            void Show(LootBoxData lootBoxData)
            {
                gameObject.SetActive(true);
                Initialize();

                switch (lootBoxData.box)
                {
                    case Box.blue:
                        badgeImage.sprite = ResourcesHolder.Get().lootBoxMedically;
                        break;
                    case Box.pink:
                        badgeImage.sprite = ResourcesHolder.Get().lootBoxLuxury;
                        break;
                    case Box.xmas:
                        badgeImage.sprite = ResourcesHolder.Get().lootBoxXmas;
                        break;
                    default:
                        badgeImage.sprite = ResourcesHolder.Get().lootBoxMedically;
                        break;
                }

                /*if (timerCoroutine != null)
                    StopCoroutine(timerCoroutine);
                timerCoroutine = StartCoroutine(UpdateTimer());*/
            }

            void Hide()
            {
                gameObject.SetActive(false);

                /*if (timerCoroutine != null)
                    StopCoroutine(timerCoroutine);*/
            }

            IEnumerator UpdateTimer()
            {
                while (true)
                {
                    long timeTillNextDay = GameEventsController.Instance.currentEvent.GetTimeToEnd();
                    //if (timeTillNextDay < 0)
                    //    Hide();

                    timerText.text = UIController.GetFormattedShortTime(timeTillNextDay);
                    yield return new WaitForSeconds(1f);
                }
            }

            public static void OnCloseImage()
            {

            }
        }
    }
}
