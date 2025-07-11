using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Hospital
{
    public class UpgradeUseCase_215 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        readonly int indexUpgradeStartValue = 114, amountOfDecosRemoved = 6;
        readonly char valueSplitter = '?';

        public Save Upgrade(Save save, bool visitingPurpose)
        {
            UpgradeDecoDBIndexes(save);
            return save;
        }

        private void UpgradeDecoDBIndexes(Save save)
        {
            if (string.IsNullOrEmpty(save.BadgesToShow))
                return;

            int oldValue = 0;
            StringBuilder newSaveString = new StringBuilder();

            var rotationsSplit = save.BadgesToShow.Split(valueSplitter);

            for (int i = 0; i < rotationsSplit.Length; i++)
            {
                if (int.TryParse(rotationsSplit[i], out oldValue))
                {
                    if (oldValue >= indexUpgradeStartValue)
                        oldValue -= amountOfDecosRemoved;

                    newSaveString.Append(oldValue.ToString());

                    if (i < rotationsSplit.Length - 1)
                        newSaveString.Append(valueSplitter);
                }
            }

            save.BadgesToShow = newSaveString.ToString();
        }
    }
}
