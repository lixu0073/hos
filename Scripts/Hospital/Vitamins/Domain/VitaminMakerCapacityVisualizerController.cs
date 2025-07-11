using System.Collections.Generic;
using UnityEngine;

public class VitaminMakerCapacityVisualizerController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    List<VitaminIndicatorUI> UIIndicators;
#pragma warning restore 0649

    public void ToggleCapacityIndicator(int collectorIndex, int levelOfFill)
    {
        UIIndicators[collectorIndex].UpdateIndicator(levelOfFill);
    }

    public void UnlockMachine(int collectorIndex)
    {
        UIIndicators[collectorIndex].UnlockMachine();
    }

    public void SetFullMachineLight(int collectorIndex, bool isFull)
    {
        UIIndicators[collectorIndex].ToggleMachineLight(isFull);
    }
}
