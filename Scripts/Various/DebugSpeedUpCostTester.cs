using UnityEngine;
using UnityEngine.UI;

public class DebugSpeedUpCostTester : MonoBehaviour {

    public Text output;
    public bool isBuilding;
    public float baseTime;
    public float timeRemaining;

    public int calculatedCost;


    void Update() {
        //GetCost();
    }

    public void GetCost() {
        if (isBuilding) {
            calculatedCost =  DiamondCostCalculator.GetCostForBuilding(timeRemaining, baseTime);
        } else {
            calculatedCost = DiamondCostCalculator.GetCostForAction(timeRemaining, baseTime);
        }

        output.text = calculatedCost.ToString();
    }

}
