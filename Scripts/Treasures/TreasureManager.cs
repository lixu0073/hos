using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

namespace Hospital
{
    public class TreasureManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private GameObject treasure;
#pragma warning restore 0649
        [SerializeField] private List<Transform> treasureSpots = new List<Transform>();
#pragma warning disable 0649
        [SerializeField] private TreasureWeights weights;
        [SerializeField] private ParticleSystem particles;
#pragma warning restore 0649

        [HideInInspector] public float[] weightRanges = new float[2];

        private int lastTreasureTime = 0;
        private Vector3 treasurePosition;

        public List<int> OccupiedSpots = new List<int>();

        private BalanceableInt treasureChestSpawnIntervalBalanceable;
        private int TreasureChestSpawnInterval
        {
            get
            {
                if (treasureChestSpawnIntervalBalanceable == null)
                    treasureChestSpawnIntervalBalanceable = BalanceableFactory.CreateTreasureChestSpawnCooldownBalanceable();

                return treasureChestSpawnIntervalBalanceable.GetBalancedValue();
            }
        }

        private int GetTreasureInterval()
        {
            return TreasureChestSpawnInterval;
        }

        void Awake()
        {
            CalcWeightRanges();
        }

        public string Save()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(lastTreasureTime.ToString());

            return builder.ToString();
        }

        public void Load(string save)
        {
            if (!string.IsNullOrEmpty(save))
                lastTreasureTime = int.Parse(save, System.Globalization.CultureInfo.InvariantCulture);
            else
                lastTreasureTime = GetTreasureInterval();
        }

        public void OnLoad()
        {
            OccupiedSpots.Clear();
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
                HideTrasure();
            else if (TutorialController.Instance.IsTutorialStepCompleted(StepTag.treasure_collected))
            {
                HideTrasure();
            }

            int timeSinceLastTreasure = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds) - lastTreasureTime;
            if (timeSinceLastTreasure >= GetTreasureInterval())
                ShowTreasure();
        }

        private void ShowTreasure()
        {
            if (!treasure.activeSelf && Game.Instance.gameState().GetHospitalLevel() > 1)
            {
                treasure.transform.position = RandomSpot().Value;
                particles.gameObject.transform.position = treasurePosition;
                particles.Play();
                treasure.SetActive(true);
            }
        }

        private void HideTrasure()
        {
            treasure.SetActive(false);
        }

        public void ShowTreasureFromTutorial()
        {
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
                return;

            treasure.transform.position = treasureSpots[13].position;
            particles.gameObject.transform.position = treasurePosition;
            particles.Play();
            treasure.SetActive(true);
        }

        public GameObject GetTreasureObject()
        {
            return treasure;
        }

        public KeyValuePair<int, Vector3> RandomSpot()
        {
            List<int> FreeSpotsIndexes = Enumerable.Range(0, treasureSpots.Count - 1).ToList();
            //Remove all occupied
            for (int i = 0; i < OccupiedSpots.Count; ++i)
            {
                FreeSpotsIndexes.Remove(OccupiedSpots[i]);
            }
            if (!FreeSpotsIndexes.Any())
            {//if no spot is free, return a point outside the view.
                return new KeyValuePair<int, Vector3>(-1, new Vector3(0, 0, 0));
            }
            int randomIndex = GameState.RandomNumber(0, FreeSpotsIndexes.Count);
            int SpotIndex = FreeSpotsIndexes[randomIndex];
            OccupiedSpots.Add(SpotIndex);
            KeyValuePair<int, Vector3> spot = new KeyValuePair<int, Vector3>(SpotIndex, treasureSpots[SpotIndex].position);
            treasurePosition = spot.Value;
            return spot;
        }

        public void ResetSpot(int index)
        {
            OccupiedSpots.Remove(index);
        }

        public void DebugSetSpot(TMPro.TMP_InputField field)
        {
            if (int.TryParse(field.text, out int index))
            {
                SetTreasureSpot(index);
                treasure.SetActive(true);
            }
            else
                Debug.LogError("Not chest treasure spawned!");
        }

        public void SetTreasureSpot(int spot)
        {
            int spotID = Mathf.Clamp(spot, 0, treasureSpots.Count - 1);
            treasure.transform.position = treasureSpots[spotID].position;
        }

        #region TreasureMethods

        public void TreasureClick()
        {
            particles.Play();
            treasure.SetActive(false);
            OpenTreasureCase();
            SoundsController.Instance.PlayPoof();

            AnalyticsController.instance.ReportTreasure();
        }

        #endregion

        private void OpenTreasureCase()
        {
            ((HospitalCasesManager)AreaMapController.Map.casesManager).GiftFromTreasure = true;
            UIController.getHospital.unboxingPopUp.OpenTreasureCasePopup();
            lastTreasureTime = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
        }

        [System.Serializable]
        private struct TreasureWeights
        {
#pragma warning disable 0649
            public float Coins;
            public float Diamonds;
            public float Tools;
#pragma warning restore 0649
        }

        public void CalcWeightRanges()
        {
            float sum = TresureChestDeltaConfig.DiamondsFromTreasureChest ? weights.Coins + weights.Diamonds + weights.Tools : weights.Coins + weights.Tools;
            weightRanges[0] = (weights.Coins) / sum;
            weightRanges[1] = TresureChestDeltaConfig.DiamondsFromTreasureChest ? (weights.Coins + weights.Diamonds) / sum : (weights.Coins) / sum;
        }

    }
}
