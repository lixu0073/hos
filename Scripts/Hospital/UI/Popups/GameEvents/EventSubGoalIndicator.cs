using System;
using UnityEngine;
using UnityEngine.UI;

public class EventSubGoalIndicator : MonoBehaviour 
{
#pragma warning disable 0649
    [SerializeField] RectTransform rect;
    [SerializeField] GameObject globe;
    [SerializeField] GameObject questionMark;
    [SerializeField] Image line;
    [SerializeField] Animator anim;
#pragma warning restore 0649
    public Image rewardIcon;
    public bool isUnlocked;
    public int goal;
    public float unlockRequirements;


    public void Setup(float unlockRequirements, bool unlocksGlobal, bool isUnlocked, Sprite rewardSprite, float positionX, bool hasLine, int goal, bool isFilling)
    {
        this.unlockRequirements = unlockRequirements;
        this.isUnlocked = isUnlocked;
        this.goal = goal;

        globe.SetActive(unlocksGlobal);
        questionMark.SetActive(!isUnlocked);
        rewardIcon.gameObject.SetActive(isUnlocked);
        rewardIcon.sprite = rewardSprite;
        rect.anchoredPosition = new Vector2(positionX, rect.anchoredPosition.y);

        line.enabled = hasLine;

        if (isUnlocked)
        {
            SetUnlocked(isFilling);
        }
        else if (!anim.GetCurrentAnimatorStateInfo(0).IsName("SubGoal_Idle"))
        {
            try
            { 
                anim.Play("SubGoal_Idle", 0, 0.0f);
            }
            catch (Exception e){
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }

        gameObject.SetActive(true);
    }
    
    void SetUnlocked(bool justUnlocked)
    {
        isUnlocked = true;
        rewardIcon.gameObject.SetActive(isUnlocked);

        if(justUnlocked && !anim.GetCurrentAnimatorStateInfo(0).IsName("SubGoal_Reward_Idle"))
            anim.SetTrigger("Reward");
        else
            anim.SetTrigger("Reward_Idle");
    }
}