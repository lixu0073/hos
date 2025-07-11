using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace Hospital
{
    public class AdvancedInfo : MonoBehaviour
    {

        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI ToggleButtonText;
        public Button ToggleButton;
        public GameObject Check;

        private IAdvancedInfoDataHolder data;

        public void SetData(IAdvancedInfoDataHolder group)
        {
            data = group;
            Initialize();
        }

        private void Initialize()
        {
            TitleText.text = data.GetDescription();
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            ToggleButtonText.text = data.IsAdvancedOptionActive() ? I2.Loc.ScriptLocalization.Get("ON") : I2.Loc.ScriptLocalization.Get("OFF");
            Check.SetActive(data.IsAdvancedOptionActive());
        }


        public void OnToggleButtonClick()
        {
            data.ToggleSettings();
            data.OnDataChange();
            UpdateButtonState();
        }
        
    }
}
