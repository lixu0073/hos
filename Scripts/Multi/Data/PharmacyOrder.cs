using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DynamoDBv2.DocumentModel;

namespace Hospital
{
    //[DynamoDBTable("AdvertisedOffersAndroid")]
    [DynamoDBTable("PharmacyOrdersAdvertised")]
    public class OldPharmacyOrderAdvertised : PharmacyOrder
    {
        public OldPharmacyOrderAdvertised(long expiration)
        {
            expirationDate = expiration;
        }
        public OldPharmacyOrderAdvertised()
        {
        }

        public override string ToString()
        {
            return UserID + "!" + ID + "!" + medicine.ToString() + "!" + amount + "!" + pricePerUnit + "!" + bought;
        }
        public static OldPharmacyOrderAdvertised Parse(string str)
        {
            var parsed = new OldPharmacyOrderAdvertised();
            var strs = str.Split('!');
            if (strs.Length != 6)
                throw new IsoEngine.IsoException("COuld not parse string for pharmacy order advertised " + str);
            parsed.UserID = strs[0];
            parsed.ID = strs[1];
            parsed.medicine = MedicineRef.Parse(strs[2]);
            parsed.amount = int.Parse(strs[3], System.Globalization.CultureInfo.InvariantCulture);
            parsed.pricePerUnit = int.Parse(strs[4], System.Globalization.CultureInfo.InvariantCulture);
            parsed.bought = bool.Parse(strs[5]);
            return parsed;
        }
    }

    //[DynamoDBTable("AdvertisedOffersAndroid")]
    [DynamoDBTable("PharmacyOrdersAdvertised")]
    public class PharmacyOrderAdvertised : PharmacyOrder
	{
		public PharmacyOrderAdvertised(long expiration)
		{
			expirationDate = expiration;
		}
		public PharmacyOrderAdvertised() {
        }
        
        public override string ToString()
		{
			return UserID + "!" + ID + "!" + medicine.ToString() + "!" + amount + "!" + pricePerUnit + "!" + bought + "!" + isFriendOffer + "!" + isVisited;
		}
		public static PharmacyOrderAdvertised Parse(string str)
		{
			var parsed = new PharmacyOrderAdvertised();
			var strs = str.Split('!');
			if (strs.Length < 6)
				throw new IsoEngine.IsoException("COuld not parse string for pharmacy order advertised " + str);
			parsed.UserID = strs[0];
			parsed.ID = strs[1];
			parsed.medicine = MedicineRef.Parse(strs[2]);
			parsed.amount = int.Parse(strs[3], System.Globalization.CultureInfo.InvariantCulture);
			parsed.pricePerUnit = int.Parse(strs[4], System.Globalization.CultureInfo.InvariantCulture);
			parsed.bought = bool.Parse(strs[5]);
            if (strs.Length > 6)
                parsed.isFriendOffer = bool.Parse(strs[6]);
            if (strs.Length > 7)
                parsed.isVisited = bool.Parse(strs[7]);
			return parsed;
		}
	}

	[DynamoDBTable("PharmacyOrdersStandard")]
	public class PharmacyOrderStandard : PharmacyOrder
	{
        public PharmacyOrderStandard(long expiration)
        {
            expirationDate = expiration;
        }
        public PharmacyOrderStandard()
        {
        }

        public PharmacyOrderAdvertised ToAdvertised()
        {
            PharmacyOrderAdvertised advertisedOrder = new PharmacyOrderAdvertised();
            advertisedOrder.isFriendOffer = isFriendOffer;
            advertisedOrder.isVisited = isVisited;
            advertisedOrder.UserID = UserID;
            advertisedOrder.expirationDate = expirationDate;
            advertisedOrder.ID = ID;
            advertisedOrder.requiredLevel = requiredLevel;
            advertisedOrder.medicine = medicine;
            advertisedOrder.amount = amount;
            advertisedOrder.pricePerUnit = pricePerUnit; //now it's total price
            advertisedOrder.bought = bought;
            advertisedOrder.bougthBuyWise = bougthBuyWise;
            advertisedOrder.buyerSaveID = buyerSaveID;
            advertisedOrder.sortOrder = sortOrder;
            advertisedOrder.UUID = UUID;
            advertisedOrder.isLocalOffer = isLocalOffer;
            advertisedOrder.runSpawnAnim = runSpawnAnim;
            advertisedOrder.runDescentAnim = runDescentAnim;
            advertisedOrder.ownerUser = ownerUser;
            advertisedOrder.buyerUser = buyerUser;
            return advertisedOrder;
        }
        
    }

