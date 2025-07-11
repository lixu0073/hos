using UnityEngine;
using System.Collections;

namespace Hospital
{

    public class RemovableDecorationInfo : ShopRoomInfo
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/RoomInfo/RemovableDecorationInfo")]
        public new static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<RemovableDecorationInfo>();
        }
#endif
        [SerializeField]
        public IsoEngine.IsoObjectPrefabData spawneObjectWhenRemoved = null;

    }
}