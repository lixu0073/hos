using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{

    /// <summary>
    /// Simple class which holds medicine reference and amount.
    /// Recommended for use as metadata.
    /// </summary>
    public class MedicineAmount
    {
        public MedicineRef medicine;
        public int amount;

        public MedicineAmount(MedicineRef medicine, int amount)
        {
            this.medicine = medicine;
            this.amount = amount;
        }
    }

}