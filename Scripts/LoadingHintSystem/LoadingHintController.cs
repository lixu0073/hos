using UnityEngine;
using System.Collections.Generic;
using TMPro;
using MovementEffects;

public class LoadingHintController : MonoBehaviour 
{
#pragma warning disable 0649
    [SerializeField] int hintDuration;
	[SerializeField] private LoadingHintDatabase hintDatabase;
#pragma warning restore 0649
    private TextMeshProUGUI hintText;

	private List<int> freeIDs;
	private List<int> takenIDs;

	private bool hintCoroutineOn = false;

	void Awake()
    {
		hintText = GetComponent<TextMeshProUGUI> ();

		freeIDs = new List<int> ();
		takenIDs = new List<int> ();
		for (int i = 0; i < hintDatabase.keys.Length; i++)
        {
#if UNITY_ANDROID
            // 13th loading hint ("LOADINGSCREEN_HINTS/LOADING_13") is about connecting with Game Center which is only available on iOS. Don't show it on Android.
            if (i == 13)
                continue;
#endif
            freeIDs.Add (i);
		}
	}

	void OnEnable()
    {
		ShowHints ();
	}

	void OnDisable()
    {
        Timing.KillCoroutine (LoadingHintCoroutine().GetType());
        hintCoroutineOn = false;
    }

    public void ShowHints()
    {
	//	hintText.gameObject.SetActive (true);
	//	hintText.text = GetRandomNoRepHint();
		if (!hintCoroutineOn)
        {
			Timing.RunCoroutine(LoadingHintCoroutine());
		}
	}

	public void HideHints()
    {
		Timing.KillCoroutine (LoadingHintCoroutine().GetType());
		hintCoroutineOn = false;
	}

	private string GetRandomNoRepHint()
    {
		string hint = "";
		bool messageFound = false;
		while (!messageFound)
        {
			int ID = freeIDs[Random.Range (0,freeIDs.Count)];
			freeIDs.Remove (ID);

			if (freeIDs.Count == 0)
            {
				RefreshSupport ();
			}
			takenIDs.Add (ID);

			hint = I2.Loc.ScriptLocalization.Get(hintDatabase.keys[ID]);
			messageFound = true;
			return hint;
		}
		return "";
	}

	private void RefreshSupport()
    {
		freeIDs.AddRange (takenIDs);
		takenIDs.Clear ();
	}	

	IEnumerator<float> LoadingHintCoroutine()
    {
        while (!Hospital.LoadingGame.AreFontsChecked)
            yield return Timing.WaitForSeconds(.05f);

        hintText.SetText("");
        for (;;)
        {
			hintCoroutineOn = true;
            yield return Timing.WaitForSeconds(0.5f);
            //	yield return Timing.WaitForSeconds (0.25f);
            hintText.SetText(GetRandomNoRepHint());
			//hintText.SetText (I2.Loc.ScriptLocalization.Get(GetRandomNoRepHint()));
			yield return Timing.WaitForSeconds (hintDuration);
			hintText.SetText ("");
			yield return Timing.WaitForSeconds (0.5f);
		}

	}
}
