using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Hospital;

public class TreatmentFriendPanel : MonoBehaviour {
    
    public Toggle selectFriendToggle = null;

    [SerializeField]
    private ProviderFriendCardController friendController = null;

    public void SetTreatmentFriendPanel(IFollower provider, UnityAction<bool> onToggle) {
        friendController.Initialize(provider, VisitingEntryPoint.TreatmentSendPush);
        SetSelectFriendToggleOnToggleAction(onToggle);
    }

    private void SetSelectFriendToggleOnToggleAction(UnityAction<bool> onToggle) {
        if (selectFriendToggle == null) {
            Debug.LogError("selectFriendToggle is null");
            return;
        }
        selectFriendToggle.onValueChanged.RemoveAllListeners();
        selectFriendToggle.onValueChanged.AddListener((selected) =>
        {
            onToggle(selected);
            UIController.PlayClickSoundSecure(selectFriendToggle.gameObject);
        });
    }
}
