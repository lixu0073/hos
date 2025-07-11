using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using IsoEngine;
using Hospital;

public class TreatmentRoomHelpSynchronizer
{
    // TEMP VALUES

    private TreatmentRoomHelpSaveData data = new TreatmentRoomHelpSaveData();

    private static TreatmentRoomHelpSynchronizer instance = null;

    public static TreatmentRoomHelpSynchronizer Instance
    {
        get
        {
            if (instance == null)
                instance = new TreatmentRoomHelpSynchronizer();

            return instance;
        }
    }

    public string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(Checkers.CheckedBool(data.helpForAddUsed));
        builder.Append("#");

        if (data.requestTimes.Count > 0)
        {
            for (int i = 0; i < data.requestTimes.Count; i++)
            {
                builder.Append(Checkers.CheckedAmount(data.requestTimes[i], 0, long.MaxValue, "TreatmentHelpSaveData requestTimes " + i + " :").ToString());
                if (i < data.requestTimes.Count - 1)
                    builder.Append("?");
            }
        }

        builder.Append("#");

        if (data.transactions.Count > 0)
        {
            for (int i = 0; i < data.transactions.Count; i++)
            {
                builder.Append(data.transactions[i]);
                if (i < data.transactions.Count - 1)
                    builder.Append("?");
            }
        }

        builder.Append("#");

        if (data.pushData.Count > 0)
        {
            for (int i = 0; i < data.pushData.Count; i++)
            {
                builder.Append(data.pushData[i].SaveToString());
                if (i < data.pushData.Count - 1)
                    builder.Append("?");
            }
        }


        return builder.ToString();
    }

    public void LoadFromString(string saveString, bool visitingMode)
    {
        if (!visitingMode)
        {
            data.transactions.Clear();
            data.requestTimes.Clear();
            data.pushData.Clear();

            if (!string.IsNullOrEmpty(saveString))
            {
                var save = saveString.Split('#');

                if (save != null)
                {
                    data.helpForAddUsed = bool.Parse(save[0]);

                    if (!string.IsNullOrEmpty(save[1]))
                    {
                        var requestTimeSave = save[1].Split('?');

                        if (requestTimeSave != null)
                        {
                            for (int i = 0; i < requestTimeSave.Length; i++)
                                data.requestTimes.Add(long.Parse(requestTimeSave[i]));
                        }
                    }

                    if (!string.IsNullOrEmpty(save[2]))
                    {
                        var transactionsSave = save[2].Split('?');

                        if (transactionsSave != null)
                        {
                            for (int i = 0; i < transactionsSave.Length; i++)
                                data.transactions.Add(transactionsSave[i]);
                        }
                    }

                    if (save.Length > 3)
                    {
                        if (!string.IsNullOrEmpty(save[3]))
                        {
                            var pushSave = save[3].Split('?');

                            if (pushSave != null)
                            {
                                for (int i = 0; i < pushSave.Length; i++)
                                    data.pushData.Add(TreatmentRoomPushData.Parse(pushSave[i]));
                            }
                        }
                    }
                }
            }
        }
    }

    public void RequestHelp(long time, List<string> savesIds = null)
    {
        if (savesIds != null)
        {
            if (savesIds.Count > 0)
            {
                for (int i = 0; i < savesIds.Count; i++)
                {
                    data.pushData.Add(new TreatmentRoomPushData(savesIds[i], time));
                }
            }
        }
        else data.requestTimes.Add(time);
    }

    public void RemoveRequestTime(long time)
    {
        if (data.requestTimes.Contains(time))
            data.requestTimes.Remove(time);
    }

    public void RemovePushData(TreatmentRoomPushData push)
    {
        if (data.pushData.Contains(push))
            data.pushData.Remove(push);
    }

    public List<long> GetRequestCooldownTimers()
    {
        return data.requestTimes;
    }

    public List<TreatmentRoomPushData> GetTreatmentRoomPushData()
    {
        return data.pushData;
    }

}
