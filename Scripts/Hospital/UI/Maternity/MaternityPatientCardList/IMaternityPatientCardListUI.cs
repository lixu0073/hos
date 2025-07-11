namespace Maternity.UI
{
    public interface IMaternityPatientCardListUI
    {
        MaternityBedPanelUI AddBedPanel();
        void ClearList();
        void SetPatientCardListActive(bool setActive);
        void ClearSelectedIndicators();
    }
}