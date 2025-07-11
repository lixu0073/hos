using UnityEngine;

namespace Hospital 
{
	public class VIPDatabase : ScriptableObject {
		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/VIPDatabase")]
		public static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<VIPDatabase>();
		}
		#endif

		public VIP_gender[] VIPgender;

		[System.Serializable]
		public class VIP_gender{
			public string gender;
			public VIP_race[] VIPrace;

		}

		[System.Serializable]
		public class VIP_race{
			public string race;
			public VIP_BIO[] VIPbio;
		}

		[System.Serializable]
		public class VIP_BIO
		{
			public VIP_info VIPinfo;
			public VIP_appearance VIPappearance;

		}

		[System.Serializable]
		public class VIP_info
		{
			[SerializeField] public string name;
			[SerializeField] public string surname;
            [SerializeField] public int description;        //description text is taken from lockit tab: VIP_BIOS, tag VIP_{this int}
			[SerializeField] public int age;
			[SerializeField] public int gender;
            [SerializeField] public int race;
            [SerializeField] public BloodType bloodType;
            [SerializeField] public Sprite avatarHead;
			[SerializeField] public Sprite avatarBody;

		}

		[System.Serializable]
		public class VIP_appearance
		{
			public Sprite headFront;
			public Sprite headBack;
			public Sprite torsoFront;
			public Sprite torsoBack;
			public Sprite Arm;
			public Sprite HandFrontL;
			public Sprite HandFrontR;
			public Sprite HandBack;
			public Sprite lowerBodyFront;
			public Sprite lowerBodyBack;
			public Sprite UpperLeg;
			public Sprite LowerLeg;
			public Sprite FootFront;
			public Sprite FootBack;
		}
	}
}
