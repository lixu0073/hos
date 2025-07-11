using UnityEngine;

public class CommunityButton : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    GameObject rewardImage;
#pragma warning restore 0649

    public void SetRewardImageActive(bool setActive)
    {
        rewardImage.SetActive(setActive);
    }
	
}
