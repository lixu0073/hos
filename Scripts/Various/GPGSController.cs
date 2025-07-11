using UnityEngine;
using Hospital;
using SimpleUI;
using System.Collections.Generic;
#if UNITY_ANDROID
using GooglePlayGames;
#endif

public class GPGSController : MonoBehaviour
{
    public static GPGSController Instance;

    public delegate void OnUpdate();
    public static event OnUpdate OnGPGStateUpdate;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
#if UNITY_ANDROID
            if (PlayGamesPlatform.Instance.IsAuthenticated()) // CV: test
            {
                Debug.LogError("GPGSController Already instanced: " + PlayGamesPlatform.Instance.GetUserDisplayName());
            }
#endif
            return;
        }
#if UNITY_ANDROID        
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
#endif
    }

    public void SignInAfterIntro()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Social.localUser.authenticated && PlayerPrefs.GetInt("GPG_LOGIN") > 0)
            SignIn();
#endif
    }

    public void Authenticated(bool success)
    {
        OnGPGStateUpdate?.Invoke();

        if (success)
        {
            PlayerPrefs.SetInt("GPG_LOGIN", 1);
            FirstConnectToGPG(); // CV: reward process started
        }
        else
        {
            Debug.Log("You've failed to log in");
#if UNITY_ANDROID && !UNITY_EDITOR
            UnityAndroidExtras.instance.makeToast("Failed to sign in. Please check your network connection and try again", 1);
#endif
            PlayerPrefs.SetInt("GPG_LOGIN", 0);
        }
    }

    void SignIn()
    {
#if UNITY_ANDROID
        Social.localUser.Authenticate((bool success) => {

            OnGPGStateUpdate?.Invoke();

            if (success)
            {
                PlayerPrefs.SetInt("GPG_LOGIN", 1);

                FirstConnectToGPG(); // CV: reward process started
            }
            else
            {
                Debug.Log("You've failed to log in");
#if UNITY_ANDROID && !UNITY_EDITOR
                UnityAndroidExtras.instance.makeToast("Failed to sign in. Please check your network connection and try again", 1);
#endif
                PlayerPrefs.SetInt("GPG_LOGIN", 0);
            }
        });
#endif
    }

    public void SignOut()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.SignOut();

        PlayerPrefs.SetInt("GPG_LOGIN", 0);

        OnGPGStateUpdate?.Invoke();
#endif
    }

    public void ShowAchievements()
    {
#if UNITY_ANDROID
        if (Social.localUser.authenticated)
        {
            ((PlayGamesPlatform)Social.Active).ShowAchievementsUI();
        }
        else
        {
            Debug.LogError("GPGSController::ShowAchievement - Social localUser NOT Authenticated - " + Social.localUser);
            try
            {
                if (Social.localUser == null)
                    Debug.LogError("-- - - - - - - -- Social.localUser is NULL!");
                else 
                    Social.localUser.Authenticate((bool success) =>
                    {
                        OnGPGStateUpdate?.Invoke();
                        if (success)
                        {
                            Debug.LogError("GPGSController::ShowAchievement - Authenticate Success");
                            PlayerPrefs.SetInt("GPG_LOGIN", 1);
                            ((PlayGamesPlatform)Social.Active).ShowAchievementsUI();
                            Social.ShowAchievementsUI();
                        }
                        else
                        {
                            Debug.LogError("GPGSController::ShowAchievement - Authenticate FAILED!!");
    #if !UNITY_EDITOR
                            PlayerPrefs.SetInt("GPG_LOGIN", 0);
                            UnityAndroidExtras.instance.makeToast("Failed to sign in. Please check your network connection and try again", 1);
    #endif
                        }
                    });
            }
            catch (System.Exception ex)
            {
                Debug.LogError("ShowAchievements EXCEPTION: " + ex.Message);
            }
        }
#endif
    }

    public void ToggleStatus()
    {
#if UNITY_ANDROID
        if (Social.localUser.authenticated)
            SignOut();
        else
            SignIn();
#endif
    }

    // CV -------------------------------------------------------------------------------------------------------------------------
    private void FirstConnectToGPG()
    {        
        OnGPGConnected();
    }

    public void OnGPGConnected()
    {
        Game.Instance.gameState().SetFBConnectionRewardEnabled(true);
        Game.Instance.gameState().SetEverLoggedInFB(true);
        TryToShowGPGConnectReward();
    }

    public void TryToShowGPGConnectReward()
    {
        Debug.LogError("<color=red>:> :>  TryToShowGPGConnectReward: </color>" + "<color=yellow>" + Game.Instance.gameState().IsFBRewardConnectionClaimed() +"</color>");
        if (Game.Instance.gameState().IsFBConnectionRewardEnabled() && !Game.Instance.gameState().IsFBRewardConnectionClaimed() && !IsGPGRewardClaimed())
        {
            Game.Instance.gameState().SetEverLoggedInFB(true);
            SaveSynchronizer.Instance.InstantSave();
            ShowGPGConnectRewardCoroutine();
        }
    }

    private void ShowGPGConnectRewardCoroutine()
    {
        MovementEffects.Timing.KillCoroutine(ShowGPGConnectReward().GetType());
        MovementEffects.Timing.RunCoroutine(ShowGPGConnectReward());
    }

    private IEnumerator<float> ShowGPGConnectReward()
    {
        int diamondReward = 5;

        UIController.get.ExitAllPopUps();
        UIController.get.CloseActiveHover();
        if (UIController.get.FriendsDrawer.IsVisible)
            UIController.get.FriendsDrawer.SetVisible(false);

        yield return MovementEffects.Timing.WaitForSeconds(0.1f);

        while (TutorialController.Instance.tutorialEnabled && TutorialUIController.Instance.IsFullscreenActive())
            yield return 0;

        Vector3 startPoint = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, ReferenceHolder.Get().engine.MainCamera.GetCamera().nearClipPlane));

        Debug.LogError("<color=#001010ff>   -> PRE ->  ShowGPGConnectReward: </color>" + "<color=yellow>" + Game.Instance.gameState().GetDiamondAmount() + "</color>  diamonds");

        int currentAmount = Game.Instance.gameState().GetDiamondAmount();
        GameState.Get().AddResource(ResourceType.Diamonds, diamondReward, EconomySource.FacebookConnected, false);
        Game.Instance.gameState().SetFBRewardConnectionClaimed(true); // Let's keep FBReward flag instead of creating a new GPG one
        SetGPGRewardClaimed(); // To avoid MHP-303
        SaveSynchronizer.Instance.InstantSave();
        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Diamond, startPoint, diamondReward, 0f, 2f, new Vector3(1.2f, 1.2f, 1), new Vector3(0.9f, 0.9f, 1), ReferenceHolder.Get().giftSystem.particleSprites[1], null, () => {
            GameState.Get().UpdateCounter(ResourceType.Diamonds, diamondReward, currentAmount);
        });

        Debug.LogError("<color=#001010ff>   -> POST ->  ShowGPGConnectReward: </color>" + "<color=yellow>" + Game.Instance.gameState().GetDiamondAmount() + "</color>  diamonds");

        // UIController.get.unboxingPopUp.OpenFacebookRewardCasePopup();
    }

    private void SetGPGRewardClaimed(bool value = true)
    {        
        PlayerPrefs.SetInt("GPGRewardClaimed", value ? 1 : 0);
    }

    private bool IsGPGRewardClaimed()
    {
        return PlayerPrefs.GetInt("GPGRewardClaimed") == 1;
    }

}
