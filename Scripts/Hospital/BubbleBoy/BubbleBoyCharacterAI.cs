using UnityEngine;
using System.Collections;
using MovementEffects;
using System.Collections.Generic;

namespace Hospital
{
    public class BubbleBoyCharacterAI : MonoBehaviour
    {
        public Animator animator;
        public BubbleBoyStatusIndicator statusIndicator;
        public GameObject character;

        public int MIN_ANIM_DURATION;
        public int MAX_ANIM_DURATION;

        private bool IsBubbling = false;

        [TutorialTriggerable]
        public void ShowPreInitialize()
        {
            if (!character.activeSelf)
                character.SetActive(true);

            Trigger("Idle");
            animator.enabled = false;
        }

        [TutorialTriggerable]
        public void Initialize()
        {
            BaseGameState.OnLevelUp -= Initialize;
            if (!TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.bubble_boy_available, true))
            {
                BaseGameState.OnLevelUp += Initialize;
                return;
            }

            if (!character.activeSelf)
                character.SetActive(true);

            animator.enabled = true;

            Trigger("StartGame");
        }

        public void Disable()
        {
            if (character.activeSelf)
                character.SetActive(false);
        }

        public void OnIdleAnimEnter()
        {
            ChooseState();
        }

        public void OnWalkFrontAnimExit()
        {
            if (!IsBubbling)
            {
                switch (GetRandomAnimIndex())
                {
                    case 1:
                        Trigger("Jump2");
                        break;
                    case 2:
                        Trigger("Sit2");
                        break;
                    case 3:
                        Trigger("Dance2");
                        break;
                }
                DelayAndGoIdle();
            }
        }

        public void OnMiniGameEnter()
        {
            Trigger("Idle");

            BubbleBoyFreeIndicatorNotVisible();
        }

        public void DelayAndGoIdle()
        {
            Timing.KillCoroutine(Delay().GetType());
            Timing.RunCoroutine(Delay());
        }

        private void BubbleBoyFreeIndicatorIsVisible()
        {
            statusIndicator.ShowFreeIndicator();
        }

        private void BubbleBoyFreeIndicatorNotVisible()
        {
            statusIndicator.HideFreeIndicator();
        }

        IEnumerator<float> Delay()
        {
            int durationInSeconds = UnityEngine.Random.Range(MIN_ANIM_DURATION, MAX_ANIM_DURATION + 1);
            yield return Timing.WaitForSeconds(durationInSeconds);
            Trigger("Idle");
        }

        private void Trigger(string trigger)
        {
            animator.SetTrigger(trigger);
        }

        private void ChooseState()
        {
            //Tutorial condition not met
            if (!TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.bubble_boy_intro, true))
            {
                return;
            }

            if (BubbleBoyDataSynchronizer.Instance.IsFreeEntryAvailable())
            {
                IsBubbling = true;
                Trigger("Blow3");
            }
            else
            {
                IsBubbling = false;
                if (BinaryRandom())
                {
                    Trigger(BinaryRandom() ? "Jump1" : "Dance1");
                    DelayAndGoIdle();
                }
                else
                {
                    Trigger("Go2");
                }
            }
        }

        private int GetRandomAnimIndex()
        {
            return UnityEngine.Random.Range(1, 4);
        }

        private bool BinaryRandom()
        {
            return UnityEngine.Random.Range(0, 2) == 1;
        }
    }
}