    public class WiseOrder : PharmacyOrder {
        public WiseOrder(){}
        public override string ToString()
        {
            return medicine.ToString() + "!" + amount + "!" + pricePerUnit + "!" + bought + "!" + sortOrder;
        }
        public static WiseOrder Parse(string str)
        {
            var parsed = new WiseOrder();
            var strs = str.Split('!');
            parsed.medicine = MedicineRef.Parse(strs[0]);
            parsed.amount = int.Parse(strs[1], System.Globalization.CultureInfo.InvariantCulture);
            parsed.pricePerUnit = int.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);
            parsed.bought = bool.Parse(strs[3]);
            parsed.sortOrder = int.Parse(strs[4], System.Globalization.CultureInfo.InvariantCulture);
            return parsed;
        }
    }

    public abstract class PharmacyOrder
	{

        [DynamoDBIgnore]
        public bool isFriendOffer = false;

        [DynamoDBIgnore]
        public bool isVisited = false;

        [DynamoDBHashKey]
		public string UserID;

        [DynamoDBProperty]
        public long expirationDate;

        [DynamoDBRangeKey]
		public string ID;

        [DynamoDBProperty]
        public int requiredLevel;

        [DynamoDBProperty(typeof(MedicineRefConverter))]
		public MedicineRef medicine;

		[DynamoDBProperty]
		public int amount;

		[DynamoDBProperty]
		public int pricePerUnit; //now it's total price

		[DynamoDBProperty]
		public bool bought;

        [DynamoDBProperty]
        public bool bougthBuyWise = false;

        [DynamoDBProperty]
        public string buyerSaveID;

        [DynamoDBProperty]
        public int sortOrder;

        [DynamoDBProperty]
        public string UUID;

        [DynamoDBIgnore]
        public bool isLocalOffer;

        [DynamoDBIgnore]
        public bool runSpawnAnim = false;

        [DynamoDBIgnore]
        public bool runDescentAnim = false;

        public PublicSaveModel ownerUser;

        public PublicSaveModel buyerUser;

        public string ShortSaveString()
        {
            return ID + "#" + medicine.ToString() + "#" + amount;
        }

        public static PharmacyOrder GetInstance(string strs)
        {
            var data = strs.Split('#');
            if(data.Length != 3)
            {
                return null;
            }
            PharmacyOrderStandard order = new PharmacyOrderStandard();
            order.ID = data[0];
            order.amount = int.Parse(data[2], System.Globalization.CultureInfo.InvariantCulture);
            order.medicine = MedicineRef.Parse(data[1]);
            if(order.medicine == null)
            {
                return null;
            }
            return order;
        }

        public string GetBuyerHospitalName()
        {
            if (bougthBuyWise)
            {
                return "Dr Wise";
            }
            return buyerUser == null ? "MyHospital" : (buyerUser.Name != null ? buyerUser.Name : "MyHospital");
        }

        public int GetBuyerLevel()
        {
            if(bougthBuyWise)
            {
                return 100;
            }
            return buyerUser == null ? 1 : buyerUser.Level;
        }

        public string GetBuyerFacebookID()
        {
            return buyerUser == null ? null : buyerUser.FacebookID;
        }

        public bool HasHelpRequest()
        {
            if(ownerUser == null)
            {
                return false;
            }
            return ownerUser.PlantationHelp || ownerUser.EpidemyHelp || ownerUser.TreatmentHelp;
        }

        public bool HasPlantationHelpRequest() {
            if (ownerUser == null)
            {
                return false;
            }
            return ownerUser.PlantationHelp;
        }

        public bool HasEpidemyHelpRequest()
        {
            if (ownerUser == null)
            {
                return false;
            }
            return ownerUser.EpidemyHelp;
        }

        public bool HasTreatmentHelpRequest()
        {
            if (ownerUser == null)
            {
                return false;
            }
            return ownerUser.TreatmentHelp;
        }

        public string GetHospitalName(string defaultValue)
        {
            return ownerUser == null ? defaultValue : ownerUser.Name;
        }

        public int GetLevel()
        {
            return ownerUser == null ? 1 : ownerUser.Level;
        }

        public string GetFacebookID()
        {
            return ownerUser == null ? null : ownerUser.FacebookID;
        }

        public string Preety()
        {
            return UserID + ", " + ID + ", " + medicine.ToString() + ", " + requiredLevel;
        }

        public bool IsExpired()
        {
            return expirationDate < (long)ServerTime.getTime();
        }

    }

	public class MedicineRefConverter : IPropertyConverter
	{
		public object FromEntry(DynamoDBEntry entry)
		{
			Primitive primitive = entry as Primitive;
			if (primitive == null || !(primitive.Value is string) || string.IsNullOrEmpty((string)primitive.Value))
				return null;
			return MedicineRef.Parse(primitive.Value as string);
		}

		public DynamoDBEntry ToEntry(object value)
		{
			MedicineRef med = value as MedicineRef;
			if (med == null)
				return new Primitive();
			string data = med.ToString();
			return new Primitive(data);
		}
	}
}
