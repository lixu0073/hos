using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleUI;
using TMPro;
using System;

namespace Hospital
{
    public class ReportPopupController : UIElement
    {
        [SerializeField] private TextMeshProUGUI CheerText = null;

        [SerializeField] private TextMeshProUGUI TotalPatientsCuredCounter = null;
        [SerializeField] private TextMeshProUGUI TreatmentPatientsCuredCounter = null;
        [SerializeField] private TextMeshProUGUI DoctorPatientsCuredCounter = null;
        [SerializeField] private TextMeshProUGUI VIPPatientsCuredCounter = null;
        [SerializeField] private TextMeshProUGUI DiagnosedPatientsCounter = null;

        [SerializeField] private TextMeshProUGUI MedicinesProducedCounter = null;
        [SerializeField] private TextMeshProUGUI ElixirCollectedCounter = null;

        [SerializeField] private GameObject NeededCurePrefab = null;
        [SerializeField] private GameObject NeededCuresContent = null;
        [SerializeField] private GameObject cures = null;
#pragma warning disable 0649
        [SerializeField] private ScrollRect scrollRect;

        [SerializeField] private ParticleSystem cureReadyParticles;
#pragma warning restore 0649
        public int minLevel = 7;
        public int minTimeFromSave = 21600;
        [HideInInspector]
        public bool canBeOpen = true;

        public int timeFromSave = 0;

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            if (!canBeOpen)
                yield break;
            if (Game.Instance.gameState().GetHospitalLevel() < minLevel || timeFromSave < minTimeFromSave)
                yield break;
            if (UIController.get.ActivePopUps.Count > 0)
                yield break;

            yield return base.Open();
            SetData();
            SoundsController.Instance.PlayFaxMachine();

            StartCoroutine(ScrollUpEffect());

            whenDone?.Invoke();
        }

        public void ButtonExit()
        {
            Exit();
        }

        public void SetData()
        {
            CheerText.SetText(I2.Loc.ScriptLocalization.Get("DAILY_REPORTS/DAILY_REPORT_" + GameState.RandomNumber(11, 16)));

            TotalPatientsCuredCounter.SetText(GameState.Get().PatientsCount.PatientsCuredCount.ToString());
            TreatmentPatientsCuredCounter.SetText(GameState.Get().PatientsCount.PatientsCuredCountBed.ToString());
            DoctorPatientsCuredCounter.SetText(GameState.Get().PatientsCount.PatientsCuredCountDoctor.ToString());
            VIPPatientsCuredCounter.SetText(GameState.Get().PatientsCount.PatientsCuredCountVIP.ToString());
            DiagnosedPatientsCounter.SetText(GameState.Get().PatientsCount.PatientsDiagnosedCount.ToString());

            MedicinesProducedCounter.SetText(GameState.Get().CuresCount.ProducedMedicinesCount.ToString());
            ElixirCollectedCounter.SetText(GameState.Get().CuresCount.ProducedElixirsCount.ToString());

            //Set cures Needed
            SetCuresContent();
        }

        public void SetCuresContent()
        {
            NeededCuresContent.SetActive(false);

            int tempCount = cures.transform.childCount;
            for (int i = 0; i < tempCount; i++)
            {
                Destroy(cures.transform.GetChild(i).gameObject);
            }

            List<MedicineBadgeHintInfo> neededMedicines = MedicineBadgeHintsController.Get().GetNeededMedicines();

            bool showCures = false;
            int curesNeededCount;
            for (int i = 0; i < neededMedicines.Count; i++)
            {
                curesNeededCount = MedicineBadgeHintsController.Get().GetMedicineNeededToHealCountFromNeededMedicines(neededMedicines[i].GetMedicineRef());
                if (curesNeededCount > 0)
                {
                    showCures = true;
                    GameObject tmp = (GameObject)Instantiate(NeededCurePrefab, cures.transform);
                    tmp.GetComponent<ReportNeededCureController>().SetMedicine(neededMedicines[i].GetMedicineRef(), curesNeededCount);
                    tmp.transform.localScale = new Vector3(1, 1, 1);
                }
            }
            if (showCures)
                NeededCuresContent.SetActive(true);
        }

        public void CheckCureReady(Vector3 position)
        {
            if (HospitalBedController.isNewCureAvailable)
            {
                cureReadyParticles.transform.position = position;
                SoundsController.Instance.PlayAlert();
                cureReadyParticles.Play();
            }

            HospitalBedController.isNewCureAvailable = false;
        }

        public void HideCuresNeeded()
        {
            cures.SetActive(false);
        }

        IEnumerator ScrollUpEffect()
        {
            //Debug.LogError("ScrollUpEffect");
            float normPos = 0;
            scrollRect.verticalNormalizedPosition = normPos;
            yield return new WaitForSeconds(1.75f);

            while (normPos < 1)
            {
                normPos += Time.deltaTime / 2.5f;
                scrollRect.verticalNormalizedPosition = normPos;
                yield return null;
            }
        }
    }
}
