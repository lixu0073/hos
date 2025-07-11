using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LockedFeatureRequiredItem : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private Image image;

    [SerializeField]
    private TextMeshProUGUI amountText;
#pragma warning restore 0649

    public void SetUp(int amount)
    {
        SetUp(ResourcesHolder.Get().bbCoinSprite, amount);
    }

    public void SetUp(MedicineRef med, int amount)
    {
        SetUp(ResourcesHolder.Get().GetSpriteForCure(med), amount);
    }

    private void SetUp(Sprite sprite, int amount)
    {
        image.sprite = sprite;
        amountText.SetText(amount.ToString());
    }

}
