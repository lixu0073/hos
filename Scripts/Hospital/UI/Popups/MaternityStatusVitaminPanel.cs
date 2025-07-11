using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaternityStatusVitaminPanel : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> VitaminSubPanel = null;
#pragma warning disable 0649
    [SerializeField]
    private List<TextMeshProUGUI> VitaminRequiredAmountText;
    [SerializeField]
    private List<Button> buyForDiamondButtons;
    [SerializeField]
    private List<TextMeshProUGUI> diamondCostAmount;
    [SerializeField]
#pragma warning restore 0649

    public void InitializeData(MaternityStatusVitaminPanelData data)
    {
        int vitaminCounter = 0;
        gameObject.SetActive(true);
        for (int i = 0; i < data.vitaminAmountRequired.Length; i++)
        {
            if (data.vitaminAmountRequired[i] <= 0)
            {
                VitaminSubPanel[i].SetActive(false);
            }
            else
            {
                VitaminSubPanel[i].SetActive(true);
                vitaminCounter++;
                VitaminRequiredAmountText[i].text = System.String.Format("X{0}", data.vitaminAmountRequired[i]);
                if (data.diamondCost[i] <= 0)
                {
                    buyForDiamondButtons[i].gameObject.SetActive(false);
                }
                else
                {
                    buyForDiamondButtons[i].gameObject.SetActive(true);
                    buyForDiamondButtons[i].RemoveAllOnClickListeners();
                    buyForDiamondButtons[i].onClick.AddListener(data.buyForDiamondsButtonActions[i]);
                    diamondCostAmount[i].text = data.diamondCost[i].ToString();
                }
            }
        }
        if (vitaminCounter == 0)
        {
            gameObject.SetActive(false);
        }
    }
}
