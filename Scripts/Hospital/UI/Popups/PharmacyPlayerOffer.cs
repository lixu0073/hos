using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PharmacyPlayerOffer : MonoBehaviour {

    public Button button;
    public Image itemIcon;
    public Image itemGlow;
    public Image adBadge;
    public Image discountBadge;
    public GameObject buyerGO;
    public Image buyerAvatar;
    public TextMeshProUGUI buyerName;
    public TextMeshProUGUI buyerLevel;
    public GameObject priceGO;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI lockedByLevelText;
    public GameObject lockedByLevelGO;
    public GameObject soldVisit;
	public Animator anim;
    public GameObject itemDescent;
    public Sprite CoinSprite;

    public Image[] toGrayscale;


    public void SetItemIcon(bool isEnabled, Sprite sprite)
    {
        itemIcon.sprite = sprite;
        itemIcon.enabled = isEnabled;
        itemGlow.enabled = isEnabled;
    }

    public void SetPriceText(int price)
    {
        priceText.text = price.ToString();
    }

    public void SetAmountText(bool isEnabled, int amount)
    {
        amountText.text = amount + "x";
        amountText.enabled = isEnabled;
    }

    public void SetAdBadge(bool isEnabled)
    {
        adBadge.enabled = isEnabled;
    }
    
    public void SetDiscountBadge(bool isEnabled)
    {
        discountBadge.enabled = isEnabled;
    }

    public void SetSoldVisit(bool isEnabled)
    {
        soldVisit.SetActive(isEnabled);
        priceGO.SetActive(!isEnabled);
    }

    public void SetBuyer(bool isEnabled, Sprite sprite, string name, int level)
    {
        if (!isEnabled)
        {
            buyerGO.SetActive(false);
            return;
        }
        else
        {
            buyerGO.SetActive(true);
            SetAmountText(false, 0);
            buyerAvatar.sprite = sprite;
            itemGlow.enabled = isEnabled;
            if (name != null)
                buyerName.text = Truncate(name,14);
            else
                buyerName.text = "";

            if (level != 0)
                buyerLevel.text = level.ToString();
        }
    }

    public string Truncate(string str, int maxLength)
    {
        if (!string.IsNullOrEmpty(str) && str.Length > maxLength)
        {
            return str.Substring(0, maxLength);
        }

        return str;
    }

    public void SetLockedByLevel(bool isEnabled, int level)
    {
        lockedByLevelGO.SetActive(isEnabled);
        if (isEnabled)
        {
            lockedByLevelText.text = "Unlock on level " + level;
            priceGO.SetActive(false);
            SetGrayscale(true);
        }
        else
        {
            priceGO.SetActive(true);
            SetGrayscale(false);
        }
    }

    public void SetGrayscale(bool isGrayscale)
    {
        int length = toGrayscale.Length;
        Material grayscaleMaterial = ResourcesHolder.Get().GrayscaleMaterial;
        if (isGrayscale)
        {
            for (int i = 0; i < length; i++)
                toGrayscale[i].material = grayscaleMaterial;
        }
        else
        {
            for (int i = 0; i < length; i++)
                toGrayscale[i].material = null;
        }
    }

    public void ActivateItemDescent()
    {
        itemDescent.SetActive(true);
    }

    public void DeactivateItemDescent()
    {
        itemDescent.SetActive(false);
    }

    public void SetupItemDescent(int amount, Sprite sprite = null)
    {
        GameObject itemGO = itemDescent.transform.GetChild(0).gameObject;
        GameObject amountGO = itemDescent.transform.GetChild(1).gameObject;
        itemGO.GetComponent<Image>().sprite = sprite == null ? CoinSprite : sprite;
        itemGO.transform.localScale = sprite == null ? new Vector3(0.6f, 0.6f, 0.6f) : Vector3.one;
        TextMeshProUGUI amountT = amountGO.GetComponent<TextMeshProUGUI>();
        amountT.text = "-" + amount; 
    }

    public void SetAnimator(string Trigger) {
		anim.SetTrigger (Trigger);
	}
}