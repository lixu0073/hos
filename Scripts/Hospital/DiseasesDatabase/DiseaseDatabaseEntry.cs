using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hospital
{

    public class DiseaseDatabaseEntry : ScriptableObject
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/Disease/DiseaseDatabaseEntry")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<DiseaseDatabaseEntry>();
        }
#endif
        public string Name;
        public DiseaseType DiseaseType = DiseaseType.Empty;
        public Sprite DiseasePic;
        public Sprite DiseasePicSmall;
        //public bool RequiresDiagnosis = false;
    }


}
