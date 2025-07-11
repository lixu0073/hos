using UnityEngine;
using System.Collections.Generic;
namespace Hospital
{
    public class WallDatabaseEntry : ScriptableObject
    {

        [SerializeField]
        WallPrefabsDatabase database = null;
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/Walls/WallDatabaseEntry")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<WallDatabaseEntry>();
        }
#endif
        Material[] materials = null;
        [SerializeField] bool isGlass = false;
        [SerializeField]
        Texture left = null;
        [SerializeField]
        Texture right = null;
        [SerializeField]
        Texture leftCorner = null;
        [SerializeField]
        Texture rightCorner = null;
        [SerializeField]
        Texture outerCornerL = null;
        [SerializeField]
        Texture outerCornerR = null;
        [SerializeField]
        Texture innerCornerL = null;
        [SerializeField]
        Texture innerCornerR = null;
        [SerializeField]
        List<LongWallItemPref> windowPrefabs = null;
        List<List<Texture>>[] windowMaterials;

        public int CheckWindowLenght(int windowID)
        {
            return windowPrefabs[windowID].list.Count;
        }

        private void GenerateMaterials()
        {
            //materials = new Material[8];
            //if (!isGlass)
            //{
            //    materials[0] = new Material(database.materialPrefab);
            //    materials[1] = new Material(database.materialPrefab);
            //    for (int i = 2; i < materials.Length; i++)//why?
            //    {
            //        materials[i] = /*new Material(*/database.windowMaterial;//);
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < materials.Length; i++)
            //    {
            //        materials[i] = new Material(database.glassWallMaterial);
            //    }
            //}
            //materials[0].mainTexture = right;
            if (isGlass)
            {
                //Debug.Log ("TexAdded");
            }
            //materials[1].mainTexture = left;
            //materials[2].mainTexture = leftCorner;
            //materials[3].mainTexture = rightCorner;
            //materials[4].mainTexture = outerCornerR;
            //materials[5].mainTexture = outerCornerL;
            //materials[6].mainTexture = innerCornerL;
            //materials[7].mainTexture = innerCornerR;

            windowMaterials = new List<List<Texture>>[2];
            windowMaterials[0] = new List<List<Texture>>();
            windowMaterials[1] = new List<List<Texture>>();

