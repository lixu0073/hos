using UnityEngine;
using System.Collections;
using SimpleUI;
using CodeStage.AntiCheat.ObscuredTypes;
using System;

namespace Hospital
{
    public class RatePopUp : UIElement
    {
        public GameObject never;

        ResourceRequest characterResourceRequest;
        GameObject characters;

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            UIController.get.ExitAllPopUps();
            yield return base.Open(true, false);
            StartCoroutine(LoadCharacters());
            SetButtons();
            ObscuredPrefs.SetInt("LastRateLevel", Game.Instance.gameState().GetHospitalLevel());
            whenDone?.Invoke();
        }

        void SetButtons()
        {
            if (Game.Instance.gameState().GetHospitalLevel() <= 7)
            {
                never.SetActive(false);
            }
            else
            {
                never.SetActive(true);
            }
        }

        IEnumerator LoadCharacters()
        {
            characterResourceRequest = Resources.LoadAsync("RateCharacters", typeof(GameObject));

            while (!characterResourceRequest.isDone)
            {
                yield return 0;
            }

            characters = Instantiate(characterResourceRequest.asset, transform) as GameObject;
            characters.transform.SetAsFirstSibling();
            characters.transform.localScale = Vector3.one;
            RectTransform rt = characters.GetComponent<RectTransform>();
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        void UnloadCharacters()
        {
            if (characters != null)
                Destroy(characters);

            Resources.UnloadAsset(characters);
            Resources.UnloadUnusedAssets();
            //System.GC.Collect();
        }

        protected override void DisableOnEnd()
        {
            base.DisableOnEnd();
            if (!IsVisible && !IsAnimating)
                UnloadCharacters();
        }

        public void ButtonExit()
        {
            Exit();
        }

        public void ButtonRate()
        {
            ObscuredPrefs.SetBool("Rated", true);
#if UNITY_ANDROID
            Application.OpenURL("https://play.google.com/store/apps/details?id=com.cherrypickgames.myhospital");
#endif
            Invoke("ButtonExit", 1f);
        }

        public void ButtonLater()
        {
            Exit();
        }

        public void ButtonNever()
        {
            ObscuredPrefs.SetBool("Rated", true);
            Exit();
        }

        public static bool ShouldShowRate(bool levelRestriction)
        {
            //we have resigned from in-game rate in favor of deltaDNA rate campaigns.
            return false;
            /*
            //ObscuredPrefs.SetBool("Rated", false);
            //ObscuredPrefs.SetInt("LastRateLevel", 0);
#if UNITY_IPHONE
            return false;
#endif
            //dont show rate if user rated already or chose 'never'
            if (ObscuredPrefs.GetBool("Rated"))
                return false;

            //dont show rate if it was shown less than 2 levels from the time user chose "later" / "not now"
            if (levelRestriction && ObscuredPrefs.GetInt("LastRateLevel") + 2 >= Game.Instance.gameState().GetHospitalLevel())
                return false;

            //prevent showing Rate each time after reloading game on level 8 after vip tutorial
            if (!levelRestriction && ObscuredPrefs.GetInt("LastRateLevel") == Game.Instance.gameState().GetHospitalLevel())
                return false;

            //show rate when it's regardless of level restriction, i.e. forced after certain tutorial step or action
                return true;
            */
        }
    }
}
