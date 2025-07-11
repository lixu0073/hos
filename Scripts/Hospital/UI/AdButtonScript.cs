using MovementEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hospital
{
    [RequireComponent(typeof(Button))]
    public class AdButtonScript : MonoBehaviour
    {
        private Button button;
        private Image buttonImage;
        private UnityEngine.Events.UnityAction onSuccess;
        private AdsController.AdType adType;

        private IEnumerator<float> restorationCoroutine;

        private void Start()
        {
            buttonImage = GetComponentInChildren<Image>();
            GetButton().onClick.AddListener(OnClickButton);
        }

        public void Initialize(UnityEngine.Events.UnityAction onSuccess, AdsController.AdType adType)
        {
            this.onSuccess = onSuccess;
            this.adType = adType;

            ForceFireRestorationCoroutine();
        }

        public void SetButtonEnable(bool setActive)
        {
            GetButton().gameObject.SetActive(TryDisplayAd() && setActive);
        }

        private void OnClickButton()
        {
            if (TryDisplayAd())
            {
                AdsController.instance.ShowAd(adType, new AdsController.OnAdSuccessLoad(onSuccess));
                DisableButton();
                ForceFireRestorationCoroutine();
            }
        }

        private bool TryDisplayAd()
        {
            return AdsController.instance.IsAdAvailable(adType);
        }

        private void DisableButton()
        {
            //buttonImage.material = ResourcesHolder.Get().GrayscaleMaterial;
            GetButton().gameObject.SetActive(false);
        }

        private void EnableButton()
        {
            //buttonImage.material = null;
            GetButton().gameObject.SetActive(true);
        }

        private void ForceFireRestorationCoroutine()
        {
            if (restorationCoroutine != null)
            {
                Timing.KillCoroutine(restorationCoroutine);
                restorationCoroutine = null;
            }
            restorationCoroutine = Timing.RunCoroutine(RestoreButtonCoroutine());
        }

        private IEnumerator<float> RestoreButtonCoroutine()
        {
            while (!TryDisplayAd())
            {
                yield return Timing.WaitForSeconds(1.0f);
            }

            EnableButton();
            Timing.KillCoroutine(restorationCoroutine);
            restorationCoroutine = null;
        }

        private Button GetButton()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            return button;
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                ForceFireRestorationCoroutine();
            }
        }
    }
}