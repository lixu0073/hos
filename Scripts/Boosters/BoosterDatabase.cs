using UnityEngine;
using System.Collections;
using System.Text;
using System;
using System.Globalization;

public class BoosterDatabase : ScriptableObject {
	#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Create/BoosterDatabase")]
	public static void CreateAsset()
	{
		ScriptableObjectUtility.CreateAsset<BoosterDatabase>();
	}
	#endif

	public Booster[] boosters;

	[System.Serializable]
	public class Booster
	{
        public enum BoosterID
        {
            DoctorCoins,
            HospitalCoins,
            BedCoins,
            HappyMinute,
            DoctorExp,
            HospitalExp,
            BedExp,
            HospitalCoinsAndExp,
            HappyHour
        }

		public string shortInfo = "";
		public string info = "";
		public int price = 1;
		public int duration = 3600;
		public float modifier = 1;
		public Sprite icon = null;
		public BoosterType boosterType = BoosterType.Coin;
		public BoosterTarget boosterTarget = BoosterTarget.PatientCard;
        public bool canBuy = true;


        public int Price 
        {
            get
            {
                int boosterPrice = DefaultConfigurationProvider.GetConfigCData().BoostersPrice[shortInfo];
                return boosterPrice == -1 ? price : boosterPrice;
            }
        }

		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.Append (Price.ToString());
			builder.Append ("?");
			builder.Append (duration.ToString());
			builder.Append ("?");
			builder.Append (modifier.ToString ());
			builder.Append ("?");
			builder.Append (boosterType.ToString ());
			builder.Append ("?");
			builder.Append (boosterTarget.ToString ());
			return builder.ToString ();
		}

		public static Booster Parse (string toParse){
			if (string.IsNullOrEmpty (toParse))
				return null;
			var toParseArr = toParse.Split ('?');
			BoosterDatabase.Booster booster = new Booster();
			booster.price = int.Parse (toParseArr [0]);
			booster.duration = int.Parse (toParseArr[1]);
			booster.modifier = float.Parse (toParseArr [2], CultureInfo.InvariantCulture);
            booster.boosterType = (BoosterType)Enum.Parse (typeof(BoosterType), toParseArr[3]);
			booster.boosterTarget = (BoosterTarget)Enum.Parse (typeof(BoosterTarget), toParseArr[4]);
			return booster;
		}
	}
		
}
