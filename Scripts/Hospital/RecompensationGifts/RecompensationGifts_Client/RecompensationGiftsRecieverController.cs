using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class RecompensationGiftsRecieverController : MonoBehaviour
    {
        private const char REWARD_SEPARATOR = '!';
        private List<RecompensationGiftClientModel> recompensationGifts;

        public void Initialize()
        {
            RecompensationGiftsAPI.Instance.FetchRecompensationGifts((gifts) =>
            {
                if (gifts != null && gifts.Count > 0)
                {
                    for (int i = 0; i < gifts.Count; i++)
                    {
                        string[] unparsedReward = gifts[i].Reward.Split(REWARD_SEPARATOR);
                        for (int j = 0; j < unparsedReward.Length; j++)
                        {
                            try
                            {
                                RecompensationType type = (RecompensationType)Enum.Parse(typeof(RecompensationType), gifts[i].RewardType);
                                TryAddGiftToList(BaseGiftableResourceFactory.CreateGiftableFromString(unparsedReward[j], EconomySource.RecompensationGift), gifts[i].TransactionID, type);
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }
                    if (recompensationGifts != null && recompensationGifts.Count > 0)
                    {
                        RecompensationGiftClientModelComparer comparer = new RecompensationGiftClientModelComparer();
                        recompensationGifts.Sort(comparer);
                        StartCoroutine(UIController.get.alertPopUp.Open(AlertType.RECOMPENSATION, null, null, () =>
                        {
                            for (int i = 0; i < recompensationGifts.Count; i++)
                            {
                                Debug.Log($"Reward {i} is a {recompensationGifts[i].gift.rewardType} in amount of {recompensationGifts[i].gift.GetGiftAmount()}");
                            }
                        }));
                    }
                }
            }, (exception) =>
            {
                Debug.LogError("Could not initialize recompensation gifts. Terminating");
                Debug.LogError(exception.Message);
            });
        }

        private void TryAddGiftToList(BaseGiftableResource baseGiftableResource, string TransactionID, RecompensationType type)
        {
            if (baseGiftableResource != null)
            {
                if (recompensationGifts == null)
                {
                    recompensationGifts = new List<RecompensationGiftClientModel>();
                }
                int transactionID = 0;
                int.TryParse(TransactionID, out transactionID);
                if (transactionID != 0)
                {
                    recompensationGifts.Add(new RecompensationGiftClientModel(baseGiftableResource, transactionID, type));
                }
            }
        }

        public RecompensationType GetLatestRecompensationType()
        {
            return recompensationGifts[0].type;
        }

        public void ClaimGifts()
        {
            for (int i = 0; i < recompensationGifts.Count; i++)
            {
                recompensationGifts[i].gift.Collect(false);
                recompensationGifts[i].gift.SpawnParticle(Vector2.zero);
            }
            RecompensationGiftsAPI.Instance.DeleteRecompensationGifts(recompensationGifts, () => { Debug.Log("Successfuly removed recompensation"); }, (ex) => { Debug.LogError(ex.Message); });
            recompensationGifts.Clear();
            SaveSynchronizer.Instance.InstantSave();
        }
    }
}

