using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;
using Hospital;
using TMPro;
using SimpleUI;

public class VitaminUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] AnimationCurve Curve;
    [SerializeField] Image barFillImage;
    [SerializeField, Range(0.01f, 0.9f)] float fillSetpper = 0;
    [SerializeField] Animator animator;
    [SerializeField] TextMeshProUGUI UnlockAtText;
    [SerializeField] TextMeshProUGUI AmountText;
    [SerializeField] ParticleSystem BubblesParticles;
    [SerializeField] ParticleSystem UpgradeParticles;
    [SerializeField] GameObject UnlockedPanel;
    [SerializeField] GameObject LockedPanel;
    [SerializeField] TextMeshProUGUI TimeToFillUnit;
    [SerializeField] TextMeshProUGUI AmountOfVitaminInStorage;
    [SerializeField] Button CollectButton;
    [SerializeField] Image buttonImage;
    [SerializeField] Sprite standardIcon;
    [SerializeField] Sprite speedUpIcon;
    [SerializeField] GameObject ExclamationMark;
    [SerializeField] Animator BarAnimator;
#pragma warning restore 0649
    [SerializeField] private float bubbleMaxDurationTime = 4.8f;

    float currentBarFill = 0;
    private Coroutine onOpenCoroutine;
    private VitaminCollectorModel model;
    private int dropAnimationTrigger = Animator.StringToHash("WholeVitamin");

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void VitaminSetup(VitaminCollectorModel model)
    {
        this.model = model;
    }

    public void PrepareContent()
    {
        ActivatePanel();
        barFillImage.fillAmount = 1;

        if (Game.Instance != null)
            Debug.LogError("Game.Instance.gameState().GetHospitalLevel(): " + Game.Instance.gameState().GetHospitalLevel().ToString());

        if (model != null)
            Debug.LogError("model.GetVitaminCollectorUnlockLevel(): " + model.GetVitaminCollectorUnlockLevel());

        if (Game.Instance.gameState().GetHospitalLevel() >= model.GetVitaminCollectorUnlockLevel())
        {
            SubscribeToEvents();
            SetupText();
        }
        else
            DeactivatePanel();
    }

    private void Model_vitaminCollected(int amount, MedicineRef vitamin)
    {
        SetupCollectButton();
    }

    private void Model_capacityChanged(float fill, float current, int max, int producedAmount, MedicineRef vitamin, int timeToDrop, VitaminCollectorModel.VitaminSource source)
    {
        if (onOpenCoroutine == null)
        {
            fill = Mathf.Clamp01(fill);
            UpdateFillBar(fill);
            if (producedAmount > 0)
            {
                animator.SetTrigger(dropAnimationTrigger);
                SoundsController.Instance.PlayPanaceaBubble();
                SetupCollectButton();
                if (source == VitaminCollectorModel.VitaminSource.FullRefill)
                    onOpenCoroutine = StartCoroutine(FillBarLerp((current - producedAmount), max));
            }
            currentBarFill = model.capacity;
            SetupText(timeToDrop);
        }
    }

    public void OnOpen()
    {
        SetupText();
        SetupCollectButton();
    }

    private void SetupText(int timeToDrop = -1)
    {
        AmountText.text = (int)model.capacity + "/" + model.maxCapacity;
        AmountOfVitaminInStorage.text = Game.Instance.gameState().GetCureCount(model.med).ToString();
        if (timeToDrop >= 0)
            TimeToFillUnit.text = UIController.GetFormattedTime(timeToDrop);
        else
            TimeToFillUnit.text = I2.Loc.ScriptLocalization.Get("COLLECTOR_FULL");
    }

    public void OnClose()
    {
        if (onOpenCoroutine != null)
        {
            try
            { 
                StopCoroutine(onOpenCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            onOpenCoroutine = null;
        }
        // ActivatePanel();
        UnSubscribeFromEvents();
    }

    private void DeactivatePanel()
    {
        LockedPanel.SetActive(true);
        UnlockedPanel.SetActive(false);
        UnlockAtText.text = string.Format(I2.Loc.ScriptLocalization.Get("UNLOCKS_AT_LEVEL"), model.GetVitaminCollectorUnlockLevel());
    }

    private void ActivatePanel()
    {
        LockedPanel.SetActive(false);
        UnlockedPanel.SetActive(true);
    }

    private void SubscribeToEvents()
    {
        if (model != null)
        {
            model.capacityChanged += Model_capacityChanged;
            model.vitaminCollected += Model_vitaminCollected;
        }
    }

    private void UnSubscribeFromEvents()
    {
        if (model != null)
        {
            model.capacityChanged -= Model_capacityChanged;
            model.vitaminCollected -= Model_vitaminCollected;
        }
    }

    private void UpdateFillBar(float normalizedValue)
    {
        barFillImage.fillAmount = normalizedValue;
        var mm = BubblesParticles.main;
        mm.startLifetime = normalizedValue * bubbleMaxDurationTime; ;
    }

    private void SetupCollectButton()
    {
        if (model.capacity < 1)
        {
            buttonImage.sprite = speedUpIcon;
            CollectButton.RemoveAllOnClickListeners();
            CollectButton.onClick.AddListener(SpeedUpCollect);
            if (!CacheManager.HasRefillVitaminPopupSeen(model.med.ToString()))
                TutorialUIController.Instance.BlinkImage(CollectButton.image, 1.1f);
        }
        else
            buttonImage.sprite = standardIcon;

        ExclamationMark.SetActive(model.IsRequiredToHealPatientInTreatmentRoom());
    }

    private IEnumerator FillBarLerp(float start, int end)
    {
        float goal = end;
        float maxVitaminCollectorCapacity = model.maxCapacity;
        float barHeighTargetOnOpen = Mathf.Clamp(goal, 0f, maxVitaminCollectorCapacity) / maxVitaminCollectorCapacity;
        float currentBarFillNormalized = start;
        if (barHeighTargetOnOpen != 0)
        {
            while (currentBarFillNormalized < barHeighTargetOnOpen * 0.99f)
            {
                float ratio = Mathf.Clamp01(currentBarFillNormalized / barHeighTargetOnOpen);
                currentBarFillNormalized += (Curve.Evaluate(ratio)) * fillSetpper;
                currentBarFillNormalized = Mathf.Clamp(currentBarFillNormalized, 0, barHeighTargetOnOpen);
                UpdateFillBar(currentBarFillNormalized);
                yield return new WaitForEndOfFrame();
            }
        }
        currentBarFill = model.capacity;
        onOpenCoroutine = null;
    }

    public void OnInfoButtonClick()
    {
        try
        { 
            UIController.getHospital.vitaminesCollectorInfoPopup.Open(new VitamineCollectorInfoPopup.InputData() { collectorModel = model }, () => UpgradeParticles.Play());
        }
        catch (Exception e)
        {
            Debug.LogWarning("exception: " + e.Message);
        }
    }

    public void SpeedUpCollect()
    {
        Debug.LogError("SpeedUP Clicked");
    }

    public void onCollectClick(bool firstTry)
    {
        int amountToCollect = 0;
        try
        {
            amountToCollect = model.Collect();
            if (amountToCollect > 0)
            {
                UIController.get.storageCounter.AddLater(amountToCollect, true);
                Canvas canvas = UIController.get.canvas;
                Vector2 startPoint = new Vector2((CollectButton.gameObject.transform.position.x - Screen.width / 2) / canvas.transform.localScale.x, (CollectButton.gameObject.transform.position.y - Screen.height / 2) / canvas.transform.localScale.y);
                if (BarAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Collect") || (BarAnimator.IsInTransition(0) && BarAnimator.GetNextAnimatorStateInfo(0).IsTag("Collect")))
                    BarAnimator.SetTrigger("ForceIdle");

                BarAnimator.SetTrigger("OnCollect");
                SoundsController.Instance.PlayCollectVitamin();
                ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Medicine, startPoint, amountToCollect, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(model.med), null, () =>
                {
                    UIController.get.storageCounter.Remove(amountToCollect, model.med.IsMedicineForTankElixir());
                });
                if (Game.Instance.gameState() is GameState)
                {
                    int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
                    int expRecieved = ResourcesHolder.Get().GetMedicineInfos(model.med).Exp * amountToCollect;
                    ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), expRecieved, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                    {
                        Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expRecieved, currentExpAmount);
                    });
                }
                SetupCollectButton();
            }
        }
        catch (VitaminCollectorModel.NothingToCollectException)
        {
            if (firstTry)
            {
                UIController.get.vitaminsMakerRefillmentPopup.Open(new VitaminsMakerRefillmentPopup.InputData() { collectorModel = model });
                SoundsController.Instance.PlayButtonClick(false);
                //nothingToCollectMessageSeen = true;
            }
        }
        catch (VitaminCollectorModel.StorageFullException)
        {
            if (UIController.getHospital != null)
            {
                if (tankFullMessageSeen)
                    return;
                if (firstTry)
                    StartCoroutine(UIController.getHospital.StorageFullPopUp.Open(true));

                MessageController.instance.ShowMessage(47);
                tankFullMessageSeen = true;
            }
            else
                MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get("TANK_FULL_FLOAT_MATERNITY"));
        }
    }

    bool collectDown = false;
    float collectButtonDownTimer = 0;

    bool tankFullMessageSeen = false;
    //bool nothingToCollectMessageSeen = false;

    private void Update()
    {
        TryToCollect();
    }

    void TryToCollect()
    {
        if (!collectDown)
            return;
        collectButtonDownTimer += Time.deltaTime;
        if (collectButtonDownTimer >= .5f)
        {
            if (collectDown)
            {
                onCollectClick(false);
                collectButtonDownTimer -= .15f;
            }
        }
    }

    public void CollectButtonDown()
    {
        collectDown = true;
        //nothingToCollectMessageSeen = false;
        tankFullMessageSeen = false;
        collectButtonDownTimer = 0;
        onCollectClick(true);
    }

    public void CollectButtonUp()
    {
        collectDown = false;
    }

}
