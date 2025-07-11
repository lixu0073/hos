using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Maternity.UI
{
    public class MaternityCurePanelUI : MonoBehaviour
    {

        [SerializeField]
        private ButtonUI cureButton = null;

        [SerializeField]
        private TextMeshProUGUI expText = null;
        
        public void SetCurePanelActive(bool setActive)
        {
            gameObject.SetActive(setActive);
        }

        public void SetCurePanel(string exp, UnityAction cureButtonAction, string cureButtonText, Sprite buttonIcon = null, bool isBlinking = false)
        {
            expText.text = exp;
            SetCureButton(cureButtonAction, cureButtonText, buttonIcon, isBlinking);
        }

        public void SetCureButton(UnityAction cureButtonAction, string cureButtonText, Sprite cureButtonIcon, bool isBlinking = false)
        {
            cureButton.SetButton(cureButtonAction, cureButtonText, cureButtonIcon, isBlinking);
        }

        public void SetCureButtonInteractive(bool isInteractive)
        {
            cureButton.SetButtonGrayscale(isInteractive);
        }

        public Vector3 GetExpSource()
        {
            return cureButton.transform.position;
        }
    }
}
