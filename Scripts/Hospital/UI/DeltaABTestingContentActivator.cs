using System;
using System.Collections.Generic;
using UnityEngine;

public class DeltaABTestingContentActivator : MonoBehaviour, ITabControllerClient
{

    public enum ABTestingSource
    {
        DailyReward,
        IAPShop,
    }
#pragma warning disable 0649
    //Objects has to be assignet in the same order as Delta Config Enum list
    [SerializeField]
    List<GameObject> ABVariantsGameObjects;
    [SerializeField]
    ABTestingSource testingSource;
#pragma warning restore 0649
    List<IPopupInicializer> ABVariantsInitializators;

    private int activationIndex = -1;

    private void Awake()
    {
        ABVariantsInitializators = new List<IPopupInicializer>();
    }

    private void OnEnable()
    {
        if (activationIndex == -1)
        {
            switch (testingSource)
            {
                case ABTestingSource.DailyReward:
                    activationIndex = 0;
                    break;
                default:
                    activationIndex = 0;
                    break;
            }

            ABVariantsInitializators.Clear();
            foreach (GameObject abVariantGO in ABVariantsGameObjects)
            {
                ABVariantsInitializators.Add(abVariantGO.GetComponent<IPopupInicializer>());
            }
        }
    }

    public void SetTabContentActive(Action onOpen, Action onFailOpen)
    {
        ABVariantsInitializators[activationIndex].Initialize(onOpen, onFailOpen);
    }

    public void DeactiveTabContent()
    {
        ABVariantsInitializators[activationIndex].DeInitialize();
    }
}