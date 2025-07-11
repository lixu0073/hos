using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Globalization;

namespace Hospital {
    public class DailyDeal {
        MedicineRef item;
        public MedicineRef Item {
            get { return item; }
        }

        int amount;
        public int Amount {
            get { return amount; }
        }

        float discount;
        public float Discount {
            get { return discount; }
        }

        public DailyDeal(MedicineRef item, int amount, float discount) {
            this.item = item;
            this.amount = amount;
            this.discount = discount;
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            builder.Append(item.ToString());
            builder.Append("%");
            builder.Append(amount.ToString());
            builder.Append("%");
            builder.Append(discount.ToString());
            return builder.ToString();
        }

        public static DailyDeal Parse(string toParse) {
            var toParseArr = toParse.Split('%');
            MedicineRef parsedItem = MedicineRef.Parse(toParseArr[0]);
            int parsedAmount = int.Parse(toParseArr[1], System.Globalization.CultureInfo.InvariantCulture);
            float parsedDiscount = float.Parse(toParseArr[2], CultureInfo.InvariantCulture);
            return new DailyDeal(parsedItem, parsedAmount, parsedDiscount);
        }
    }
}
