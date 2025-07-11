using UnityEngine;
using System.Collections;

namespace Hospital
{

    public class CanInfo : ShopRoomInfo
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/RoomInfo/CanInfo")]
        public new static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<CanInfo>();
        }

#endif

        public Vector4 canColor;
        public int goldIncreasePerOwnedItem;
        public Sprite adornerImage;
        public GameObject adornerParticles;
        public int colorTexureID = 0;
    }
}