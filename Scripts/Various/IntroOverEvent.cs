using UnityEngine;

namespace Hospital
{
    public class IntroOverEvent : MonoBehaviour
    {
        LoadingGame loading;

        void Start()
        {
            loading = FindObjectOfType<LoadingGame>();
        }

        public void IntroOver()
        {
            if(loading == null)
                loading = FindObjectOfType<LoadingGame>();

            CanvasScalerAdjuster.BalanceWidthHeight(0.0f); // Changes Canvas Scaler Match balance between Width and Heigh

            if (loading)
                loading.IntroOver();
        }
    }
}
