using System.Collections.Generic;

public enum RecompensationType
{
    recompensation,
    reward,
    support
}

public struct RecompensationGiftClientModel
{
    public BaseGiftableResource gift;
    public int TransactionID;
    public RecompensationType type;

    public RecompensationGiftClientModel(BaseGiftableResource gift, int TransactionID, RecompensationType type)
    {
        this.gift = gift;
        this.TransactionID = TransactionID;
        this.type = type;
    }
}

public class RecompensationGiftClientModelComparer : IComparer<RecompensationGiftClientModel>
{
    public int Compare(RecompensationGiftClientModel x, RecompensationGiftClientModel y)
    {
        if (x.TransactionID > y.TransactionID)
        {
            return -1;
        }
        else if (x.TransactionID < y.TransactionID)
        {
            return 1;
        }
        else return 0;
    }
}
