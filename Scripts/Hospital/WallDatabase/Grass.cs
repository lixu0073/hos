using UnityEngine;
using System.Collections;

namespace Hospital
{

	public class Grass : ScriptableObject
	{
		[SerializeField]
		private WallPrefabsDatabase database = null;
#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/Walls/Grass")]
		public static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<Grass>();
		}
#endif
		[SerializeField]
		Texture grassTexture = null;

		Material material;
		private void GenerateMaterials()
		{
			material = new Material(database.materialPrefab);
            material.mainTexture = grassTexture;
		}
		public GameObject this[int i]
		{
			get
			{
				if (material == null)
					GenerateMaterials();
				var temp = GameObject.Instantiate(database.grassPrefab);
				temp.GetComponent<Renderer>().material = material;
				return temp;
			}
		}
	}
}