using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftRewardDevTesting : MonoBehaviour
{
    public void TestGIftRewardGenerator()
    {
#if UNITY_EDITOR
        GiftsReceiveController.Instance.TestRandomGift();
#endif
    }
}
