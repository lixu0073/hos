using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using MovementEffects;

public class BoosterActivatedPopupController : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI boosterTitle = null;
	[SerializeField] private TextMeshProUGUI boosterName = null;

	[SerializeField] private Image mainImage = null;
	[SerializeField] private Image mainImageHighlight = null;

	[SerializeField] private Image[] SideImages = null;
#pragma warning disable 0649
    [SerializeField] private Sprite coinImage;
	[SerializeField] private Sprite starImage;
#pragma warning restore 0649
    [SerializeField] private GameObject LittleStars  = null;

	public void Open(int boosterID, bool activated, float delay)
    {
		Timing.RunCoroutine (OpenDelayed(boosterID, activated, delay));
	}

	public void Open(int boosterID, bool activated)
    {
		gameObject.SetActive (true);

		if (activated)
        {
			LittleStars.SetActive (true);
			boosterTitle.SetText (I2.Loc.ScriptLocalization.Get("BOOSTERS/BOOSTER_ACTIVATED"));
		}
        else
        {
			LittleStars.SetActive (false);
			boosterTitle.SetText (I2.Loc.ScriptLocalization.Get("BOOSTERS/BOOSTER_EXPIRED"));
		}

		boosterName.SetText (I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().boosterDatabase.boosters[boosterID].shortInfo));
		mainImage.sprite = ResourcesHolder.Get ().boosterDatabase.boosters [boosterID].icon;
		mainImageHighlight.sprite = ResourcesHolder.Get ().boosterDatabase.boosters [boosterID].icon;
		Sprite tempSprite = null;
		switch (ResourcesHolder.Get ().boosterDatabase.boosters [boosterID].boosterType)
        {
    		case BoosterType.Coin:
    			tempSprite = coinImage;
    			break;
    		case BoosterType.Exp:
    			tempSprite = starImage;
    			break;
            case BoosterType.CoinAndExp:
                tempSprite = starImage;
                break;
            case BoosterType.Action:
    			tempSprite = null;
    			break;
		}

        bool isStar = false;
        int tmpId = 0;
		if(tempSprite != null)
        {
			for(int i = 0; i < SideImages.Length; i++)
            {
				SideImages[i].gameObject.SetActive(true);
                if (ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterType == BoosterType.CoinAndExp)
                {
                    if (i == 5) // dla lewej strony reset dla pierszego itemu ~ rocket science aby nie zmieniac tego co jest
                    {
                        isStar = false;
                        tmpId = 0;
                    }

                    if (isStar)
                        SideImages[i].sprite = starImage;
                    else
                        SideImages[i].sprite = coinImage;

                    ++tmpId;

                    if (tmpId == 2)
                    {
                        isStar = !isStar;
                        tmpId = 0;
                    }
                }
                else SideImages[i].sprite = tempSprite;
			}
		}
        else
        {
			for(int i = 0; i < SideImages.Length; i++)
            {
				SideImages[i].gameObject.SetActive(false);
			}
		}
	}

	public void HideIt()
    {
		gameObject.SetActive (false);
	}

	IEnumerator<float> OpenDelayed(int boosterID, bool activated, float delay)
    {
		yield return Timing.WaitForSeconds (delay);
		Open(boosterID, activated);
	}
}
