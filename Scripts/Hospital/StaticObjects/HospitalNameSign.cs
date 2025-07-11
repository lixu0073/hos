using UnityEngine;
using TMPro;
using SimpleUI;

namespace Hospital
{
    public class HospitalNameSign : SuperObjectWithVisiting
    {
#pragma warning disable 0414
        [SerializeField] private GameObject newBadge = null;
#pragma warning restore 0414
        [SerializeField] private GameObject pole = null;

        public TextMeshPro nameOnSign;
        public ParticleSystem[] particles;
#pragma warning disable 0649
        [SerializeField] private GameObject SignIndicator;
#pragma warning restore 0649
        [HideInInspector]
        public bool SignIndicatorVisible;

        void Start()
        {
            if (GameState.Get().HospitalName == "")
            {
                GameState.Get().HospitalName = "My Hospital";
            }
            SetNameOnSign();
            BaseGameState.OnLevelUp += GameState_OnLevelUp;
        }

        private void GameState_OnLevelUp()
        {
            if (Game.Instance.gameState().GetHospitalLevel() == 8)
            {
                SetIndicatorVisible(true);
            }
        }

        public void SetNameOnSign()
        {
            SetNameOnSign(GameState.Get().HospitalName);

            if (!string.IsNullOrEmpty(GameState.Get().HospitalName))
                AnalyticsGeneralParameters.hospitalName = GameState.Get().HospitalName;
        }

        public void SetNameOnSign(string name)
        {
            nameOnSign.text = name;
        }

        public void SetPoleActive(bool active)
        {
            pole.SetActive(active);
        }

        public void FireSignChangeParticles()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].gameObject.SetActive(true);

            }
            Invoke("DisableParticles", 4f);
            SoundsController.Instance.PlayMagicPoof();
        }

        public void GiveExp(int amount, EconomySource source)
        {
            if (amount <= 0)
                return;
            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            GameState.Get().AddResource(ResourceType.Exp, amount, source, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, transform.position, amount, 0.5f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Exp, amount, currentExpAmount);
            });
        }

        void DisableParticles()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].gameObject.SetActive(false);
            }
        }

        public override void OnClick()
        {
            if (UIController.get.drawer.IsVisible)
            {
                UIController.get.drawer.SetVisible(false);
                return;
            }
            if (UIController.get.FriendsDrawer.IsVisible)
            {
                UIController.get.FriendsDrawer.SetVisible(false);
                return;
            }

            TutorialController tc = TutorialController.Instance;
            if (tc.tutorialEnabled && tc.CurrentTutorialStepIndex < tc.GetStepId(StepTag.name_hospital_close))
                return;

            //UIController.get.HospitalNamePopUp.Open();
            UIController.getHospital.hospitalSignPopup.Open();
        }

        public void SetIndicatorVisible(bool setVisible)
        {
            if (!visitingMode)
            {
                SignIndicatorVisible = setVisible;
                SignIndicator.SetActive(SignIndicatorVisible);
            }
            else
            {
                SignIndicator.SetActive(false);
            }
        }

        public override void IsoDestroy()
        {
            BaseGameState.OnLevelUp -= GameState_OnLevelUp;
        }
    }
}
