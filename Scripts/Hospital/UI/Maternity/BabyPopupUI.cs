using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Maternity.UI
{
    public class BabyPopupUI : MonoBehaviour
    {
        [SerializeField]
        private PatientAvatarUI babyAvatar = null;

        [SerializeField]
        private Image popupBackground = null;
        [SerializeField]
        private Image[] balloons = null;

        [SerializeField]
        private TextMeshProUGUI babyInfo = null;

        public void SetBabyPopupActive(bool setActive)
        {
            gameObject.SetActive(setActive);
        }
        
        public void SetBabyView(Sprite head, Sprite body, PatientAvatarUI.PatientBackgroundType backgroundType)
        {
            SetBabyAvatar(head, body, backgroundType);
            SetPopupBackground(backgroundType);
            SetBabyInfo(I2.Loc.ScriptLocalization.Get(backgroundType == PatientAvatarUI.PatientBackgroundType.boyBaby ? "MATERNITY_ITS_A_BOY" : "MATERNITY_ITS_A_GIRL"));
        }

        public void SetBabyAvatar(Sprite head, Sprite body, PatientAvatarUI.PatientBackgroundType backgroundType)
        {
            babyAvatar.SetAvatarView(head, body, backgroundType);
        }

        public void SetPopupBackground(PatientAvatarUI.PatientBackgroundType backgroundType)
        {
            if (backgroundType == PatientAvatarUI.PatientBackgroundType.boyBaby)
            {
                popupBackground.sprite = ResourcesHolder.GetMaternity().popupBoyBg;
            }

            if (backgroundType == PatientAvatarUI.PatientBackgroundType.girlBaby)
            {
                popupBackground.sprite = ResourcesHolder.GetMaternity().popupGirlBg;
            }
        }

        public void SetBalloons(PatientAvatarUI.PatientBackgroundType backgroundType)
        {
            if (backgroundType == PatientAvatarUI.PatientBackgroundType.boyBaby)
            {
                for (int i = 0; i < balloons.Length; ++i)
                {
                    SetBalloon(balloons[i], ResourcesHolder.GetMaternity().boyBabyBallon);
                }
                return;
            }

            if (backgroundType == PatientAvatarUI.PatientBackgroundType.girlBaby)
            {
                for (int i = 0; i < balloons.Length; ++i)
                {
                    SetBalloon(balloons[i], ResourcesHolder.GetMaternity().girlBabyBallon);
                }
            }
        }

        private void SetBalloon(Image ballonImage, Sprite ballonSprite)
        {
            ballonImage.sprite = ballonSprite;
        }

        public void SetBabyInfo(string babyInfoText)
        {
            babyInfo.text = babyInfoText;
        }

        public void HideIt()
        {
            SetBabyPopupActive(false);
        }

    }
}
