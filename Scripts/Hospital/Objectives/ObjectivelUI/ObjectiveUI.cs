using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ObjectiveUI : MonoBehaviour 
{
#pragma warning disable 0649
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] Animator progressAnim;
    [SerializeField] Animator rewardAnim;
    [SerializeField] Image image;
#pragma warning restore 0649
    Objective objective;

    //bool playZip = true;

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void Setup(Objective objective)
    {
        if (objective != this.objective)
        {
            try
            { 
                rewardAnim.Play("Lvl_goal_inactive", 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }
        this.objective = objective;

        image.sprite = this.objective.Reward.GetSprite();
        description.text = objective.GetDescription();
        UpdateProgress();
    }

    public void UpdateProgress()
    {
        if (objective == null)
        {
            Debug.LogError("objective == null in ObjectiveUI");
            return;
        }
        CoroutineInvoker.Instance.StartCoroutine(UpdateProgressCoroutine());
    }

    IEnumerator UpdateProgressCoroutine()
    {
        progressText.text = "";
        
        progressText.text = objective.GetProgressStringUI();

        bool progressChanged = false;
        if (objective.ProgressUI != objective.ProgressObjective)
        {
            yield return new WaitForSeconds(.5f);
            objective.RefreshProgressUI();
            if (progressText.text != objective.GetProgressStringUI() && progressAnim != null)
            {
                progressAnim.SetTrigger("Bounce");
                yield return new WaitForSeconds(.2f);
                progressText.text = objective.GetProgressStringUI();
                progressChanged = true;
            }
        }

        if (objective.GetProgressFloat() >= 1)
        {
            if (objective.isRewardClaimed)
            {
                try
                { 
                    if (!rewardAnim.GetCurrentAnimatorStateInfo(0).IsName("Lvl_goal_completed"))
                    {
                        rewardAnim.Play("Lvl_goal_completed", 0, 0.0f);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }
            else
            {
                if (progressChanged)
                {
                    yield return new WaitForSeconds(.5f);
                }
                if (IsRewardClaimed()) //when you finish a goal and level up at the sime time the reward can be claimed in the last .5 sec
                {
                    yield break;
                }
                try
                { 
                    if (rewardAnim != null && 
                        !rewardAnim.GetCurrentAnimatorStateInfo(0).IsName("ShowReward") && 
                        !rewardAnim.GetCurrentAnimatorStateInfo(0).IsName("RewardIdle"))
                    {
                        try
                        { 
                            rewardAnim.Play("ShowReward", 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }

                        if (!objective.rewardShown)
                        {
						    objective.rewardShown = true;
						    SoundsController.Instance.PlayZip();
					    }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }
        }
    }

    public bool IsRewardClaimed()
    {
        if(objective != null)
            return objective.isRewardClaimed;
        return true;
    }

    public bool IsCompleted()
    {
        if (objective != null)
            return objective.GetProgressFloat() >= 1;
        return false;
    }

    public void ButtonReward()
    {
        if (IsRewardClaimed())
            return;

        Canvas canvas = UIController.get.canvas;
        try
        { 
            Vector2 startPoint = new Vector2((rewardAnim.transform.position.x - Screen.width / 2) / canvas.transform.localScale.x, (rewardAnim.transform.position.y - Screen.height / 2) / canvas.transform.localScale.y);
            objective.CollectReward(startPoint);

            rewardAnim.Play("CollectReward", 0, 0.0f);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Animator - exception: " + e.Message);
        }

        if (ReferenceHolder.Get().objectiveController.IsDynamicObjective())
        {
            if (ReferenceHolder.Get().objectiveController.ObjectivesCompletedAndClaimed)
                UIController.getHospital.ObjectivesPanelUI.SlideOutWithCoroutine();
        }
    }

    public void CollectReward(bool delayed = true)
    {
        if (IsRewardClaimed())
            return;

        int offset = 75;

        Canvas canvas = UIController.get.canvas;
        try
        { 
            Vector2 startPoint = new Vector2((rewardAnim.transform.position.x - Screen.width / 2 ) / canvas.transform.localScale.x + (delayed?0:offset), (rewardAnim.transform.position.y - Screen.height / 2) / canvas.transform.localScale.y);
            objective.CollectReward(startPoint, delayed);

            rewardAnim.Play("CollectReward", 0, 0.0f);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Animator - exception: " + e.Message);
        }

        if (ReferenceHolder.Get().objectiveController.IsDynamicObjective() && delayed)
        {
            if (ReferenceHolder.Get().objectiveController.ObjectivesCompletedAndClaimed)
                UIController.getHospital.ObjectivesPanelUI.SlideOutWithCoroutine();
        }
    }
}
