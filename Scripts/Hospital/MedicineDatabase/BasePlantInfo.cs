using UnityEngine;
using System.Collections.Generic;

namespace Hospital
{
	public class BasePlantInfo : MedicineDatabaseEntry
	{
#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/Medicine/BasePlant")]
		public new static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<BasePlantInfo>();
		}
#endif

		[SerializeField]
		Sprite deadPlant = null;

		[SerializeField]
		List<Sprite> spritesForPlant = null;

		[SerializeField]
		int price = 0;

		[SerializeField]
		int productionAmount = 0;

        [SerializeField]
        int helpExp = 15;

        [SerializeField]
        GameObject explosion = null;

        [SerializeField] private LifeKingdom lifeKingdom = LifeKingdom.Plant;

		public LifeKingdom Kingdom{
			get{ 
				return lifeKingdom;
			}
		}

		public int Price
		{
			get
			{
				return price;
			}
		}

		public int ProductionAmount
		{
			get
			{
				return productionAmount;
			}
		}

        public int HelpExp
        {
            get
            {
                return helpExp;
            }
        }

        public Sprite GetSpriteForPlant(float percent)
		{
			if (spritesForPlant.Count < 1)
				return null;
			int id = (int)(percent * (spritesForPlant.Count - 1));
			if (id >= spritesForPlant.Count)
				id = spritesForPlant.Count - 1;
			return spritesForPlant[id];
		}

		public Sprite GetDeadSpriteForPlant()
		{
			return deadPlant;
		}

        public GameObject Explosion
        {
            get
            {
                return explosion;
            }
        }

        public enum LifeKingdom{
			Plant,
			Fungi
		}
	}
}
