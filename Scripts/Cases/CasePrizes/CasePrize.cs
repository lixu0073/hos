using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CasePrize
{
    public int caseTier = 0;
    public int coinsAmount = 0;
    public int diamondsAmount = 0;
    public int positiveEnergyAmount = 0;

    public List<ItemCasePrizeType> items = new List<ItemCasePrizeType>();
    public List<DecorationCasePrizeType> decorations = new List<DecorationCasePrizeType>();
    public List<BoosterItemCasePrizeType> boosters = new List<BoosterItemCasePrizeType>();
    public EconomySource economySource;

    public CasePrize()
    {

    }

    public CasePrize(CasePrizeCreateInput input)
    {
        caseTier = input.caseTier;
        coinsAmount = input.coinsAmount;
        diamondsAmount = input.diamondsAmount;
        positiveEnergyAmount = input.positiveEnergyAmount;
        if (input.items!=null)
        {
            items.AddRange(input.items);
        }
        if (input.decorations!=null)
        {
            decorations.AddRange(input.decorations);
        }
        if (input.boosters!=null)
        {
            boosters.AddRange(input.boosters);
        }
    }

    public string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(caseTier.ToString());
        builder.Append("!");
        builder.Append(coinsAmount.ToString());
        builder.Append("!");
        builder.Append(diamondsAmount.ToString());
        builder.Append("!");

        for (int i = 0; i < items.Count; i++)
        {
            builder.Append(items[i].item.ToString());
            builder.Append(";");
            builder.Append(items[i].amount.ToString());
            if (i < items.Count - 1)
            {
                builder.Append("*");
            }
        }


        builder.Append("!");
        for (int i = 0; i < decorations.Count; i++)
        {
            builder.Append(decorations[i].decoration.Tag);
            builder.Append(";");
            builder.Append(decorations[i].amount.ToString());
            if (i < decorations.Count - 1)
            {
                builder.Append("*");
            }
        }


        builder.Append("!");
        for (int i = 0; i < boosters.Count; i++)
        {
            builder.Append(boosters[i].boosterID.ToString());
            builder.Append(";");
            builder.Append(boosters[i].amount.ToString());
            if (i < boosters.Count - 1)
            {
                builder.Append("*");
            }
        }
        //builder.Append ();

        return builder.ToString();
    }

    public static CasePrize Parse(string casePrize)
    {
        CasePrize casePrizeP = new CasePrize();
        var caseP = casePrize.Split('!');
        casePrizeP.caseTier = int.Parse(caseP[0], System.Globalization.CultureInfo.InvariantCulture);
        casePrizeP.coinsAmount = int.Parse(caseP[1], System.Globalization.CultureInfo.InvariantCulture);
        casePrizeP.diamondsAmount = int.Parse(caseP[2], System.Globalization.CultureInfo.InvariantCulture);
        if (!string.IsNullOrEmpty(caseP[3]))
        {
            var itemsP = caseP[3].Split('*');
            for (int i = 0; i < itemsP.Length; i++)
            {
                var cont = itemsP[i].Split(';');
                MedicineRef item = MedicineRef.Parse(cont[0]);
                int amount = int.Parse(cont[1], System.Globalization.CultureInfo.InvariantCulture);
                ItemCasePrizeType tempItem = new ItemCasePrizeType(item, amount);
                casePrizeP.items.Add(tempItem);
            }
        }
        if (!string.IsNullOrEmpty(caseP[4]))
        {
            var decorationsP = caseP[4].Split('*');
            for (int i = 0; i < decorationsP.Length; i++)
            {
                var cont = decorationsP[i].Split(';');
                ShopRoomInfo decoration = Hospital.AreaMapController.Map.drawerDatabase.DrawerItems.Find(x => x.Tag == cont[0]);
                int amount = int.Parse(cont[1], System.Globalization.CultureInfo.InvariantCulture);
                DecorationCasePrizeType tempItem = new DecorationCasePrizeType(decoration, amount);
                casePrizeP.decorations.Add(tempItem);
            }
        }
        if (!string.IsNullOrEmpty(caseP[5]))
        {
            var boostersP = caseP[5].Split('*');
            for (int i = 0; i < boostersP.Length; i++)
            {
                var cont = boostersP[i].Split(';');
                int boosterID = int.Parse(cont[0], System.Globalization.CultureInfo.InvariantCulture);
                int amount = int.Parse(cont[1], System.Globalization.CultureInfo.InvariantCulture);
                BoosterItemCasePrizeType tempItem = new BoosterItemCasePrizeType(boosterID, amount);
                casePrizeP.boosters.Add(tempItem);
            }
        }
        return casePrizeP;
    }

    //private void GenerateStandardPrize(int caseTier, CaseType type)
    //{
    //    coinsAmount = HospitalAreasMapController.HospitalMap.casesManager.coinGambling(caseTier);
    //    diamondsAmount = HospitalAreasMapController.HospitalMap.casesManager.diamondGambling(caseTier);

    //    positiveEnergyAmount = HospitalAreasMapController.HospitalMap.casesManager.positiveEnergyGambling(caseTier);

    //    int itemAmount = HospitalAreasMapController.HospitalMap.casesManager.returnNewItemsAmount(caseTier);

    //    items = HospitalAreasMapController.HospitalMap.casesManager.itemGambling(caseTier, GetShovelSource(caseTier, type), itemAmount);

    //    decorations = HospitalAreasMapController.HospitalMap.casesManager.decorationGambling(caseTier, HospitalAreasMapController.HospitalMap.casesManager.casePrizesParams.decoAmount[caseTier]);

    //    boosters = HospitalAreasMapController.HospitalMap.casesManager.boosterGambling(caseTier);
    //}
}

public class CasePrizeCreateInput
{
    public int caseTier { get; private set; }
    public int coinsAmount { get; private set; }
    public int diamondsAmount { get; private set; }
    public int positiveEnergyAmount { get; private set; }
    public EconomySource economySource;

    public List<ItemCasePrizeType> items;
    public List<DecorationCasePrizeType> decorations;
    public List<BoosterItemCasePrizeType> boosters;

    public CasePrizeCreateInput(EconomySource economySource, int caseTier, int coinsAmount, int diamondsAmount, int positiveEnergyAmount, List<ItemCasePrizeType> items = null, List<DecorationCasePrizeType> decorations = null, List<BoosterItemCasePrizeType> boosters = null)
    {
        this.economySource = economySource;
        this.caseTier = caseTier;
        this.coinsAmount = coinsAmount;
        this.diamondsAmount = diamondsAmount;
        this.positiveEnergyAmount = positiveEnergyAmount;
        this.items = items;
        this.decorations = decorations;
        this.boosters = boosters;
    }
}

public class IAPCaseData
{
    public int case_id = 0;
    public string case_tag = "";

    public IAPCaseData(int case_id, string case_tag)
    {
        this.case_tag = case_tag;
        this.case_id = case_id;
    }
}

public enum CaseType
{
    ordinary,
    VIP,
    FACEBOOK,
    EPIDEMY,
    TREASURE,
    DAILY_QUEST,
    DAILY_REWARD
}