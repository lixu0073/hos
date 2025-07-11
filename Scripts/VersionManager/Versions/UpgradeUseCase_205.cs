using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Hospital
{
    public class UpgradeUseCase_205 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        private const char BEGIN_PERSONAL_GOALS_DEF = '[';
        private const char END_PERSONAL_GOALS_DEF = ']';
        private const char BEGIN_GLOBAL_GOALS_DEF = '{';
        private const char END_GLOBAL_GOALS_DEF = '}';

        public const char MAIN_SEPARATOR = ';';
        public const char INTERIA_SEPARATOR = '#';
        public const char BUNDLE_INTERIA_SEPARATOR = '@';
        public const char RANDOM_AMOUNT_SEPARATOR = '|';
        public const char RANDOM_ITEM_TYPE = 'R';
        public const char CLAIMED_TAG = '=';
        public const string RANDOM_STANDARD_BOOSTER = "RSB";
        public const string RANDOM_PREMIUM_BOOSTER = "RPB";
        public const string RANDOM_STORAGE_TOOL = "RST";
        public const string RANDOM_TANK_TOOL = "RTT";
        public const string RANDOM_SPECIAL = "RS";

        public static readonly int[] standardBoosterIndexes = new int[7] { 0, 1, 2, 4, 5, 6, 7 };
        public static readonly int[] premiumBoosterIndexes = new int[2] { 3, 8 };
        public static readonly int[] storageToolIndexes = new int[3] { 0, 1, 2 };
        public static readonly int[] tankToolIndexes = new int[3] { 4, 5, 6 };
        public static readonly int[] specialToolIndexes = new int[7] { 0, 1, 2, 3, 4, 5, 6 };

        private const string DailyRewardBigRewardNewString = "bundle;1|1||1@awesomeBox";
        private const int DailyRewardBigRewardIndex = 4;

        public Save Upgrade(Save save, bool visitingPurpose)
        {
            TranslateGlobalEventString(ref save);
            UpgradeDailyRewardSave(ref save);
            return save;
        }



        private readonly Vector2[] CoinRanges = new Vector2[6]
        {
        new Vector2(10, 50),
        new Vector2(150, 300),
        new Vector2(1000, 2000),
        new Vector2(0, 0),
        new Vector2(500, 750),
        new Vector2(25, 125)
        };

        private readonly Vector2[] DiamondRanges = new Vector2[6]
        {
        new Vector2(0, 0),
        new Vector2(0, 0),
        new Vector2(3, 3),
        new Vector2(3, 3),
        new Vector2(1, 1),
        new Vector2(1, 1)
        };

        private readonly Vector2[] PositiveEnergyRanges = new Vector2[6]
        {
        new Vector2(0, 0),
        new Vector2(0, 0),
        new Vector2(0, 0),
        new Vector2(0, 0),
        new Vector2(10, 30),
        new Vector2(0, 0),
        };

        private readonly int[] ItemAmount = new int[6] { 1, 2, 4, 1, 3, 1 };
        private readonly int[] DecoAmount = new int[6] { 0, 1, 1, 0, 1, 0 };
        private readonly float[][] BoosterProbabilities = new float[6][]
        {
        new float[3] {0.05f, 0, 0},
        new float[3] {0.2f, 0, 0},
        new float[3] {1f, 0.5f, 0},
        new float[3] {0.02f, 0, 0},
        new float[3] {1f, 0.02f, 0},
        new float[3] {0, 0, 0}
        };

        private void UpgradeDailyRewardSave(ref Save save)
        {
            if (save.dailyRewardSave != null && save.dailyRewardSave.Count > DailyRewardBigRewardIndex)
            {
                save.dailyRewardSave[DailyRewardBigRewardIndex] = DailyRewardBigRewardNewString;
            }
        }

        private void TranslateGlobalEventString(ref Save save)
        {
            if (!String.IsNullOrEmpty(save.GlobalEvent) && save.GlobalEvent.IndexOf('!') < 0 && save.GlobalEvent.IndexOf('{') < 0)
            {
                save.GlobalEvent = save.GlobalEvent.Replace('#', '!');
                string[] parts = save.GlobalEvent.Split(';');
                if (parts.Length > 1)
                {
                    int startPartIndex = 3;

                    StringBuilder builder = new StringBuilder();

                    for (int i = 0; i < startPartIndex; i++)
                    {
                        builder.Append(parts[i]);
                        builder.Append(';');
                    }

                    builder.Append(BEGIN_GLOBAL_GOALS_DEF);

                    foreach (string reward in TranslateToBaseGiftable(parts[startPartIndex]))
                    {
                        builder.Append(reward);
                        builder.Append('%');
                    }

                    builder.Remove(builder.Length - 1, 1);
                    builder.Append(END_GLOBAL_GOALS_DEF);

                    builder.Append(';');
                    startPartIndex++;

                    builder.Append(BEGIN_PERSONAL_GOALS_DEF);

                    foreach (string reward in TranslateToBaseGiftable(parts[startPartIndex]))
                    {
                        builder.Append(reward);
                        builder.Append('%');
                    }

                    builder.Remove(builder.Length - 1, 1);
                    builder.Append(END_PERSONAL_GOALS_DEF);

                    for (int i = (++startPartIndex); i < parts.Length; i++)
                    {
                        builder.Append(';');
                        builder.Append(parts[i]);
                    }

                    save.GlobalEvent = builder.ToString();
                }
            }
        }

        private IEnumerable<string> TranslateToBaseGiftable(string rewards)
        {
            string[] parts = rewards.Split('%');

            for (int i = 0; i < parts.Length; i++)
            {
                GlobalEventRewardPackage rew = GlobalEventRewardPackage.Parse(parts[i]);

                yield return TranslateGift(rew);
            }
        }

        private string TranslateGift(GlobalEventRewardPackage reward)
        {
            switch (reward.RewardType)
            {
                case GlobalEventRewardPackage.GlobalEventRewardType.Booster:
                    return TranslateToBooster(reward);

                case GlobalEventRewardPackage.GlobalEventRewardType.Coin:
                    return TranslateToCoin(reward);

                case GlobalEventRewardPackage.GlobalEventRewardType.Decoration:
                    return TranslateToDecoration(reward);

                case GlobalEventRewardPackage.GlobalEventRewardType.Diamond:
                    return TranslateToDiamond(reward);

                case GlobalEventRewardPackage.GlobalEventRewardType.GoodieBox:
                    return TranslateToGoodieBox(reward);

                case GlobalEventRewardPackage.GlobalEventRewardType.LootBox:
                    return string.Empty; //TranslateToLootBox(reward);

                case GlobalEventRewardPackage.GlobalEventRewardType.Luring:
                    return TranslateToLuring(reward);

                case GlobalEventRewardPackage.GlobalEventRewardType.Medicine:
                    return TranslateToMedicine(reward);

                case GlobalEventRewardPackage.GlobalEventRewardType.SpecialBox:
                    return TranslateToSpecialBox(reward);
            }

            return string.Empty;
        }

        private string TranslateToBooster(GlobalEventRewardPackage reward)
        {
            StringBuilder result = new StringBuilder();

            AddDefinitionHeader(ref result, BaseGiftableResourceFactory.BaseResroucesType.booster, reward.Amount);

            result.Append(INTERIA_SEPARATOR);
            result.Append((reward as GlobalEventBoosterRewardPackage).boosterID);

            AddSpriteInfo(ref result);
            AddClaimedInfo(ref result, reward.IsClaimed);

            return result.ToString();
        }

        private string TranslateToCoin(GlobalEventRewardPackage reward)
        {
            StringBuilder result = new StringBuilder();

            AddDefinitionHeader(ref result, BaseGiftableResourceFactory.BaseResroucesType.coin, reward.Amount);
            AddSpriteInfo(ref result, "coin1");
            AddClaimedInfo(ref result, reward.IsClaimed);

            return result.ToString();
        }

        private string TranslateToDecoration(GlobalEventRewardPackage reward)
        {
            StringBuilder result = new StringBuilder();

            AddDefinitionHeader(ref result, BaseGiftableResourceFactory.BaseResroucesType.decoration, reward.Amount);

            result.Append(INTERIA_SEPARATOR);
            result.Append((reward as GlobalEventDecorationRewardPackage).GetTag());

            AddSpriteInfo(ref result);
            AddClaimedInfo(ref result, reward.IsClaimed);

            return result.ToString();
        }

        private string TranslateToDiamond(GlobalEventRewardPackage reward)
        {
            StringBuilder result = new StringBuilder();

            AddDefinitionHeader(ref result, BaseGiftableResourceFactory.BaseResroucesType.diamond, reward.Amount);
            AddSpriteInfo(ref result, "diamond1");
            AddClaimedInfo(ref result, reward.IsClaimed);

            return result.ToString();
        }

        private string TranslateToMedicine(GlobalEventRewardPackage reward)
        {
            StringBuilder result = new StringBuilder();

            AddDefinitionHeader(ref result, BaseGiftableResourceFactory.BaseResroucesType.medicine, reward.Amount);

            result.Append(INTERIA_SEPARATOR);
            result.Append((reward as GlobalEventMedicineRewardPackage).GetTag());

            AddSpriteInfo(ref result);
            AddClaimedInfo(ref result, reward.IsClaimed);

            return result.ToString();
        }

        private string TranslateToLuring(GlobalEventRewardPackage reward)
        {
            StringBuilder result = new StringBuilder();

            AddDefinitionHeader(ref result, BaseGiftableResourceFactory.BaseResroucesType.medicine, reward.Amount);

            result.Append(INTERIA_SEPARATOR);
            result.Append(RANDOM_SPECIAL);

            AddSpriteInfo(ref result);
            AddClaimedInfo(ref result, reward.IsClaimed);

            return result.ToString();
        }

        private string TranslateToGoodieBox(GlobalEventRewardPackage reward)
        {
            return TranslateToCase(reward, 0);
        }

        private string TranslateToSpecialBox(GlobalEventRewardPackage reward)
        {
            return TranslateToCase(reward, 1);
        }

        private string TranslateToCase(GlobalEventRewardPackage reward, int caseTier)
        {
            StringBuilder result = new StringBuilder();

            AddDefinitionHeader(ref result, BaseGiftableResourceFactory.BaseResroucesType.bundle, reward.Amount);
            result.Append(GetPrototypeCase(caseTier));
            AddClaimedInfo(ref result, reward.IsClaimed);

            return result.ToString();
        }

        private string GetCaseTexturesId(int caseTier)
        {
            switch (caseTier)
            {
                case 0: return SingleBoxSpriteData.BundledResourceSprite.goodieBox.ToString();
                case 1: return SingleBoxSpriteData.BundledResourceSprite.specialBox.ToString();
            }

            return string.Empty;
        }

        private string GetPrototypeCase(int caseTier)
        {
            StringBuilder result = new StringBuilder();

            result.Append(BUNDLE_INTERIA_SEPARATOR);
            result.Append("GIFT_BOXES_NAME_");
            result.Append(caseTier + 1);
            result.Append(BUNDLE_INTERIA_SEPARATOR);
            result.Append(GetCaseTexturesId(caseTier));

            result.Append(BUNDLE_INTERIA_SEPARATOR);
            result.Append(GetPrototypeCoin((int)CoinRanges[caseTier].x, (int)CoinRanges[caseTier].y, 1));

            if (DiamondRanges[caseTier].y > 0)
            {
                result.Append(BUNDLE_INTERIA_SEPARATOR);
                result.Append(GetPrototypeDiamond((int)DiamondRanges[caseTier].x, (int)DiamondRanges[caseTier].y, 1));
            }

            if (PositiveEnergyRanges[caseTier].y > 0)
            {
                result.Append(BUNDLE_INTERIA_SEPARATOR);
                result.Append(GetPrototypePositiveEnergy((int)PositiveEnergyRanges[caseTier].x, (int)PositiveEnergyRanges[caseTier].y, 1));
            }

            if (ItemAmount[caseTier] > 0)
            {
                result.Append(GetPrototypeItemListForBundle(ItemAmount[caseTier], RANDOM_SPECIAL));
            }

            result.Append(GetPrototypeBoosterListForBundle(caseTier, RANDOM_STANDARD_BOOSTER));

            if (DecoAmount[caseTier] > 0)
            {
                result.Append(GetPrototypeDecorationsListForBundle(DecoAmount[caseTier], RANDOM_ITEM_TYPE.ToString()));
            }

            return result.ToString();
        }

        private string GetPrototypeCoin(int min, int max, float chance, bool isClaimed = false)
        {
            StringBuilder result = new StringBuilder();

            AddDefinitionHeader(ref result, BaseGiftableResourceFactory.BaseResroucesType.coin, min, max, chance);
            AddSpriteInfo(ref result, "coin1");
            if (isClaimed) AddClaimedInfo(ref result, true);

            return result.ToString();
        }

        private string GetPrototypeDiamond(int min, int max, float chance, bool isClaimed = false)
        {
            StringBuilder result = new StringBuilder();

            AddDefinitionHeader(ref result, BaseGiftableResourceFactory.BaseResroucesType.diamond, min, max, chance);
            AddSpriteInfo(ref result, "diamond1");
            if (isClaimed) AddClaimedInfo(ref result, true);

            return result.ToString();
        }

        private string GetPrototypePositiveEnergy(int min, int max, float chance, bool isClaimed = false)
        {
            StringBuilder result = new StringBuilder();

            AddDefinitionHeader(ref result, BaseGiftableResourceFactory.BaseResroucesType.positiveEnergy, min, max, chance);
            AddSpriteInfo(ref result, BaseResourceSpriteData.SpriteType.positiveEnergy1.ToString());
            if (isClaimed) AddClaimedInfo(ref result, true);

            return result.ToString();
        }

        private string GetPrototypeItemListForBundle(int count, string itemTypeTag)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < count; i++)
            {
                result.Append(BUNDLE_INTERIA_SEPARATOR);
                result.Append(GetPrototypeMedicine(1, 1, 1, itemTypeTag));
            }

            return result.ToString();
        }

        private string GetPrototypeMedicine(int min, int max, float chance, string type, bool isClaimed = false)
        {
            StringBuilder result = new StringBuilder();

            AddDefinitionHeader(ref result, BaseGiftableResourceFactory.BaseResroucesType.medicine, min, max, chance);

            result.Append(INTERIA_SEPARATOR);
            result.Append(type);

            AddSpriteInfo(ref result);
            if (isClaimed) AddClaimedInfo(ref result, isClaimed);

            return result.ToString();
        }

        private string GetPrototypeBoosterListForBundle(int caseTier, string type)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < BoosterProbabilities[caseTier].Length; i++)
            {
                float chance = BoosterProbabilities[caseTier][i];

                if (chance > 0f)
                {
                    result.Append(BUNDLE_INTERIA_SEPARATOR);
                    result.Append(GetPrototypeBooster(1, 1, chance, type));
                }
            }

            return result.ToString();
        }

        private string GetPrototypeBooster(int min, int max, float chance, string type, bool isClaimed = false)
        {
            StringBuilder result = new StringBuilder();

            AddDefinitionHeader(ref result, BaseGiftableResourceFactory.BaseResroucesType.booster, min, max, chance);

            result.Append(INTERIA_SEPARATOR);
            result.Append(type);

            AddSpriteInfo(ref result);
            if (isClaimed) AddClaimedInfo(ref result, isClaimed);

            return result.ToString();
        }

        private string GetPrototypeDecorationsListForBundle(int count, string type)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < count; i++)
            {
                result.Append(BUNDLE_INTERIA_SEPARATOR);
                result.Append(GetPrototypeDecoration(1, 1, 1, type));
            }

            return result.ToString();
        }

        private string GetPrototypeDecoration(int min, int max, float chance, string type, bool isClaimed = false)
        {
            StringBuilder result = new StringBuilder();

            AddDefinitionHeader(ref result, BaseGiftableResourceFactory.BaseResroucesType.decoration, min, max, chance);

            result.Append(INTERIA_SEPARATOR);
            result.Append(type);

            AddSpriteInfo(ref result);
            if (isClaimed) AddClaimedInfo(ref result, isClaimed);

            return result.ToString();
        }

        private void AddDefinitionHeader(ref StringBuilder result, BaseGiftableResourceFactory.BaseResroucesType type, int amount)
        {
            AddTypeInfo(ref result, type);
            AddAmountInfo(ref result, amount);
        }

        private void AddDefinitionHeader(ref StringBuilder result, BaseGiftableResourceFactory.BaseResroucesType type, int min, int max, float chance)
        {
            AddTypeInfo(ref result, type);
            AddAmountInfo(ref result, min, max, chance);
        }

        private void AddTypeInfo(ref StringBuilder builder, BaseGiftableResourceFactory.BaseResroucesType type)
        {
            builder.Append(type);
            builder.Append(MAIN_SEPARATOR);
        }

        private void AddAmountInfo(ref StringBuilder builder, int amount)
        {
            builder.Append(RANDOM_AMOUNT_SEPARATOR);
            builder.Append(RANDOM_AMOUNT_SEPARATOR);
            builder.Append(amount);
        }

        private void AddAmountInfo(ref StringBuilder builder, int min, int max, float chance)
        {
            builder.Append(min);
            builder.Append(RANDOM_AMOUNT_SEPARATOR);
            builder.Append(max);
            builder.Append(RANDOM_AMOUNT_SEPARATOR);
            builder.Append(RANDOM_AMOUNT_SEPARATOR);
            builder.Append(chance);
        }

        private void AddSpriteInfo(ref StringBuilder builder, string sprite = "dynamic")
        {
            builder.Append(INTERIA_SEPARATOR);
            builder.Append(sprite);
        }

        private void AddClaimedInfo(ref StringBuilder builder, bool isClaimed)
        {
            builder.Append(CLAIMED_TAG);
            builder.Append(isClaimed);
        }
    }
}
