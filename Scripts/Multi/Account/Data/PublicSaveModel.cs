using Amazon.DynamoDBv2.DataModel;
using System;
using Amazon.DynamoDBv2.DocumentModel;

namespace Hospital
{
    [DynamoDBTable("PublicSaves")]
    public class PublicSaveModel
    {
        [DynamoDBHashKey]
        public string SaveID;

        [DynamoDBProperty]
        public string Name;

        [DynamoDBProperty]
        public string FacebookID;

        [DynamoDBProperty]
        public int Level;

        [DynamoDBProperty]
        public bool PlantationHelp;

        [DynamoDBProperty]
        public bool EpidemyHelp;

        [DynamoDBProperty]
        public bool TreatmentHelp;

        [DynamoDBProperty]
        public long lastPromotionOfferAdd = -1;

        [DynamoDBProperty]
        public long lastStandardOfferAdd = -1;

        [DynamoDBProperty]
        public long lastActivity = -1;

        [DynamoDBProperty(typeof(BubbleBoyRewardConverter))]
        public BubbleBoyReward BestWonItem;

        [DynamoDBProperty]
        public string Platform;

        [DynamoDBProperty]
        public string Language;

        [DynamoDBProperty]
        public string NotificationApiKey;

        [DynamoDBProperty]
        public bool PushOn = true;

        [DynamoDBProperty]
        public string ReputationScores;

        public PublicSaveModel Copy()
        {
            return new PublicSaveModel()
            {
                SaveID = SaveID,
                Level = Level,
                Name = Name,
                PlantationHelp = PlantationHelp,
                EpidemyHelp = EpidemyHelp,
                TreatmentHelp = TreatmentHelp,
                BestWonItem = BestWonItem,
                lastPromotionOfferAdd = lastPromotionOfferAdd,
                lastStandardOfferAdd = lastStandardOfferAdd,
                Platform = Platform,
                Language = Language,
                NotificationApiKey = NotificationApiKey,
                lastActivity = lastActivity,
                PushOn = PushOn,
                ReputationScores = ReputationScores
            };
        }

        public bool HasAdvertisedOffer()
        {
            if (lastPromotionOfferAdd == -1)
                return false;
            return lastPromotionOfferAdd > (long)ServerTime.getTime() - 10800;
        }

        public bool HasStandardOffer()
        {
            if (lastStandardOfferAdd == -1)
                return false;
            return lastStandardOfferAdd > (long)ServerTime.getTime() - 216000;
        }

        public string Pretty()
        {
            return "save id: " + SaveID + " facebook: " + FacebookID + " Name:" + Name;
        }

        public class BubbleBoyRewardConverter : IPropertyConverter
        {
            public object FromEntry(DynamoDBEntry entry)
            {
                Primitive primitive = entry as Primitive;
                if (primitive == null || !(primitive.Value is string) || string.IsNullOrEmpty((string)primitive.Value))
                    return null;

                string saveString = (string)primitive.Value;
                var save = saveString.Split(';');
                Type type = Type.GetType(save[0]);
                if (type == null)
                {
                    return null;
                }
                BubbleBoyReward reward = (BubbleBoyReward)Activator.CreateInstance(type);
                try
                {
                    reward.LoadFromString(saveString);
                }
                catch (BubbleBoyReward.UnknownMedicineException)
                {
                    return null;
                }
                return reward;
            }

            public DynamoDBEntry ToEntry(object value)
            {
                BubbleBoyReward reward = value as BubbleBoyReward;
                if (reward == null)
                    return new Primitive();
                string data = reward.SaveToString();
                return new Primitive(data);
            }
        }

    }
}