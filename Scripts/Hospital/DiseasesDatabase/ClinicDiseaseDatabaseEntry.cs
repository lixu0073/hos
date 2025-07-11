using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hospital
{

    public class ClinicDiseaseDatabaseEntry : ScriptableObject
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/Disease/ClinicDiseaseDatabaseEntry")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<ClinicDiseaseDatabaseEntry>();
        }
#endif

        public int id;
        public string Name;
        public Sprite DiseasePic;
        public DoctorRoomInfo Doctor;
    }
}
