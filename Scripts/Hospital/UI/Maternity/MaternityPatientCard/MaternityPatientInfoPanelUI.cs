using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Maternity.Adapter;

namespace Maternity.UI
{
    public class MaternityPatientInfoPanelUI : MonoBehaviour
    {
        [SerializeField]
        protected GameObject nameLabel = null;
        [SerializeField]
        protected GameObject aboutLabel = null;

        [SerializeField]
        protected PatientAvatarUI avatar = null;

        [SerializeField]
        protected TextMeshProUGUI nameText = null;
        [SerializeField]
        protected TextMeshProUGUI aboutText = null;

        public void SetPatientInfoPanelActive(bool setActive)
        {
            gameObject.SetActive(setActive);
        }

        //Szmury reove background type
        public virtual void SetPatientInfoView(MaternityPatientInfo info)
        {
            nameText.text = info.name;
            aboutText.text = info.about;
            avatar.SetAvatarView(info.head, info.body, info.gender);

            SetInfoObjectsActive(true);
        }


        protected void SetInfoObjectsActive(bool setActive)
        {
            avatar.SetPatientAvatarActive(setActive);
            nameLabel.SetActive(setActive);
            aboutLabel.SetActive(setActive);
            nameText.gameObject.SetActive(setActive);
            aboutText.gameObject.SetActive(setActive);
        }
    }
}
