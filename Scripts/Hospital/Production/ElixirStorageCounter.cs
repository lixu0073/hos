using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Hospital;
using TMPro;
using MovementEffects;

public class ElixirStorageCounter : MonoBehaviour
{
    public Image storageIcon;
    public Image equalizer;
    public Sprite medicineStorageSprite;
    public Sprite medicineTankSprite;
    public Sprite[] equalizerSprites;
    public Transform[] equalizerPositions;
#pragma warning disable 0649
    [SerializeField] BoolVar IsActive;
#pragma warning restore 0649
    [SerializeField] Slider storageBar = null;
    [SerializeField] TextMeshProUGUI nameLabel = null;
    [SerializeField] TextMeshProUGUI amountLabel = null;
    [SerializeField] CanvasGroup group = null;

    ElixirStorage elixirStorage;
    int elixirStorageTempAmount;

    ElixirTank elixirTank;
    int elixirTankTempAmount;

    int count = 0;
    bool isShown = false;
    bool blockHide = true;

    private void Start()
    {
        if (ExtendedCanvasScaler.HasNotch())
        {
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.localPosition += new Vector3(0, -24.46f, 0);
        }
    }

    public void Add(bool isTankElixir, bool updateSprite = true)
    {
        if (isTankElixir && elixirTank == null)
            elixirTank = Game.Instance.gameState().ElixirTank;
        else if (elixirStorage == null)
            elixirStorage = Game.Instance.gameState().ElixirStore;

        ++count;

        if (!isShown)
        {
            isShown = true;
            blockHide = true;
            SetTempAmounts(isTankElixir);
            UpdateStorageInfo(isTankElixir, updateSprite);
            Timing.KillCoroutine(Hide().GetType());
            Timing.RunCoroutine(Show());
        }
    }

    public void Add(int medicineAmount, bool isTankElixir, bool updateSprite = true)
    {
        if (isTankElixir && elixirTank == null)
            elixirTank = Game.Instance.gameState().ElixirTank;
        else if (elixirStorage == null)
            elixirStorage = Game.Instance.gameState().ElixirStore;

        count = count + medicineAmount;

        if (!isShown)
        {
            isShown = true;
            blockHide = true;
            SetTempAmounts(isTankElixir);
            UpdateStorageInfo(isTankElixir, updateSprite);
            Timing.KillCoroutine(Hide().GetType());
            Timing.RunCoroutine(Show());
        }
    }

    public void SetCounterStartAmount()
    {
        elixirTank = Game.Instance.gameState().ElixirTank;
        elixirStorage = Game.Instance.gameState().ElixirStore;

        SetTempAmounts(true);
        SetTempAmounts(false);
    }

    // method for instant add resources (BubbleBoy && Pharmacy)
    public void AddLater(int medicineAmount, bool isTankElixir, bool updateSprite = true)
    {
        if (isTankElixir && elixirTank == null)
            elixirTank = Game.Instance.gameState().ElixirTank;
        else if (elixirStorage == null)
            elixirStorage = Game.Instance.gameState().ElixirStore;

        count = count + medicineAmount;

        if (!isShown)
        {
            isShown = true;
            blockHide = true;
            SetTempAmounts(isTankElixir, medicineAmount);
            UpdateStorageInfo(isTankElixir, updateSprite);
            Timing.KillCoroutine(Hide().GetType());
            Timing.RunCoroutine(Show());
        }
    }

    public void AddManyLater(int medicineAmount, bool isTankElixir, bool updateSprite = true)
    {
        if (isTankElixir && elixirTank == null)
            elixirTank = Game.Instance.gameState().ElixirTank;
        else if (elixirStorage == null)
            elixirStorage = Game.Instance.gameState().ElixirStore;

        count = count + medicineAmount;

        if (!isShown)
        {
            isShown = true;
            blockHide = true;
            UpdateStorageInfo(isTankElixir, updateSprite);
            Timing.KillCoroutine(Hide().GetType());
            Timing.RunCoroutine(Show());
        }
    }

    public void Remove(int medicineAmount, bool isTankElixir, bool updateSprite = true)
    {
        if (isTankElixir)
            elixirTankTempAmount += medicineAmount;
        else elixirStorageTempAmount += medicineAmount;

        UpdateStorageInfo(isTankElixir, updateSprite);
        count = count - medicineAmount;
        if (count <= 0)
        {
            isShown = false;
            Timing.KillCoroutine(Hide().GetType());
            Timing.RunCoroutine(Hide());
        }
    }

    private void SetTempAmounts(bool isTankElixir, int laterAddAmount = 0)
    {
        if (isTankElixir)
            elixirTankTempAmount = elixirTank.actualAmount - laterAddAmount > 0 ? elixirTank.actualAmount - laterAddAmount : 0;
        else elixirStorageTempAmount = elixirStorage.actualAmount - laterAddAmount > 0 ? elixirStorage.actualAmount - laterAddAmount : 0;
    }

    private void UpdateStorageInfo(bool isTankElixir, bool updateSprite)
    {
        if (isTankElixir)
        {
            if (updateSprite)
            {
                nameLabel.text = I2.Loc.ScriptLocalization.Get("CREATE_OFFER_TITLE_TANK");
                storageIcon.sprite = medicineTankSprite;

                storageBar.value = elixirTankTempAmount / (float)elixirTank.maximumAmount;
                amountLabel.text = elixirTankTempAmount + "/" + elixirTank.maximumAmount.ToString();
            }
        }
        else
        {
            if (updateSprite)
            {
                nameLabel.text = I2.Loc.ScriptLocalization.Get("CURE_STORAGE_U");
                storageIcon.sprite = medicineStorageSprite;

                storageBar.value = elixirStorageTempAmount / (float)elixirStorage.maximumAmount;
                amountLabel.text = elixirStorageTempAmount + "/" + elixirStorage.maximumAmount.ToString();
            }
        }

        equalizer.gameObject.SetActive(false);
    }

    IEnumerator<float> Show()
    {
        IsActive.Value = true;
        transform.SetAsLastSibling();
        float t = 0;
        float fadeTime = .35f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            group.alpha = t / fadeTime;
            yield return 0;
        }

        group.alpha = 1;
        blockHide = false;
    }

    IEnumerator<float> Hide()
    {
        // wait 2s for many collection items
        yield return Timing.WaitForSeconds(1f);

        while (UIController.get.LevelUpPopUp.gameObject.activeSelf)
            yield return Timing.WaitForSeconds(.1f);

        yield return Timing.WaitForSeconds(1f);

        while (UIController.get.LevelUpPopUp.gameObject.activeSelf)
            yield return Timing.WaitForSeconds(.5f);

        float t = 0;
        float fadeTime = .35f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            if (!blockHide)
            {
                group.alpha = 1 - (t / fadeTime);
            }
            yield return 0;
        }

        isShown = false;
        group.alpha = 0;
        IsActive.Value = false;
        yield return 0;
    }
}
