using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Hospital;
using SimpleUI;
using TMPro;
using MovementEffects;
using IsoEngine;
using System.Collections;

public class StoragePopup : UIElement
{
    #region localizationText
    public TextMeshProUGUI UpgradeText;
    public TextMeshProUGUI AmountText;
    #endregion

    [SerializeField] ScrollRect scrollRect = null;
    [SerializeField] RectTransform scrollContentRect = null;
    [SerializeField] GameObject elementPrefab = null;
    [SerializeField] Slider AmountIndicator = null;
    [SerializeField] GameObject scrollBar = null;
    [SerializeField] private List<StorageTab> tabs = null;

    int actualTab = 0;
    int index = 0;

    ElixirStorage elixirStorage;
    ElixirTank elixirTank;

    [SerializeField] private Button UpgradeButton = null;
    [SerializeField] private Sprite upgradeSpriteActive = null;
    [SerializeField] private Sprite upgradeSpriteInactive = null;

    private bool isElixirTank = false;
    private Vector2 contentPos = Vector2.zero;

    void Awake()
    {
        contentPos = scrollContentRect.anchoredPosition;
    }

    public void Open(bool isFadeIn = true, bool preservesHovers = false, bool isElixirTank = false)
    {
        this.isElixirTank = isElixirTank;

        if (!isElixirTank)
        {
            index = 0;
            this.elixirStorage = GameState.Get().ElixirStore;
            ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(elixirStorage.position + new Vector2i(2, 2), 1f, false);
        }
        else
        {
            index = 1;
            this.elixirTank = GameState.Get().ElixirTank;
            ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(elixirTank.position + new Vector2i(2, 2), 1f, false);
        }

        //HintsController.Get().TryToHideIndicator();
        SetUpgradeButton();

        gameObject.SetActive(true);
        StartCoroutine(OpenCoroutine(isElixirTank));
    }

    private IEnumerator OpenCoroutine(bool isElixirTank = false)
    {
        yield return null; // CV: to force 1-frame delay before render the TMPro

        UpgradeText.text = I2.Loc.ScriptLocalization.Get("UPGRADE_U");

        ResetContent();
        ActualiseIndicator();

        int storageCounter = 0;

        foreach (var p in GameState.Get().EnumerateResourcesMedRef())
        {
            if (ResourcesHolder.Get().GetLvlForCure(p.Key) <= Game.Instance.gameState().GetHospitalLevel() && ResourcesHolder.Get().GetIsTankStorageCure(p.Key) == isElixirTank)
            {
                var z = GameObject.Instantiate(elementPrefab);
                var t = z.transform;
                t.GetChild(0).GetComponent<Image>().sprite = ResourcesHolder.Get().GetSpriteForCure(p.Key);
                var cure = p.Key;

                t.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = p.Value.ToString();
                t.SetParent(scrollRect.content);
                t.localScale = new Vector3(1, 1, 1);

                if (p.Key.type == MedicineType.BasePlant)
                {
                    z.GetComponent<PointerDownListener>().SetDelegate(() =>
                    {
                        FloraTooltip.Open(cure);
                    });
                }
                else
                {
                    z.GetComponent<PointerDownListener>().SetDelegate(() =>
                    {
                        TextTooltip.Open(cure, false, true);
                    });
                }
                z.SetActive(true);
                storageCounter++;
            }
        }

        LayoutRebuilder.MarkLayoutForRebuild(scrollRect.content);

        UpdateTabVisible();

        int modifier = (storageCounter <= 4) ? 1 : Mathf.CeilToInt(storageCounter / 4f);        
        scrollContentRect.sizeDelta = new Vector2(428f, 105f * modifier);

        scrollBar.SetActive(true);
        scrollRect.verticalNormalizedPosition = 1;

        yield return base.Open();
    }

    public void ActualiseIndicator()
    {
        if (isElixirTank)
        {
            AmountIndicator.value = GameState.Get().elixirTankAmount / (float)GameState.Get().maximimElixirTankAmount;
            AmountText.text = I2.Loc.ScriptLocalization.Get("CREATE_OFFER_TITLE_TANK") + ": " + GameState.Get().elixirTankAmount.ToString() + " /" + GameState.Get().maximimElixirTankAmount.ToString();
        }
        else
        {
            AmountIndicator.value = GameState.Get().elixirStorageAmount / (float)GameState.Get().maximimElixirStorageAmount;
            AmountText.text = I2.Loc.ScriptLocalization.Get("CURE_STORAGE_U") + ": " + GameState.Get().elixirStorageAmount.ToString() + " /" + GameState.Get().maximimElixirStorageAmount.ToString();
        }
    }

