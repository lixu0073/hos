using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using MovementEffects;

public class TutorialAnimation : MonoBehaviour {
    
    public TextMeshProUGUI TitleText;
    public GameObject CloseButton;
    public Transform AnimParent;
    
    [HideInInspector]
    public bool IsActive = false;

    Animator anim;
    GameObject spawnedGameObject;

    void Awake() {
        anim = GetComponent<Animator>();
    }

    public void ShowAnimation(TutorialAnimationClips clip) {
        IsActive = true;
        anim.ResetTrigger("Outro");
        anim.SetTrigger("Intro");
        CloseButton.SetActive(false);
        DestroyUsedAnimation();

        string animPath = "";
        switch (clip) {
            case (TutorialAnimationClips.BuildDoctor):
                SetTitle(I2.Loc.ScriptLocalization.Get("TUTORIAL/BUILD_DOCTOR_TITLE"));
                animPath = "TutorialAnimations/BuildDoctor";
                break;
            case (TutorialAnimationClips.ProbeTableCollect):
                SetTitle(I2.Loc.ScriptLocalization.Get("TUTORIAL/PROBE_TABLE_COLLECT_TITLE"));
                animPath = "TutorialAnimations/ProbeTableCollect";
                break;
            case (TutorialAnimationClips.ProbeTableSeed):
                SetTitle(I2.Loc.ScriptLocalization.Get("TUTORIAL/PROBE_TABLE_SEED_TITLE"));
                animPath = "TutorialAnimations/ProbeTableSeed";
                break;
            case (TutorialAnimationClips.DeliverElixirToDoctor):
                SetTitle(I2.Loc.ScriptLocalization.Get("TUTORIAL/DELIVER_ELIXIR_TO_DOCTOR_TITLE"));
                animPath = "TutorialAnimations/DeliverElixirToDoctor";
                break;
            case (TutorialAnimationClips.ProduceCures):
                SetTitle(I2.Loc.ScriptLocalization.Get("TUTORIAL/PRODUCE_CURES_TITLE"));
                animPath ="TutorialAnimations/ProduceCures";
                break;
            case (TutorialAnimationClips.MoveAndRotateObjects):
                SetTitle(I2.Loc.ScriptLocalization.Get("TUTORIAL/MOVE_AND_ROTATE_OBJECT_TITLE"));
                animPath = "TutorialAnimations/MoveAndRotateObjects";
                break;
            default:
                Debug.LogError("This animation clip case does not exist. Please add it.");
                break;
        }

        Timing.RunCoroutine(CreateAnimation(animPath));
    }

    void SetTitle(string title) {
        TitleText.text = title;
    }

    IEnumerator<float> CreateAnimation(string resourcesPath) {
        //Animations are loaded from Resources so their assets are not stored in RAM when they are not used (99% of the time)
#if UNITY_EDITOR
        ResourceRequest rr = Resources.LoadAsync(resourcesPath);
        while (!rr.isDone)
            yield return 0;

        DestroyUsedAnimation();
        spawnedGameObject = Instantiate(rr.asset) as GameObject;
#elif UNITY_ANDROID
        ResourceRequest rr = Resources.LoadAsync(resourcesPath);
        while (!rr.isDone)
            yield return 0;

        DestroyUsedAnimation();
        spawnedGameObject = Instantiate(rr.asset) as GameObject;
#else
        if (BundleManager.Instance.tutorialBundle == null)
        {
            ResourceRequest rr = Resources.LoadAsync(resourcesPath);
            while (!rr.isDone)
                yield return 0;

            DestroyUsedAnimation();
            spawnedGameObject = Instantiate(rr.asset) as GameObject;
        }
        else
        {
            var AssetPathSplit = resourcesPath.Split('/');
            string assetName = AssetPathSplit[AssetPathSplit.Length - 1];
            AssetBundleRequest ar = BundleManager.Instance.tutorialBundle.LoadAssetAsync(assetName);
            while (!ar.isDone)
                yield return 0;

            DestroyUsedAnimation();
            spawnedGameObject = Instantiate(ar.asset) as GameObject;
        }
#endif

        spawnedGameObject.transform.SetParent(AnimParent);
        spawnedGameObject.transform.localScale = Vector3.one;
        RectTransform rt = spawnedGameObject.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector3.zero;

        Timing.KillCoroutine(DelayedCloseButton().GetType());
        Timing.RunCoroutine(DelayedCloseButton());
    }

    IEnumerator<float> DelayedCloseButton() {
        bool isShown = false;
        float t = 0;

        while (!isShown)
        {
            t += Time.deltaTime;
            if (t >= 4f)
            {
                CloseButton.SetActive(true);
                CloseButton.transform.SetAsLastSibling();
                isShown = true;
            }
            yield return 0;
        }
    }

    public void HideAnimation()
    {
        IsActive = false;
        anim.ResetTrigger("Intro");
        anim.SetTrigger("Outro");
        DestroyUsedAnimation();
    }

    void DestroyUsedAnimation() {
		if (spawnedGameObject) {
			Transform[] children = spawnedGameObject.GetComponentsInChildren<Transform>();
			for (int i = 0; i < children.Length; ++i) {
				Destroy (children[i].gameObject);
			}
			Destroy(spawnedGameObject);
			//#if UNITY_IOS
			Resources.UnloadAsset(spawnedGameObject);
			Resources.UnloadUnusedAssets();
			//System.GC.Collect();
			//#endif
		}
    }

}
