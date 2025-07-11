using System.Collections.Generic;
using UnityEngine;

public class VitaminIndicatorUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    List<GameObject> indicators;
    [SerializeField]
    GameObject lockedVitaminLight;
    [SerializeField]
    GameObject unlockedVitaminLight;
    [SerializeField]
    GameObject fullCollectorLight;
#pragma warning restore 0649

    public void UpdateIndicator(int levelOfFill)
    {
        for (int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
        }

        switch (levelOfFill)
        {
            case 1:
                indicators[0].SetActive(true);
                break;
            case 2:
                indicators[0].SetActive(true);
                indicators[1].SetActive(true);
                break;
            case 3:
                indicators[0].SetActive(true);
                indicators[1].SetActive(true);
                indicators[2].SetActive(true);
                break;
            default:
                break;
        }
    }

    public void ToggleMachineLight(bool isFull)
    {
        unlockedVitaminLight.SetActive(!isFull);
        fullCollectorLight.SetActive(isFull);
    }

    public void UnlockMachine()
    {
        lockedVitaminLight.SetActive(false);
        unlockedVitaminLight.SetActive(true);
    }
}
