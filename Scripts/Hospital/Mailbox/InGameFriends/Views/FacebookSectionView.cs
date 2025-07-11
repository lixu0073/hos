using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Hospital
{
    public class FacebookSectionView : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Button connectFacebook;
        [SerializeField] private TextMeshProUGUI connectFacebookText;
        [SerializeField] private GameObject RewardGameobject;
#pragma warning restore 0649

        public UnityAction FacebookConnect
        {
            set
            {
                connectFacebook.RemoveAllOnClickListeners();
                connectFacebook.onClick.AddListener(value);
            }
        }

        public void SetReawardGameobjectActive(bool isActive)
        {
            RewardGameobject.SetActive(isActive);
        }

        public void SetConnectFacebookButtonActive(bool value)
        {
            connectFacebook.gameObject.SetActive(value);
        }
    }
}