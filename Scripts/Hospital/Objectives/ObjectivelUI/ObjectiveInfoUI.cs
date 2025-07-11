using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ObjectiveInfoUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] TextMeshProUGUI progressValText;
    [SerializeField] TextMeshProUGUI rewardValText;
    [SerializeField] Image rewardImage;
#pragma warning restore 0649
    [SerializeField] private GameObject checkmark = null;

    //Objective objective;

    public void Setup(Objective objective)
    {
        //this.objective = objective;

        //image.sprite = this.objective.Reward.GetSprite();
        description.text = objective.GetInfoDescription();
        if (objective.Progress < objective.ProgressObjective)
        {
            checkmark.SetActive(false);
            progressValText.gameObject.SetActive(true);
            progressValText.text = objective.Progress.ToString() + "/" + objective.ProgressObjective;
        }
        else
        {
            checkmark.SetActive(true);
            progressValText.gameObject.SetActive(false);
        }
        rewardValText.text = objective.Reward.Amount.ToString();
    }
}
