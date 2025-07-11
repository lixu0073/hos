using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleUI;
using System;

namespace Hospital
{
    /// <summary>
    /// 增益道具菜单弹窗控制器，负责管理增益道具列表界面的显示和交互。
    /// 处理增益道具列表的动态生成、滚动定位、购买交互和状态刷新等功能。
    /// </summary>
    public class BoosterMenuPopUpController : UIElement
    {
        [SerializeField] private ScrollRect scrollRect = null;
        [SerializeField] private GameObject boosterList = null;
        [SerializeField] private GameObject scrollBar = null;
        [SerializeField] private GameObject boosterPanelPrefab = null;
        [SerializeField] private GameObject specialBoosterPanelPrefab = null;

        Coroutine HideScrollbarCorutine = null;

        private List<GameObject> boosterCards = new List<GameObject>();
        //private bool isInfoOn = false;
        private int infoBoosterID = -1;
        public int InfoBoosterID { get { return infoBoosterID; } }

        Coroutine _scrollBoosters;

        private void OnDisable()
        {
            if (_scrollBoosters != null)
            {
                try
                { 
                    StopCoroutine(_scrollBoosters);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
            }
        }

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            yield return base.Open(isFadeIn, preservesHovers, null);

            SetBoosterList();
            scrollBar.SetActive(false);
            ScrollBoosterList();
            HospitalAreasMapController.HospitalMap.boosterManager.ClearIndicators();

            NotificationCenter.Instance.BoostersPopUpOpen.Invoke(new BaseNotificationEventArgs());
            AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.Booster.ToString(), (int)FunnelStepBoosters.PopUpOpen, FunnelStepBoosters.PopUpOpen.ToString());

            whenDone?.Invoke();
        }

        public void ButtonExit()
        {
            Exit();
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            NotificationCenter.Instance.BoosterPopupClosed.Invoke(new BaseNotificationEventArgs());
            base.Exit(hidePopupWithShowMainUI);
        }

        public void RefreshCurrent()
        {
            SetBoosterList();
        }

        private void SetBoosterList()
        {
            //isInfoOn = false;
            boosterList.SetActive(true);

            int boostersCount = boosterList.transform.childCount;
            for (int j = 0; j < boostersCount; ++j)
            {
                Destroy(boosterList.transform.GetChild(j).gameObject);
            }

            boosterCards.Clear();
            for (int j = 0; j < ResourcesHolder.Get().boosterDatabase.boosters.Length; ++j)
            {
                GameObject tmp;
                if (ResourcesHolder.Get().boosterDatabase.boosters[j].canBuy)
                    tmp = Instantiate(boosterPanelPrefab, boosterList.transform.position, boosterList.transform.rotation) as GameObject;
                else tmp = Instantiate(specialBoosterPanelPrefab, boosterList.transform.position, boosterList.transform.rotation) as GameObject;
                tmp.transform.SetParent(boosterList.transform);
                tmp.transform.localScale = Vector3.one;
                tmp.GetComponent<BoosterPanelController>().FillCard(j);
                tmp.GetComponent<BoosterPanelController>().Refresh(j);
            }

            // set last two dailyQuest boosters at first positions
            for (int i = 0; i < boosterList.transform.childCount; ++i)
            {
                if (i > boosterList.transform.childCount - 3)
                {
                    boosterList.transform.GetChild(i).SetAsFirstSibling();
                }
            }
        }

        public void BackButton()
        {
            SetBoosterList();
        }

        public void GetBoosterForDiamonds()
        {
            int cost = ResourcesHolder.Get().boosterDatabase.boosters[infoBoosterID].Price;

            if (Game.Instance.gameState().GetDiamondAmount() >= cost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
                {
                    GameState.Get().RemoveDiamonds(cost, EconomySource.GetBooster);
                    HospitalAreasMapController.HospitalMap.boosterManager.AddBooster(infoBoosterID, EconomySource.GetBooster, false);
                    RefreshCurrent();
                }, this);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        public void ShowScrollbar()
        {
            scrollBar.SetActive(true);
            try
            {
                IsoEngine.UtilsCoroutine.Instance.StopCoroutineMy(HideScrollbarCorutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            HideScrollbarCorutine = IsoEngine.UtilsCoroutine.Instance.StartCoroutine(HideScrollbar());
        }

        IEnumerator HideScrollbar()
        {
            yield return new WaitForSeconds(0.5f);
            scrollBar.SetActive(false);
        }

        public void ScrollBoosterList()
        {
            int firstNewIndex = HospitalAreasMapController.HospitalMap.boosterManager.GetNewBoosterIndex();

            //because 2 'PREMIUM' boosters (index 8 and 9) are positioned as first in the hierarchy we need to adjust to get the child index
            if (firstNewIndex >= 8)
                firstNewIndex -= 8;
            else
                firstNewIndex += 2;

            _scrollBoosters = StartCoroutine(ScrollBoosters(firstNewIndex));
        }

        IEnumerator ScrollBoosters(int index)
        {
            //Debug.LogError("ScrollToNew index " + index);
            scrollRect.verticalNormalizedPosition = 1f;     //set the scroll rect position at the top (default)

            /*
            //if It's the first time this pop up is open a non linear tutorial will show. We need to wait till its closed to scroll the list
            if (!NotificationCenter.Instance.BoostersPopUpOpen.IsNull())
            {
                index = -1;
                scrollRect.verticalNormalizedPosition = 0f;     //set the scroll rect position at the bottom
                yield return new WaitForSeconds(0.5f);          //wait for the fullscreen tutorial to open
                
                while (TutorialUIController.Instance.IsFullscreenTutorialActive())
                {
                    //wait for the fullscreen tutorial to be closed
                    yield return new WaitForSeconds(0.2f);
                }
            }
            */

            float targetPos = GetTargetPos(index);
            float timer = 0f;

            if (scrollRect.verticalNormalizedPosition == targetPos)
                yield break;

            //Debug.LogError("Will scroll to pos: " + targetPos);
            while (true)
            {
                timer += Time.deltaTime;
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, targetPos, .075f);

                //Debug.LogError(" vertpos = " + scrollRect.verticalNormalizedPosition);
                if (timer > 3.5f || Mathf.Abs(scrollRect.verticalNormalizedPosition - targetPos) < .01f)
                {
                    //Debug.LogError("BREAKING OUT OF COROUTINE. POSITION REACHED");
                    scrollRect.verticalNormalizedPosition = targetPos;
                    break;
                }
                yield return 0f;
            }

            scrollRect.velocity = Vector2.zero;
        }

        float GetTargetPos(int index)
        {
            //this return to what position scrollRect should move to center on given index.
            //if new boosters are added this positions have to be upgraded. 
            //You can get them by opening Boosters Pop Up in play mode, centering on each booster and writing down SCROLL BAR 'Value'

            switch (index)
            {
                case 0:
                    return 1f;
                case 1:
                    return 1f;
                case 2:
                    return 0.72f;
                case 3:
                    return 0.57f;
                case 4:
                    return 0.40f;
                case 5:
                    return 0.23f;
                case 6:
                    return 0.10f;
                case 7:
                    return 0f;
                case 8:
                    return 0f;
            }

            return 1f;
        }
    }
}
