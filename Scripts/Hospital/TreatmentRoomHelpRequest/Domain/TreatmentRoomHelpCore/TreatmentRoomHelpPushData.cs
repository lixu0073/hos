using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class TreatmentRoomPushData {

    string saveID;
    long pushTime;

    public TreatmentRoomPushData(string saveID, long pushTime)
    {
        this.saveID = saveID;
        this.pushTime = pushTime;
    }

    public static TreatmentRoomPushData Parse(string save)
    {
        var tmp = save.Split('^');

        string saveID = tmp[0];
        long pushTime = long.Parse(tmp[1]);

        return new TreatmentRoomPushData(saveID, pushTime);
    }

    public virtual string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(saveID);
        builder.Append("^");
        builder.Append(Checkers.CheckedAmount(pushTime, 0, long.MaxValue, "TreatmentRoomPushData pushTime").ToString());
        return builder.ToString();
    }

    public string GetPushSaveID()
    {
        return saveID;
    }

    public long GetPushTime()
    {
        return pushTime;
    }
}
