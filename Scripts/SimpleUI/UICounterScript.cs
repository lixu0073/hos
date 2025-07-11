using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace SimpleUI
{
    [RequireComponent(typeof(RectTransform))]
    public class UICounterScript : MonoBehaviour
    {
        public TextMeshProUGUI valueText;
        public int value = 0;
        public void Start()
        {
            SetValue(value);
            print(transform.position);
        }

        public void SetValue(int fill)
        {
            valueText.text = fill.ToString();

        }
        public void AddToCounter(int addition, int ammountBeforeAddition)
        {
            SetValue(ammountBeforeAddition + addition);
        }
        public void SubstractFromCounter(int substraction, int ammountBeforeSubstraction)
        {
            SetValue(ammountBeforeSubstraction - substraction);
        }

    }

}
