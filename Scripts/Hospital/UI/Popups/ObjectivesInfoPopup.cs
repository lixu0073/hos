using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using SimpleUI;
using MovementEffects;
using System;

namespace Hospital
{
	public class ObjectivesInfoPopup : UIElement
	{
#pragma warning disable 0649
        [SerializeField] GameObject objectivesInfoPrefab;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI rewardText;
        [SerializeField] Transform content;
        [SerializeField] ScrollRect scrollRect;
#pragma warning restore 0649

        private IEnumerator<float> scrollCoroutine;

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            StopScrollCoroutine();

            foreach (Transform child in content)
                GameObject.Destroy(child.gameObject);

            yield return null; // CV: to force waiting one frame before rendering the TMPro

            List<Objective> objectives = ReferenceHolder.Get().objectiveController.GetAllObjectives();
            rewardText.text = ReferenceHolder.Get().objectiveController.ObjectivesListReward.ToString();

            GameObject info;

            int notFinishedIndex = -1;

            if (objectives != null && objectives.Count > 0)
            {
                for (int i = 0; i < objectives.Count; i++)
                {
                    info = Instantiate(objectivesInfoPrefab, transform.position, Quaternion.identity);
                    info.transform.SetParent(content);
                    info.transform.localScale = new Vector3(1, 1, 1);
                    info.GetComponent<ObjectiveInfoUI>().Setup(objectives[i]);

                    if (!objectives[i].IsCompleted && notFinishedIndex == -1)
                        notFinishedIndex = i;
                }

                if (objectives.Count > 3)
                    scrollCoroutine = Timing.RunCoroutine(CenterToItemCoroutine(notFinishedIndex, objectives.Count));
                else scrollCoroutine = Timing.RunCoroutine(CenterToItemCoroutine(0, objectives.Count));
            }

            yield return base.Open(true, false);
            title.text = string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_LIST_TITLE"), ReferenceHolder.Get().objectiveController.ToDoCounter);

            whenDone?.Invoke();
        }

        IEnumerator<float> CenterToItemCoroutine(int objectiveIndex, int size)
        {
            if (objectiveIndex == -1)
                objectiveIndex = 0;

            float targetPos = 1 - ((float)objectiveIndex / size);
            targetPos = Mathf.Clamp(targetPos, 0f, 1f);
            float timer = 0f;

            if (scrollRect.verticalNormalizedPosition == targetPos)
                yield break;

            while (true)
            {
                timer += Time.deltaTime;

                scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, targetPos, .1f);

                if (timer > 5f || Mathf.Abs(scrollRect.verticalNormalizedPosition - targetPos) < .001f)
                {
                    scrollRect.verticalNormalizedPosition = targetPos;
                    break;
                }
                yield return 0f;
            }

            scrollRect.velocity = Vector2.zero;
        }

        public void StopScrollCoroutine()
        {
            if (scrollCoroutine != null)
            {
                Timing.KillCoroutine(scrollCoroutine.GetType());
                scrollCoroutine = null;
            }
        }

        public void ButtonExit()
        {
            StopScrollCoroutine();
            Exit();
        }

    }
}
