using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftsSynchronizer
{

    private Data data = new Data();

    #region Static

    private static GiftsSynchronizer instance = null;

    public static GiftsSynchronizer Instance
    {
        get
        {
            if (instance == null)
                instance = new GiftsSynchronizer();
            return instance;
        }
    }

    #endregion

    public class Data
    {
        public List<long> giftsCooldownTimers = new List<long>();
        public Dictionary<string, long> sendedGifts = new Dictionary<string, long>();
        public long LastBuyOrLastRefreshWithSomeGiftsTime = -1;

        public List<string> SendedGiftsToList()
        {
            List<string> list = new List<string>();
            foreach(KeyValuePair<string, long> data in sendedGifts)
            {
                list.Add(data.Key + ";" + data.Value);
            }
            return list;
        }
    }
    
    public Data GetData()
    {
        return data;
    }

    public List<long> GetGiftsCooldownTimers()
    {
        return data.giftsCooldownTimers;
    }

    public Dictionary<string, long> GetSendedGifts()
    {
        return data.sendedGifts;
    }

    public long GetLastBuyOrLastRefreshWithSomeGiftsTime()
    {
        return data.LastBuyOrLastRefreshWithSomeGiftsTime;
    }

    public void Load(List<long> giftsCooldownTimers, List<string> sendedGifts, long LastBuyOrLastRefreshWithSomeGiftsTime, bool isVisiting)
    {
        if (!isVisiting)
        {
            data.giftsCooldownTimers = giftsCooldownTimers == null ? new List<long>() : giftsCooldownTimers;
            data.sendedGifts = ParseListToDictionary(sendedGifts);
            data.LastBuyOrLastRefreshWithSomeGiftsTime = LastBuyOrLastRefreshWithSomeGiftsTime;
        }
        OnDataLoaded(isVisiting);
    }

    private Dictionary<string, long> ParseListToDictionary(List<string> sendedGiftsList)
    {
        Dictionary<string, long> dic = new Dictionary<string, long>();
        if (sendedGiftsList != null)
        {
            foreach (string data in sendedGiftsList)
            {
                try
                {
                    string[] array = data.Split(';');
                    dic.Add(array[0], long.Parse(array[1]));
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error in sended gift parse: " + ex.Message);
                }
            }
        }
        return dic;
    }

    private void OnDataLoaded(bool isVisiting)
    {
        if (!isVisiting)
        {
            GiftsSendController.Instance.Initialize();
        }
        GiftsReceiveController.Instance.Initialize(isVisiting);
    }

}
