using UnityEngine;
using System.Collections.Generic;
using SimpleUI;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using MovementEffects;
using System.Collections;
using System;

namespace Hospital
{
	public class AchievementPopUpController : UIElement
    {
        bool populated = false;
        public UIAchievement achievementPrefab;
        public Transform achievementContainer;
        public AchievementDatabase achievementDatabase;
#pragma warning disable 0649
        [SerializeField] private ScrollRect scrollRect;
#pragma warning restore 0649
        public Dictionary<string, Achievement> AchievementInfoList = new Dictionary<string, Achievement>();
        public Dictionary<string, UIAchievement> UIAchievementList = new Dictionary<string, UIAchievement>();

        public AchievementController ac = new AchievementController();

		[SerializeField] private TextMeshProUGUI achievementsCounter = null;
        
        [Header("Device specific layout refs")]
        public RectTransform popUpRect;
        public RectTransform content;

        [Header("Platform specific")]
        public GameObject socialAchievements;


        void Awake()
        {
            SetDeviceLayout();
            SetPlatformSpecific();
        }
        
        void SetDeviceLayout()
        {
            if (ExtendedCanvasScaler.isPhone() && !ExtendedCanvasScaler.HasNotch())
            //if (true)
            {
                popUpRect.sizeDelta = new Vector3(720, 460);
                popUpRect.anchoredPosition = new Vector3(0, -15);
                content.localScale = new Vector3(1.4f, 1.4f, 1f);
            }
            else
            {
                popUpRect.sizeDelta = new Vector3(500, 350);
                popUpRect.anchoredPosition = new Vector3(0, -20);
                content.localScale = Vector3.one;
            }
        }

        void SetPlatformSpecific()
        {
#if UNITY_ANDROID
            socialAchievements.SetActive(true);
#else
            socialAchievements.SetActive(false);
#endif
        }

        public void StartAchievements()
        {
            //Debug.Log("Osiagniecia Odpalone");
            if (!populated)
            {
                AchievementNotificationCenter.UnsuscribeAllNotification();
                Initialize(ac.initializeAchievementList(achievementDatabase));
            }
        }

        public IEnumerator Open(Action whenDone = null)
        {
            gameObject.SetActive(true);
            StartCoroutine(base.Open(true, false, () =>
            {
                ac.achievementChecked = true;
			    ac.UpdateAchievements ();
			    UIController.getHospital.achievementIndicator.Activate(false);
			    achievementsCounter.text = I2.Loc.ScriptLocalization.Get("ACHIEVEMENTS_COMPLETED") + " " + GameState.Get ().achievementsDone + "/" + (achievementDatabase.AchievementItem.Count * 3);
                Timing.RunCoroutine(CenterToItemCoroutine(GetClaimableAchievementIndex()));
            }));
            yield return null;
            whenDone?.Invoke();
        }

        int GetClaimableAchievementIndex()
        {
            int index = -1;
            string str = UIAchievementList.FirstOrDefault((a) => a.Value.collectButton.isActiveAndEnabled).Key;
            if (str != null)
                index = UIAchievementList[str].transform.GetSiblingIndex();

            //Debug.LogError("Found (or not) claimable achievement child index = " + index);
            return index;
        }
        
        public void UpdateByID(string id)
        {
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
                return;

            UIAchievementList[id].Set(AchievementInfoList[id]);
        }
        
        public void Initialize(List<Achievement> AchievementList)
        {
            UIAchievement achiev;
            int count = AchievementList.Count;

            for (int i = 0; i < count; ++i)
            {
                achiev = Instantiate(achievementPrefab, transform.position, Quaternion.identity) as UIAchievement;
                achiev.transform.SetParent(achievementContainer);
                achiev.transform.localScale = new Vector3(1, 1, 1);
                achiev.Set(AchievementList[i]);
                
                if (!UIAchievementList.ContainsKey(AchievementList[i].id))
                    UIAchievementList.Add(AchievementList[i].id, achiev);
                if (!AchievementInfoList.ContainsKey(AchievementList[i].id))
                    AchievementInfoList.Add(AchievementList[i].id, AchievementList[i]);
            }
            populated = true;
        }

        public void UpdateTranslation()
        {
            for (int i = 0; i < achievementDatabase.AchievementItem.Count; ++i)
            {
                UpdateByID(achievementDatabase.AchievementItem[i].achievementID);
            }
        }

		IEnumerator<float> CenterToItemCoroutine(int achievementIndex)
		{
            if (achievementIndex == -1)
                yield break;
            
            float targetPos = 1 - ((float)achievementIndex / (AchievementInfoList.Count - 2));      //-2 so the target achievement is not on the bottom of the list after scrolling
            targetPos = Mathf.Clamp(targetPos, 0f, 1f);
            float timer = 0f;

            if (scrollRect.verticalNormalizedPosition == targetPos)
                yield break;

            //Debug.LogError("Will scroll to pos: " + targetPos);
			while (true)
			{
                timer += Time.deltaTime;

                scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, targetPos, .1f);

                //Debug.LogError("t = " + t + " vertpos = " + scrollRect.verticalNormalizedPosition);
                if (timer > 5f || Mathf.Abs(scrollRect.verticalNormalizedPosition - targetPos) < .001f)
                {
                    //Debug.LogError("BREAKING OUT OF COROUTINE. POSITION REACHED");
                    scrollRect.verticalNormalizedPosition = targetPos;
                    break;
                }
                yield return 0f;
			}

            scrollRect.velocity = Vector2.zero;
        }

        public void BreakScrollCoroutine()      //event trigger OnPointerDown
        {
            Timing.KillCoroutine(CenterToItemCoroutine(0).GetType());
        }

        public void ButtonExit()
        {
            Exit();
        }

        public void ButtonSocialAchievements()
        {
#if UNITY_ANDROID
            try
            {
                GPGSController.Instance.ShowAchievements();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
#endif
        }

        public void Destroy()
        {
            foreach (var achievement in ac.AchievementList)
            {
                achievement.RemoveListener();
            }
        }
    }
}