            foreach (var p in windowPrefabs)
            {
                var lef = new List<Texture>();
                var rig = new List<Texture>();
                //p.list.Reverse();
                foreach (var z in p.list)
                {
                    //var l = /*new Material(*/database.windowMaterial;//);
                    //var r = /*new Material(*/database.windowMaterial;//);
                    //l.mainTexture = z.right;
                    //r.mainTexture = z.left;
                    lef.Add(z.right);
                    rig.Add(z.left);
                }
                lef.Reverse();
                rig.Reverse();
                windowMaterials[0].Add(lef);
                windowMaterials[1].Add(rig);

            }
        }
        public GameObject this[wallType type, int windowID = 0, int windowElement = 0]
        {
            get
            {
                GameObject temp = null;
                if (materials == null || materials.Length < 8 || materials[0] == null)
                    GenerateMaterials();
                Renderer renderer;
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                switch (type)
                {
                    case wallType.left:
                        temp = GameObject.Instantiate(database.left);
                        renderer = temp.GetComponent<Renderer>();
                        renderer.material = (this.isGlass) ? database.glassWallMaterial : database.materialPrefab;
                        renderer.GetPropertyBlock(block);
                        if (right != null)
                            block.SetTexture("_MainTex", right);
                        renderer.SetPropertyBlock(block);
                        break;
                    case wallType.right:
                        //temp = GameObject.Instantiate(database.right);
                        //temp.GetComponent<Renderer>().material = materials[1];
                        temp = GameObject.Instantiate(database.right);
                        renderer = temp.GetComponent<Renderer>();
                        renderer.material = (this.isGlass) ? database.glassWallMaterial : database.materialPrefab;
                        renderer.GetPropertyBlock(block);
                        if (left != null)
                            block.SetTexture("_MainTex", left);
                        renderer.SetPropertyBlock(block);
                        break;
                    case wallType.leftCorner:
                        //temp = GameObject.Instantiate(database.leftCorner);
                        //temp.transform.GetChild(0).gameObject.GetComponent<Renderer>().material = materials[2];
                        temp = GameObject.Instantiate(database.leftCorner);
                        renderer = temp.transform.GetChild(0).GetComponent<Renderer>();
                        renderer.material = (this.isGlass) ? database.glassWallMaterial : database.windowMaterial;
                        renderer.GetPropertyBlock(block);
                        if (leftCorner != null)
                            block.SetTexture("_MainTex", leftCorner);
                        renderer.SetPropertyBlock(block);
                        break;
                    case wallType.rightCorner:
                        //temp = GameObject.Instantiate(database.rightCorner);
                        //temp.transform.GetChild(0).gameObject.GetComponent<Renderer>().material = materials[3];
                        temp = GameObject.Instantiate(database.rightCorner);
                        renderer = temp.transform.GetChild(0).GetComponent<Renderer>();
                        renderer.material = (this.isGlass) ? database.glassWallMaterial : database.windowMaterial;
                        renderer.GetPropertyBlock(block);
                        if (rightCorner != null)
                            block.SetTexture("_MainTex", rightCorner);
                        renderer.SetPropertyBlock(block);
                        break;
                    case wallType.outerCorner:
                        temp = GameObject.Instantiate(database.outerCorner);
                        // Right
                        renderer = temp.transform.GetChild(1).gameObject.GetComponent<Renderer>();
                        renderer.material = (this.isGlass) ? database.glassWallMaterial : database.windowMaterial;
                        renderer.GetPropertyBlock(block);
                        if (outerCornerR != null)
                            block.SetTexture("_MainTex", outerCornerR);
                        renderer.SetPropertyBlock(block);
                        // Left
                        renderer = temp.transform.GetChild(0).gameObject.GetComponent<Renderer>();
                        renderer.material = (this.isGlass) ? database.glassWallMaterial : database.windowMaterial;
                        renderer.GetPropertyBlock(block);
                        if (outerCornerL != null)
                            block.SetTexture("_MainTex", outerCornerL);
                        renderer.SetPropertyBlock(block);
                        break;
                    case wallType.innerCorner:
                        temp = GameObject.Instantiate(database.innerCorner);
                        // Left
                        renderer = temp.transform.GetChild(1).gameObject.GetComponent<Renderer>();
                        renderer.material = (this.isGlass) ? database.glassWallMaterial : database.windowMaterial;
                        renderer.GetPropertyBlock(block);
                        if (innerCornerL != null)
                            block.SetTexture("_MainTex", innerCornerL);
                        renderer.SetPropertyBlock(block);
                        // Right
                        renderer = temp.transform.GetChild(0).gameObject.GetComponent<Renderer>();
                        renderer.material = (this.isGlass) ? database.glassWallMaterial : database.windowMaterial;
                        renderer.GetPropertyBlock(block);
                        if (innerCornerR != null)
                            block.SetTexture("_MainTex", innerCornerR);
                        renderer.SetPropertyBlock(block);
                        break;
                    case wallType.leftWindow:
                        temp = GameObject.Instantiate(database.left);
                        renderer = temp.GetComponent<Renderer>();
                        renderer.material = database.windowMaterial;
                        renderer.GetPropertyBlock(block);
                        block.SetTexture("_MainTex", windowMaterials[0][windowID][windowElement]);
                        renderer.SetPropertyBlock(block);
                        break;
                    case wallType.rightWindow:
                        temp = GameObject.Instantiate(database.right);
                        renderer = temp.GetComponent<Renderer>();
                        renderer.material = database.windowMaterial;
                        renderer.GetPropertyBlock(block);
                        block.SetTexture("_MainTex", windowMaterials[1][windowID][windowElement]);
                        renderer.SetPropertyBlock(block);
                        break;
                    default:
                        break;
                }

                if (this.isGlass)
                    temp.transform.GetChild(0).gameObject.SetActive(false);
                return temp;
            }
        }
    }
}