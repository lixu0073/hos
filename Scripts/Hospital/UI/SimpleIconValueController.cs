using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SimpleUI
{
    public class SimpleIconValueController : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI valueText = null;
        [SerializeField]
        private Image icon = null;

        public void SetIconValueObject(float value, ValueFormat format, IconType iconType) {
            switch (format)
            {
                case ValueFormat.simple:
                    SetIconValueSimple(value, iconType);
                    break;
                case ValueFormat.simpleSigned:
                    SetIconValueSimpleSigned(value, iconType);
                    break;
                case ValueFormat.percent:
                    SetIconValuePercent(value, iconType);
                    break;
                default:
                    Debug.LogError("format not implemented");
                    break;
            }
        }

        public void SetIconValueObject(string valueText, IconType iconType)
        {
            SetValueText(valueText);
            SetIcon(GetIconOfType(iconType));
        }

        private void SetIconValueSimple(float value, IconType iconType) {
            SetIcon(GetIconOfType(iconType));
            SetValueText(value.ToString());
        }

        private void SetIconValueSimpleSigned(float value, IconType iconType)
        {
            SetIcon(GetIconOfType(iconType));

            if (value == 0)
            {
                SetIconValueSimple(value, iconType);
                return;
            }
            SetValueText(((value > 0) ? "+ " : "- ") + Mathf.Abs(value).ToString());
        }

        private void SetIconValuePercent(float value, IconType iconType) {
            SetIcon(GetIconOfType(iconType));

            if (value == 0) {
                SetIconValueSimple(value, iconType);
                return;
            }
            SetValueText(((value > 0) ? "+ " : "- ") + Mathf.Abs(value).ToString() + "%");
        }

        private void SetValueText(string text) {
            if (valueText == null)
            {
                Debug.LogError("valueText is null");
                return;
            }

            if (string.IsNullOrEmpty(text)) {
                valueText.gameObject.SetActive(false);
                return;
            }

            valueText.gameObject.SetActive(true);
            valueText.text = text;
        }

        private void SetIcon(Sprite iconToSet) {
            if (icon == null) {
                Debug.LogError("icon is null");
                return;
            }

            if (iconToSet == null)
            {
                icon.gameObject.SetActive(false);
                return;
            }

            icon.gameObject.SetActive(true);
            icon.sprite = iconToSet;
        }

        private Sprite GetIconOfType(IconType type) {
            switch (type)
            {
                case IconType.coin:
                    return ResourcesHolder.Get().coinSprite;
                case IconType.diamond:
                    return ResourcesHolder.Get().diamondSprite;
                case IconType.exp:
                    return ResourcesHolder.Get().expSprite;
                case IconType.positiveEnergy:
                    return ResourcesHolder.Get().PESprite;
                case IconType.time:
                    return ResourcesHolder.Get().timeSprite;
                default:
                    Debug.LogError("IconType not implemented");
                    return null;
            }
        }


        public enum ValueFormat {
            simple,
            simpleSigned,
            percent
        }

        public enum IconType {
            coin,
            diamond,
            exp,
            positiveEnergy,
            time
        }
    }
}