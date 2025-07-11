using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Globalization;

namespace Hospital {
    public class DailyDealData {
        List<DailyDeal> dailyDeals;
        DailyDealChanceParams dailyDealChanceParams;
        ShovelParams shovelParams;
        ToolParams upgradeToolParams;
        PositiveEnergyParams positiveEnergyParams;

        public DailyDealData() { }

        DailyDealData(List<DailyDeal> dailyDeals, DailyDealChanceParams dailyDealChanceParams, ShovelParams shovelParams, ToolParams upgradeToolParams, PositiveEnergyParams positiveEnergyParams) {
            this.dailyDeals = dailyDeals;
            this.dailyDealChanceParams = dailyDealChanceParams;
            this.shovelParams = shovelParams;
            this.upgradeToolParams = upgradeToolParams;
            this.positiveEnergyParams = positiveEnergyParams;
        }

        public List<DailyDeal> GetDailyDeals() {
            return dailyDeals;
        }

        public DailyDealChanceParams GetDailyDealChanceParams() {
            return dailyDealChanceParams;
        }

        public ShovelParams GetShovelParams() {
            return shovelParams;
        }
        public ToolParams GetUpgradeToolParams()
        {
            return upgradeToolParams;
        }
        public PositiveEnergyParams GetPositiveEnergyParams()
        {
            return positiveEnergyParams;
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < dailyDeals.Count; ++i)
            {
                builder.Append(dailyDeals[i].ToString());
                if (i < dailyDeals.Count - 1) {
                    builder.Append("^");
                }
            }
            builder.Append("!");
            builder.Append(dailyDealChanceParams.ToString());
            builder.Append("!");
            builder.Append(shovelParams.ToString());
            builder.Append("!");
            builder.Append(upgradeToolParams.ToString());
            builder.Append("!");
            builder.Append(positiveEnergyParams.ToString());

            return builder.ToString();
        }

        public static DailyDealData Parse(string toParse) {
            List<DailyDeal> parsedDailyDeals = new List<DailyDeal>();
            DailyDealChanceParams parsedDailyDealChanceParams;
            ShovelParams parsedShovelParams;
            ToolParams parsedUpgradeToolParams;
            PositiveEnergyParams parsedPositiveEnergyParams;

            var toParseArr = toParse.Split('!');
            var toParseDailyDeals = toParseArr[0].Split('^');
            for (int i = 0; i < toParseDailyDeals.Length; ++i)
            {
                parsedDailyDeals.Add(DailyDeal.Parse(toParseDailyDeals[i]));
            }
            parsedDailyDealChanceParams = DailyDealChanceParams.Parse(toParseArr[1]);
            parsedShovelParams = ShovelParams.Parse(toParseArr[2]);
            parsedUpgradeToolParams = ToolParams.Parse(toParseArr[3]);
            parsedPositiveEnergyParams = PositiveEnergyParams.Parse(toParseArr[4]);

            return new DailyDealData(parsedDailyDeals, parsedDailyDealChanceParams, parsedShovelParams, parsedUpgradeToolParams, parsedPositiveEnergyParams);
        }

        public class DailyDealChanceParams {
            float dailyDealChance;
            public float DailyDealChance {
                get { return dailyDealChance; }
            }

            int minIndex;
            public int MinIndex {
                get { return minIndex; }
            }

            int maxIndex;
            public int MaxIndex {
                get { return maxIndex; }
            }

            int dailyDealLength;
            public int DailyDealLength {
                get { return dailyDealLength; }
            }

            DailyDealChanceParams(float dailyDealChance, int minIndex, int maxIndex, int dailyDealLength)
            {
                this.dailyDealChance = dailyDealChance;
                this.minIndex = minIndex;
                this.maxIndex = maxIndex;
                this.dailyDealLength = dailyDealLength;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(dailyDealChance.ToString());
                builder.Append("%");
                builder.Append(minIndex.ToString());
                builder.Append("%");
                builder.Append(maxIndex.ToString());
                builder.Append("%");
                builder.Append(dailyDealLength.ToString());
                return builder.ToString();
            }

            public static DailyDealChanceParams Parse(string toParse)
            {
                float toParseDailyDealChance;
                int toParseMinIndex;
                int toParseMaxIndex;
                int toParseDailyDealLength;

                var toParseArr = toParse.Split('%');
                toParseDailyDealChance = float.Parse(toParseArr[0], CultureInfo.InvariantCulture);
                toParseMinIndex = int.Parse(toParseArr[1], System.Globalization.CultureInfo.InvariantCulture);
                toParseMaxIndex = int.Parse(toParseArr[2], System.Globalization.CultureInfo.InvariantCulture);
                toParseDailyDealLength = int.Parse(toParseArr[3], System.Globalization.CultureInfo.InvariantCulture);

                return new DailyDealChanceParams(toParseDailyDealChance, toParseMinIndex, toParseMaxIndex, toParseDailyDealLength);
            }
        }

        public class ShovelParams{
            int fieldTreshold;
            public int FieldTreshold{
                get { return fieldTreshold; }  
            }

            int shovelTreshold;
            public int ShovelTreshold
            {
                get { return shovelTreshold; }
            }

            int shovelMinAmount;
            public int ShovelMinAmount
            {
                get { return shovelMinAmount; }
            }

            ShovelParams(int fieldTreshold, int shovelTreshold, int shovelMinAmount) {
                this.fieldTreshold = fieldTreshold;
                this.shovelTreshold = shovelTreshold;
                this.shovelMinAmount = shovelMinAmount;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(fieldTreshold.ToString());
                builder.Append("%");
                builder.Append(shovelTreshold.ToString());
                builder.Append("%");
                builder.Append(shovelMinAmount.ToString());
                return builder.ToString();
            }

            public static ShovelParams Parse(string toParse) {
                int toParseFieldTreshold;
                int toParseShoveltreshold;
                int toParseShovelMinAmount;

                var toParseArr = toParse.Split('%');
                toParseFieldTreshold = int.Parse(toParseArr[0]);
                toParseShoveltreshold = int.Parse(toParseArr[1]);
                toParseShovelMinAmount = int.Parse(toParseArr[2]);

                return new ShovelParams(toParseFieldTreshold, toParseShoveltreshold, toParseShovelMinAmount);
            }
        }
        public class ToolParams{
            int upgradeToolTreshold;
            public int UpgradeToolTreshold
            {
                get { return upgradeToolTreshold; }
            }

            ToolParams(int upgradeToolTreshold)
            {
                this.upgradeToolTreshold = upgradeToolTreshold;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(upgradeToolTreshold.ToString());
                return builder.ToString();
            }

            public static ToolParams Parse(string toParse)
            {
                int toParseUpgradeToolTreshold;
                toParseUpgradeToolTreshold = int.Parse(toParse);

                return new ToolParams(toParseUpgradeToolTreshold);
            }
        }
        public class PositiveEnergyParams{
            float positiveEnergyTreshold;
            public float PositiveEnergyTreshold
            {
                get { return positiveEnergyTreshold; }
            }

            PositiveEnergyParams(float positiveEnergyTreshold)
            {
                this.positiveEnergyTreshold = positiveEnergyTreshold;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(positiveEnergyTreshold.ToString());
                return builder.ToString();
            }

            public static PositiveEnergyParams Parse(string toParse)
            {
                float toParseParsepositiveEnergyTreshold;
                toParseParsepositiveEnergyTreshold = float.Parse(toParse, CultureInfo.InvariantCulture);

                return new PositiveEnergyParams(toParseParsepositiveEnergyTreshold);
            }
        }
    }
}