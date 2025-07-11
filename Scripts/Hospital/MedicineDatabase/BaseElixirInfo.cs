using UnityEngine;
using System.Collections.Generic;

namespace Hospital
{
	public class BaseElixirInfo : MedicineDatabaseEntry
	{
#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/Medicine/BaseElixir")]
		public new static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<BaseElixirInfo>();
		}
#endif

		[SerializeField]
		List<Sprite> spritesForTable = null;
		[SerializeField]
		int panaceaAmount = 0;
        [SerializeField]
        GameObject bubbles = null;
        [SerializeField]
        GameObject explosion = null;
        public int PanaceaAmount
		{
			get
			{
				return panaceaAmount;
			}
		}

        public GameObject Bubbles
        {
            get
            {
                return bubbles;
            }
        }

        public GameObject Explosion
        {
            get
            {
                return explosion;
            }
        }

        public Sprite GetSpriteForTable(float percent)
		{
			if (spritesForTable.Count < 1)
				return null;
			int id =(int)(percent * (spritesForTable.Count - 1));
			if (id >= spritesForTable.Count)
				id = spritesForTable.Count - 1;
			return spritesForTable[id];
		}

	}
}
