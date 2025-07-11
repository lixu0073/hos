using UnityEngine;
using System.Collections.Generic;


public class BubbleBoyDatabase : ScriptableObject
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/BubbleBoyDatabase")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<BubbleBoyDatabase>();
    }
#endif

    [SerializeField] public int bestWonItemLifeTime;
#pragma warning disable 0649
    [SerializeField] List<BubbleOpenFee> bubbleBoyEntryFee;
    [SerializeField] List<BubbleOpenFee> bubbleOpenFee;
    [SerializeField] List<ShopRoomInfo> decorationsReward;
    [SerializeField] List<int> coinsReward;
#pragma warning restore 0649

    public BubbleOpenFee GetEntryFee(int index)
    {
        if (bubbleBoyEntryFee != null && bubbleBoyEntryFee.Count > 0)
        {
            if (index >= bubbleBoyEntryFee.Count)
                return bubbleBoyEntryFee[bubbleBoyEntryFee.Count - 1];

            for (int i = 0; i < bubbleBoyEntryFee.Count; ++i)
            {
                if (i >= index) return bubbleBoyEntryFee[index];
            }
        }
        return null;
    }

    public BubbleOpenFee GetBubbleFee(int index)
    {
        if (bubbleOpenFee != null && bubbleOpenFee.Count > 0)
        {
            if (index >= bubbleOpenFee.Count)
                return bubbleOpenFee[bubbleOpenFee.Count - 1];

            for (int i = 0; i < bubbleOpenFee.Count; ++i)
            {
                if (i >= index) return bubbleOpenFee[index];
            }
        }
        return null;
    }

    public ShopRoomInfo GetDecoration(string tag)
    {
        if (decorationsReward != null && decorationsReward.Count > 0)
        {
            for (int i = 0; i < decorationsReward.Count; ++i)
            {
                if(decorationsReward[i].Tag == tag)
                    return decorationsReward[i];
            }
        }
        return null;
    }

    public ShopRoomInfo GetDecoration(int index)
    {
        if (decorationsReward != null && decorationsReward.Count > 0)
        {
            if (index >= decorationsReward.Count)
                return decorationsReward[decorationsReward.Count - 1];

            for (int i = 0; i < decorationsReward.Count; ++i)
            {
                if (i >= index) return decorationsReward[index];
            }
        }
        return null;
    }

    public int GetCoins(int index)
    {
        if (coinsReward != null && coinsReward.Count > 0)
        {
            if (index >= coinsReward.Count)
                return coinsReward[coinsReward.Count - 1];

            for (int i = 0; i < coinsReward.Count; ++i)
            {
                if (i >= index) 
                    return coinsReward[index];
            }
        }
        return 0;
    }

    public List<int> GetCoinsList()
    {
        return coinsReward;
    }

    public int DecorationSize()
    {
        if (decorationsReward!=null)
            return decorationsReward.Count;

        return 0;
    }

    public int CoinsSize()
    {
        if (coinsReward != null)
            return coinsReward.Count;

        return 0;
    }
}
