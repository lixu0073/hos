using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Hospital
{
    public class DailyDealSaveData
    {
        DailyDeal currentDailyDeal;
        public DailyDeal CurrentDailyDeal {
            get { return currentDailyDeal; }
        }

        int dailyDealStartTime;
        public int DailyDealStartTime {
            get { return dailyDealStartTime; }
        }

        int occupyIndex;
        public int OcuupyIndex{
            get { return occupyIndex; }
        }

        public DailyDealSaveData(DailyDeal currentDailyDeal, int dailyDealStartTime, int occupyIndex) {
            this.currentDailyDeal = currentDailyDeal;
            this.dailyDealStartTime = dailyDealStartTime;
            this.occupyIndex = occupyIndex;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (currentDailyDeal != null) {
                builder.Append(currentDailyDeal.ToString()); 
            } else {
                builder.Append("null");
            }
            builder.Append("^");
            builder.Append(dailyDealStartTime.ToString());
            builder.Append("^");
            builder.Append(occupyIndex.ToString());
            return builder.ToString();
        }

        public static DailyDealSaveData Parse(string toParse) {
            DailyDeal parsedCurrentDailyDeal;
            int parsedDailyDealStartTime;
            int parsedOccupyIndex;

            var toParseArr = toParse.Split('^');
            if (string.Compare(toParseArr[0], "null") != 0)
            {
                parsedCurrentDailyDeal = DailyDeal.Parse(toParseArr[0]);
            }
            else {
                parsedCurrentDailyDeal = null;
            }
            parsedDailyDealStartTime = int.Parse(toParseArr[1], System.Globalization.CultureInfo.InvariantCulture);
            parsedOccupyIndex = int.Parse(toParseArr[2], System.Globalization.CultureInfo.InvariantCulture);
            return new DailyDealSaveData(parsedCurrentDailyDeal, parsedDailyDealStartTime, parsedOccupyIndex);
        }
    }
}