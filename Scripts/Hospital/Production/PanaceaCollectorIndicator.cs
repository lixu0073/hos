using UnityEngine;

namespace Hospital
{
    public class PanaceaCollectorIndicator : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] GameObject plusOneTxt;
#pragma warning restore 0649

        public void Show(bool isForLvlUP)
        {
            plusOneTxt.SetActive(!plusOneTxt);
        }
    }
}
