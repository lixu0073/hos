using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Hospital;

public class TutorialProgressChecker
{
    private static TutorialProgressChecker instance;
    private TutorialProgressChecker()
    {

    }

    private Dictionary<int, StepTag> levelToSteptagDictionary = new Dictionary<int, StepTag>()
        {
            {2,StepTag.newspaper_1 },
            {3,StepTag.level_3 },
            {4,StepTag.level_4 },
            {5,StepTag.level_5 },
            {6,StepTag.level_6 },
            {7,StepTag.level_7 },
            {8,StepTag.level_8 },
            {9,StepTag.level_9},
            {10,StepTag.level_10 },
            {12,StepTag.level_12 },
            {14,StepTag.level_14 },
            {15,StepTag.level_15 },
            {16,StepTag.level_16 },
            {17,StepTag.level_17 },
            {18,StepTag.level_18 },
            {23,StepTag.level_23 },
        };

    /// <summary>
    /// These are just to set the trigger on the next level. 
    /// Some of these wont trigger before the level is reached since the tutorials require correct level to activate.
    /// </summary>
    private Dictionary<int, StepTag> secondaryLevelToSteptagDictionary = new Dictionary<int, StepTag>()
    {
        {0,StepTag.None},
        {1,StepTag.None},
        {11,StepTag.level_12 },
        {13,StepTag.level_14 },
        {19,StepTag.level_19 },
        {20,StepTag.level_20 },
        {21,StepTag.level_23 },
        {22,StepTag.level_23 },
    };

    public static TutorialProgressChecker GetInstance()
    {
        if (instance == null)
        {
            instance = new TutorialProgressChecker();
        }
        return instance;
    }

    public bool IsTutorialMatchLevel(int levelReached)
    {
        StepTag TutorialTagCorrespondngToReachedLevel;
        if (levelToSteptagDictionary.TryGetValue(levelReached, out TutorialTagCorrespondngToReachedLevel))
        {
            if (TutorialTagCorrespondngToReachedLevel != TutorialController.Instance.CurrentTutorialStepTag)
            {
                return false;
            }
        }
        return true;
    }

    public void MatchTutorialStepToLevel(int levelTomatch)
    {
        TutorialController tc = TutorialController.Instance;

        if (levelTomatch > 6 && !tc.IsTutorialStepCompleted(StepTag.package_collected))
        {
            ((HospitalCasesManager)AreaMapController.Map.casesManager).StartCounting();
        }


        StepTag TutorialTagCorrespondngToReachedLevel;
        if (levelToSteptagDictionary.TryGetValue(levelTomatch, out TutorialTagCorrespondngToReachedLevel))
        {
            ReferenceHolder.Get().engine.MainCamera.StopFollowing();

            TutorialSystem.TutorialController.SkipTo(TutorialTagCorrespondngToReachedLevel);

        }
        else if (secondaryLevelToSteptagDictionary.TryGetValue(levelTomatch, out TutorialTagCorrespondngToReachedLevel))
            TutorialSystem.TutorialController.SkipTo(TutorialTagCorrespondngToReachedLevel);

        ReferenceHolder.Get().engine.MainCamera.StopFollowing();
    }
    public StepTag GetTutorialStepToLevel(int levelTomatch)
    {

        StepTag TutorialTagCorrespondngToReachedLevel;
        Debug.Log("GetTutorialStepToLevel " + levelTomatch);
        //This is override since some levels break when trying to reload the tutorial step
        switch (levelTomatch)
        {
            case 2:
                return StepTag.level_3;
            case 6:
                return StepTag.level_7;
            case 20: //fix Cloudy lungs newspaper gem cheat bug
                return StepTag.level_23;
        }

        if (levelToSteptagDictionary.TryGetValue(levelTomatch, out TutorialTagCorrespondngToReachedLevel))
        {
            return TutorialTagCorrespondngToReachedLevel;
        }
        if (secondaryLevelToSteptagDictionary.TryGetValue(levelTomatch, out TutorialTagCorrespondngToReachedLevel))
        {
            return TutorialTagCorrespondngToReachedLevel;
        }

        if (TutorialSystem.TutorialModule.Controller.finalStepTag != StepTag.None)
        {
            Debug.Log("No tutorial level match. Returning final step tag");
            return TutorialSystem.TutorialModule.Controller.finalStepTag;
        }
        else
        {
            Debug.LogError("No tutorial level match. No final step tag.");
            return StepTag.SKIP_TUTORIALS_TO_THIS_STEP;
        }
    }

    public void CheckHardSkipOnExp(int level, int exp)
    {
        if (level == 6 && exp >= 125)
        {
            if (TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.emma_about_wise))
            {
                Debug.LogError("Skipping tutorial steps about Wise because player did not visit him for 125 xp... All is good.");
                TutorialController.Instance.SetStep(StepTag.package_emma);
                TutorialUIController.Instance.BlinkFriendsButton(false);
            }
        }

        /*
        int expRequired = 120;
        if (level == 7 && exp >= expRequired)
        {
            if (TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.treasure_first_spawn))
            {
                Debug.LogError("Skipping tutorial steps because player did not complete Pharmacy tutorial and we need to show Treasures... All is good.");
                TutorialController.Instance.SetStep(StepTag.treasure_first_spawn);
            }
        }

        expRequired = 180;
        if (level == 7 && exp >= expRequired) {
            if (TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.daily_quests_unlocked)) {
                Debug.LogError("Skipping tutorial steps because player did not complete Treasures tutorial and we need to show Daily Quests... All is good.");
                TutorialController.Instance.SetStep(StepTag.daily_quests_unlocked);
            }
        }
        */

        if (level == 18 && exp >= 150)
        {
            if (TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.bacteria_newspaper))
            {
                Debug.LogError("Skipping tutorial steps because player did not complete Epidemy tutorial and we need to show Bacteria... All is good.");
                TutorialController.Instance.SetStep(StepTag.bacteria_newspaper);
            }
        }
    }
}
