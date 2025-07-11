using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreatmentRoomHelpSaveData
{
    public bool helpForAddUsed;
    public List<long> requestTimes;
    public List<string> transactions;
    public List<TreatmentRoomPushData> pushData;

    public TreatmentRoomHelpSaveData()
    {
        helpForAddUsed = false;
        requestTimes = new List<long>();
        transactions = new List<string>();
        pushData = new List<TreatmentRoomPushData>();
    }
}
