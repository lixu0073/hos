using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DiamondTransactionController : MonoBehaviour
{
    public static DiamondTransactionController Instance { get { return instance; } }


    private static DiamondTransactionController instance = null;
    private DiamondTransaction curentlyPendingDiamondTransaction;
    private Coroutine timer;
    private const int TRANSACTION_LIFESPAN = 2;
    private const string TRANSACTION_REPETITION_LOCKIT = "CONFIRM_DIAMOND_SPEND";
    public const string PLAYER_PREFS_ACTIVATION_BOOL_NAME = "DiamondTransactionSystemActivated";
    private bool isSystemRunning = false;
    private int diamondLimitFromStroingTransaction;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void Initialize()
    {
        diamondLimitFromStroingTransaction = DefaultConfigurationProvider.GetConfigCData().DiamondLimitToShowSpendConfirmation;
        isSystemRunning = PlayerPrefs.GetInt(PLAYER_PREFS_ACTIVATION_BOOL_NAME, 0) == 1 ? true : false;
    }

    public void ToggleSystem(bool turnOnSystem)
    {
        isSystemRunning = turnOnSystem;
    }

    public void AddDiamondTransaction(int diamondAmountToWithdraw, Action onSuccess, IDiamondTransactionMaker provider)
    {
        if (provider.GetID() != Guid.Empty && (isSystemRunning && diamondAmountToWithdraw >= diamondLimitFromStroingTransaction))
        {
            if (curentlyPendingDiamondTransaction == null || curentlyPendingDiamondTransaction.ID != provider.GetID())
            {
                SetupNewTransaction(onSuccess, provider.GetID());
            }
            else if (curentlyPendingDiamondTransaction.ID == provider.GetID())
            {
                curentlyPendingDiamondTransaction.FinalizeTransaction();
                DeleteTransaction();
            }
        }
        else
        {
            if (provider.GetID() == Guid.Empty)
            {
                Debug.LogError("Because ID is empty");
            }
            onSuccess?.Invoke();
        }
    }

    public void ActivateSystem(bool activate)
    {
        isSystemRunning = activate;
    }

    private void SetupNewTransaction(Action onSuccess, Guid key)
    {
        curentlyPendingDiamondTransaction = new DiamondTransaction(onSuccess, key);
        StartTimer();
        MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get(TRANSACTION_REPETITION_LOCKIT));
    }

    private IEnumerator CountTimer()
    {
        float timePassed = 0;
        while (timePassed < TRANSACTION_LIFESPAN)
        {
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        DeleteTransaction();
    }

    public void DeleteTransaction()
    {
        StopTimer();
        curentlyPendingDiamondTransaction = null;
    }

    private void StartTimer()
    {
        if (timer != null)
        {
            StopTimer();
        }
        timer = StartCoroutine(CountTimer());
    }

    private void StopTimer()
    {
        try { 
            if (timer != null)
            {
                StopCoroutine(timer);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            DeleteTransaction();
        }
    }
}
