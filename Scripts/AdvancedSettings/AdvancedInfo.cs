using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace Hospital
{
    /// <summary>
    /// 高级信息显示与控制类，用于在UI中展示和管理一个高级设置选项。
    /// </summary>
    public class AdvancedInfo : MonoBehaviour
    {

        /// <summary>
        /// 标题文本，用于显示高级设置选项的描述。
        /// </summary>
        public TextMeshProUGUI TitleText;
        /// <summary>
        /// 切换按钮的文本，显示“开”或“关”。
        /// </summary>
        public TextMeshProUGUI ToggleButtonText;
        /// <summary>
        /// 切换设置的按钮。
        /// </summary>
        public Button ToggleButton;
        /// <summary>
        /// 选中状态的GameObject，通常是一个勾选标记。
        /// </summary>
        public GameObject Check;

        private IAdvancedInfoDataHolder data;

        /// <summary>
        /// 设置高级信息的数据源。
        /// </summary>
        /// <param name="group">实现IAdvancedInfoDataHolder接口的数据持有者。</param>
        public void SetData(IAdvancedInfoDataHolder group)
        {
            data = group;
            Initialize();
        }

        /// <summary>
        /// 初始化UI元素，根据数据源设置标题文本和按钮状态。
        /// </summary>
        private void Initialize()
        {
            TitleText.text = data.GetDescription();
            UpdateButtonState();
        }

        /// <summary>
        /// 更新切换按钮的显示状态（文本和勾选标记）。
        /// </summary>
        private void UpdateButtonState()
        {
            ToggleButtonText.text = data.IsAdvancedOptionActive() ? I2.Loc.ScriptLocalization.Get("ON") : I2.Loc.ScriptLocalization.Get("OFF");
            Check.SetActive(data.IsAdvancedOptionActive());
        }

        /// <summary>
        /// 切换按钮点击事件处理。
        /// 切换设置状态，通知数据改变，并更新UI显示。
        /// </summary>
        public void OnToggleButtonClick()
        {
            data.ToggleSettings();
            data.OnDataChange();
            UpdateButtonState();
        }
        
    }
}
