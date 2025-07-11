using System;
using UnityEngine;
using UnityEngine.UI;

public class GiveAmountTransferer : MonoBehaviour
{
    [SerializeField] private GameState GameStateController;
    [SerializeField] private Items TypeToGive;

    private enum Items
    {
        coins,
        diamonds,
    }

    public void Start()
    {
        InputField field = GetComponentInChildren<InputField>();
        field.onEndEdit.AddListener( (value) => SubmitData(value));
    }
    
    public void SubmitData(string amount)
    {
#if MH_QA || !MH_RELEASE
        if (!String.IsNullOrEmpty(amount))
        {
            try
            {
                int rewardAmount = Int32.Parse(amount);
                switch (TypeToGive)
                {                    
                    case Items.coins:                        
                        int currentCoinsAmount = Game.Instance.gameState().GetCoinAmount();
                        GameStateController.GivePlayerCoinsDebug(rewardAmount, currentCoinsAmount, false);
                        break;                                              

                    case Items.diamonds:
                        int currentDiamondAmount = Game.Instance.gameState().GetDiamondAmount();
                        GameStateController.GivePlayerDiamondsDebug(rewardAmount, currentDiamondAmount, false);
                        break;
                }                
            }
            catch (FormatException ex)
            {
                Debug.LogError("Unable to Parse amount to transfer to Player profile: " + ex.Message);
            }
        }
#endif
    }

}
