using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedOffersDeltaConfig
{

    private static readonly string OFFER_PARAM_PREFIX = "offer_";
    private static readonly string UI_BADGE_PARAM = "main_UI_badge";
    private static readonly string SHOW_ON_LOAD_INTERVAL_PARAM = "interval_for_offers";

    public static bool useDefaultBadge = true;

    public static string timedOfferUIBadge = "-";
    public static int showOnLoadInterval = int.MaxValue;
    public static string currentTimedOffersConfigID = "";
    public static List<OfferInfo> timedOffersDecisionPoints = new List<OfferInfo>();

    //TODO FIX
    public static void Initialize(Dictionary<string, object> parameters)
    {
        if (parameters == null)
            return;

        //if (respons.JSON.ContainsKey("eventParams"))
        //{
        //    Dictionary<string, object> temp = (Dictionary<string, object>)respons.JSON["eventParams"];

        //    if (temp.ContainsKey("responseEngagementID"))
        //    {
        //        currentTimedOffersConfigID = temp["responseEngagementID"].ToString();
        //    }
        //}


        foreach (KeyValuePair<string, object> data in parameters)
        {
            if (data.Key == UI_BADGE_PARAM)
            {
                timedOfferUIBadge = data.Value.ToString();
            }

            if (data.Key == SHOW_ON_LOAD_INTERVAL_PARAM)
            {
                showOnLoadInterval = int.Parse(data.Value.ToString());
            }

            if (data.Key.StartsWith(OFFER_PARAM_PREFIX))
            {
                string[] offerInfo = data.Value.ToString().Split(';');
                if (timedOffersDecisionPoints.Find((x) => x.offerDecisionPoint == offerInfo[0]) != null)
                {
                    continue;
                }

                int offerEndDate = 0;
                int.TryParse(offerInfo[2], out offerEndDate);
                if (offerEndDate == 0)
                {
                    continue;
                }
                if (offerEndDate <= ServerTime.UnixTime(DateTime.UtcNow))
                {
                    continue;
                }

                int offerPriority = int.MaxValue;
                if (offerInfo.Length > 3)
                {
                    int.TryParse(offerInfo[3], out offerPriority);
                }

                timedOffersDecisionPoints.Add(new OfferInfo()
                {
                    offerDecisionPoint = offerInfo[0],
                    offerOrderNo = int.Parse(offerInfo[1]),
                    offerEndDate = offerEndDate,
                    offerPriority = offerPriority,
                });
            }
        }
    }

    public class OfferInfo
    {
        public string offerDecisionPoint = "-";
        public int offerOrderNo = 0;
        public int offerEndDate = 0;
        public int offerPriority = int.MaxValue;
    }
}
