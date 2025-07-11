using Maternity.Adapter;
using Maternity.UI;
using UnityEngine;

namespace Maternity
{
    public class MaternityPatientMasterCardController : MonoBehaviour
    {
        public MaternityPatientAI Ai;
        public MaternityCharacterInfo Info;
        public MaternityWaitingRoomBed Bed;
        private MaternityPatientMasterCardBaseAdapter adapter;
        private MaternityPatientMasterCardBaseAdapter Adapter
        {
            get { return adapter; }
            set {
                if (adapter != null)
                    adapter.OnDestroy();
                adapter = value;
            }   
        }
#pragma warning disable 0649
        [SerializeField]
        private MaternityTreatmentPanelUI treatmentPanelUI;
#pragma warning restore 0649

        public void SetData(MaternityWaitingRoomBed bed)
        {
            if (adapter != null)
                adapter.OnDestroy();
            this.Bed = bed;
            if (Bed != null)
            {
                IMaternityFacilityPatient patient = Bed.GetPatient();
                if (patient != null)
                {
                    Ai = patient.GetPatientAI();
                    Info = patient.GetInfoPatient();
                    Ai.Person.State.MoveTo();
                    SetShown();
                }
                else
                {
                    Ai = null;
                    Info = null;
                    Bed.room.MoveCameraToThisRoom();
                }
            }
            SetUpAdapter();
            AddListeners();
        }

        private void SetShown()
        {
            if(!Info.PatientShown && Ai.Person.State.GetTag() != PatientStates.MaternityPatientStateTag.GTWR)
            {
                Info.PatientShown = true;
                Bed.room.SetUpIndicators();
            }
        }

        private void AddListeners()
        {
            if (Ai == null)
                return;
            RemoveListeners();
            Ai.onStateChanged += Ai_onStateChanged;
        }

        private void Ai_onStateChanged()
        {
            Adapter = MaternityPatientMasterCardAdapterFactory.Get(this, treatmentPanelUI);
        }

        private void RemoveListeners()
        {
            if (Ai != null)
                Ai.onStateChanged -= Ai_onStateChanged;
        }

        private void SetUpAdapter()
        {
            Ai_onStateChanged();
        }

        #region MonoBehaviour

        private void Update()
        {
            if (Adapter != null)
                Adapter.OnUpdate();
        }

        private void OnDestroy()
        {
            RemoveListeners();
            if (Adapter != null)
                Adapter = null;
        }
        #endregion
    }
}
