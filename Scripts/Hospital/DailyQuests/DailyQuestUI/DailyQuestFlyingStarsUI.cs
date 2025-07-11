using UnityEngine;
using System;

public class DailyQuestFlyingStarsUI : MonoBehaviour 
{
#pragma warning disable 0649
    [SerializeField] FlyingStar[] stars;
	[SerializeField] ParticleSystem[] starParticles;
#pragma warning restore 0649

    void Awake()
    {
        for (int i = 0; i < stars.Length; ++i)
            stars[i].gameObject.SetActive(false);
    }

    public void FlyStars(int amount, int day)
    {
        for (int i = 0; i < amount; ++i)
        {
            FlyStar(i, day, i+1 == amount);
        }
    }

    void FlyStar(int starIndex, int day, bool showReward)
    {
        FlyingStar star = GetStarFromPool();

        Vector2 startPos = UIController.getHospital.DailyQuestPopUpUI.taskList[starIndex].GetStarPosition();
        Vector2 endPos = UIController.getHospital.DailyQuestPopUpUI.questList[day].GetStarPosition(starIndex);
        Vector2 startSize = UIController.getHospital.DailyQuestPopUpUI.taskList[starIndex].GetStarSize();
        Vector2 endSize = UIController.getHospital.DailyQuestPopUpUI.questList[day].GetStarSize(starIndex);
        float delay = starIndex * 0.5f;

        star.Init(day, startPos, endPos, startSize, endSize, delay, () =>
        {
            //dailyQuestPopUpUI.taskList[starIndex].HideStar();
        }, () =>
        {
            UIController.getHospital.DailyQuestPopUpUI.questList[day].ShowStar(starIndex, showReward);
            starParticles[starIndex].gameObject.transform.position = UIController.getHospital.DailyQuestPopUpUI.questList[day].GetStarPosition(starIndex);
            try
            { 
			    starParticles[starIndex].Play();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
            SoundsController.Instance.PlayAnySound(71 + starIndex);
        });
    }

    FlyingStar GetStarFromPool()
    {
        for (int i = 0; i < stars.Length; ++i)
        {
            if (!stars[i].isInUse)
                return stars[i];
        }

        Debug.LogError("Star pool is too small. Call Mikko"); // CV: who you gonna call?
        return stars[0];
    }

    Vector3 GetEndPosition(int day, int starIndex)
    {
        return transform.position;
    }
}
