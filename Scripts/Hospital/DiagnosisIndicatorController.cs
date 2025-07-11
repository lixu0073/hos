using UnityEngine;

namespace Hospital
{
    public class DiagnosisIndicatorController : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private SpriteRenderer icon;
#pragma warning restore 0649

        public void SetIcon(HospitalDataHolder.DiagRoomType roomType)
        {
            icon.sprite = ResourcesHolder.GetHospital().diagnosisBadgeGfx.GetDiagnosisCloudIcon(roomType);
        }
    }
}
