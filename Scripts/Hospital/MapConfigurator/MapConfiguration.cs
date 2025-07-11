using UnityEngine;
using System.Collections.Generic;
using IsoEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hospital
{
    public class MapConfiguration : ScriptableObject
    {
#if UNITY_EDITOR
        [CustomEditor(typeof(MapConfiguration))]
        public class MapConfigurationEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                var data = target as MapConfiguration;
                if (data == null)
                    return;
                DrawDefaultInspector();

                if (GUILayout.Button("Parse data"))
                    data.ParseData();
            }
        }
        [UnityEditor.MenuItem("Assets/Create/MapConfigurator/Map")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<MapConfiguration>();
        }
#endif
        public Vector2i MapSize;

        public Section HospitalClinic;
        public Section Patio;
        public Section Laboratory;
        public Section MaternityWardClinic;
        public Section MaternityWardPatio;

        public List<Section> StaticAreas;

        public List<NotAvaiable> blockedArea;
        public List<NotAvaiable> fakeWallDoorBlocker;
#pragma warning disable 0649
        [SerializeField] string data;
#pragma warning restore 0649
        public int[] ImpassableTileData;

        void ParseData()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(data))
            {
                Debug.LogError("Invalid data");
                return;
            }

            string trimmedData = data.Replace("\n", "");
            string[] splitData = data.Split(';');
            if (splitData.Length == 0)
            {
                Debug.LogError("Error while splitting data");
                return;
            }

            ImpassableTileData = new int[(splitData.Length - 1) * 2];
            for (int i = 0; i < splitData.Length - 1; i++)
            {
                string[] coordinateData = splitData[i].Split('.');
                if (coordinateData.Length == 2)
                {
                    for (int j = 0; j < coordinateData.Length; j++)
                    {
                        if (ImpassableTileData[i * 2 + j] != 0)
                            Debug.LogError("About to overwrite data!" + i + " :: " + j);
                        if (!int.TryParse(coordinateData[j], out ImpassableTileData[i * 2 + j]))
                        {
                            Debug.LogError("Error when parsing coordinate data " + i + " :: " + j);
                            return;
                        }
                    }
                }
            }
            EditorUtility.SetDirty(this);
#endif
        }
    }

    [System.Serializable]
    public class NotAvaiable
    {
        public Vector2i from;
        public Vector2i size;
        public PathType pathType = PathType.Default;
    }
}