    public void ResetContent()
    {
        scrollContentRect.anchoredPosition = contentPos;

        if (scrollRect.content.childCount > 0)
        {
            for (int i = 0; i < scrollRect.content.childCount; i++)
                Destroy(scrollRect.content.GetChild(i).gameObject);
        }
    }

    public void UpdateTabVisible()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            tabs[i].GetComponent<RectTransform>().SetAsFirstSibling();
        }

        tabs[index].GetComponent<RectTransform>().SetSiblingIndex(2);

        if (index >= 0 && index < tabs.Count)
        {
            if (actualTab != -1)
            {
                tabs[actualTab].ChangeTabButton(false);
            }
            else
            {
                for (int i = 0; i < tabs.Count; i++)
                    tabs[i].ChangeTabButton(false);
            }

            tabs[index].ChangeTabButton(true);
            actualTab = index;
        }
    }


    public void SetTabVisible(int index)
    {
        this.index = index;

        if (index == 0)
            Open();
        else
            Open(true, false, true);
    }

    public int GetActualTab()
    {
        return actualTab;
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        base.Exit(hidePopupWithShowMainUI);

        ResetContent();
    }

    public void ButtonExit()
    {
        Exit();
    }

    public void ButtonUpgrade()
    {
        if (Game.Instance.gameState().GetHospitalLevel() >= 4)
        {
            SoundsController.Instance.PlayObjectUpgrade();
            Exit(false);

            if (!this.isElixirTank)
            {
                UIController.getHospital.StorageUpgradePopUp.Open(() =>
                {
                    var fp = (GameObject)Instantiate(ResourcesHolder.Get().ParticleUnpack, new Vector3(this.elixirStorage.position.x + this.elixirStorage.actualData.rotationPoint.x, 0, this.elixirStorage.position.y + this.elixirStorage.actualData.rotationPoint.y) + new Vector3(-5, 5 * Mathf.Sqrt(2), -5), Quaternion.Euler(0, 0, 0));
                    fp.transform.localScale = Vector3.one * 1.4f;
                    fp.SetActive(true);
                    base.Exit();
                }, false);
            }
            else
            {
                UIController.getHospital.StorageUpgradePopUp.Open(() =>
                {
                    var fp = (GameObject)Instantiate(ResourcesHolder.Get().ParticleUnpack, new Vector3(this.elixirTank.position.x + this.elixirTank.actualData.rotationPoint.x, 0, this.elixirTank.position.y + this.elixirTank.actualData.rotationPoint.y) + new Vector3(-5, 5 * Mathf.Sqrt(2), -5), Quaternion.Euler(0, 0, 0));
                    fp.transform.localScale = Vector3.one * 1.4f;
                    fp.SetActive(true);
                    base.Exit();
                }, true);
            }

            ActualiseIndicator();
        }
        else
        {
            if (!this.isElixirTank)
                MessageController.instance.ShowMessage(45);
            else
                MessageController.instance.ShowMessage(48);
        }
    }

    void SetUpgradeButton()
    {
        if (!this.isElixirTank)
        {
            if (elixirStorage.actualLevel == elixirStorage.GetMaxLVL())
            {
                UpgradeButton.gameObject.SetActive(false);
            }
            else
            {
                UpgradeButton.gameObject.SetActive(true);

                if (Game.Instance.gameState().GetHospitalLevel() < 4)
                    UpgradeButton.image.sprite = upgradeSpriteInactive;
                else
                    UpgradeButton.image.sprite = upgradeSpriteActive;
            }
        }
        else
        {
            if (elixirTank.actualLevel == elixirTank.GetMaxLVL())
            {
                UpgradeButton.gameObject.SetActive(false);
            }
            else
            {
                UpgradeButton.gameObject.SetActive(true);

                if (Game.Instance.gameState().GetHospitalLevel() < 4)
                    UpgradeButton.image.sprite = upgradeSpriteInactive;
                else
                    UpgradeButton.image.sprite = upgradeSpriteActive;
            }
        }
    }


    public void ShowScrollbar()
    {
        scrollBar.SetActive(true);
        Timing.KillCoroutine(HideScrollbar().GetType());
        Timing.RunCoroutine(HideScrollbar());
    }

    IEnumerator<float> HideScrollbar()
    {
        yield return Timing.WaitForSeconds(0.5f);
        scrollBar.SetActive(false);
    }

}
