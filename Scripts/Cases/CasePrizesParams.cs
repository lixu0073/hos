using UnityEngine;
using System.Collections;
using System.Text;
using System;

public class CasePrizesParams : ScriptableObject {
	#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Create/CasesPrizeparams")]
	public static void CreateAsset()
	{
		ScriptableObjectUtility.CreateAsset<CasePrizesParams>();
	}
	#endif


	[Header("Coin Ranges")]
	public Vector2[] coinRanges = new Vector2[3]; 

	[Header("Diamond Ranges")]
	public Vector2[] diamondRanges = new Vector2[3];

    [Header("Positive Energy Ranges")]
    public Vector2[] positiveEnergyRanges = new Vector2[3];

    [Header("ItemAmount")]
	public int[] itemAmount = new int[3];

	[Header("Decorations Amount")]
	public int[] decoAmount = new int[3];
		
	[Header("BoosterProbabilities")]
	public BoosterProbabilities[] boosterProbabilities = new BoosterProbabilities[3];


	[System.Serializable]
	public class BoosterProbabilities{
		public float[] boosterProbability = new float[3];
	}



}