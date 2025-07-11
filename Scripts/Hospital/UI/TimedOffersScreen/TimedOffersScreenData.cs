using UnityEngine.Events;

public class TimedOffersScreenData
{
    public TimedOffersScreenViewStrategy strategy = null;//
    public int timeLeft = 0;
    public int currentPageId = 0;//
    public int totalPagesCount = 0;//
    public UnityAction onCloseButtonClick = null;
    public UnityAction onPreviousOfferButtonClick = null;
    public UnityAction onNextOfferButtonClick = null;
}
