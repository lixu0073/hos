using UnityEngine;
using System.Collections.Generic;

namespace Hospital
{

    public class CoverInfo : BaseRoomInfo
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/RoomInfo/CoverInfo")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<CoverInfo>();
        }

#endif
        
        public List<CoverSpawnInfo> SpawnedObjects;
    }
    [System.Serializable]
    public class CoverSpawnInfo
    {
        public string tag;
        public IsoEngine.Vector2i pos;
        public Rotation rot;
        public string settings = "";
    }
}