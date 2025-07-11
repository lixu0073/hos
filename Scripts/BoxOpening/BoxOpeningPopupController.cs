using Hospital.BoxOpening.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Hospital.BoxOpening
{
    public class BoxOpeningPopupController
    {
        BoxOpeningPopupUI view;
        BoxQueue boxQueue = new BoxQueue();
        BaseBoxModel currentBoxModel;
        GiftReward currentReward;
        public bool openingInProgress = false;

        public void SetView()
        {
            if (view != null) return;
            view = UIController.getMaternity.boxOpeningPopupUI;
        }

        public void AddBox(BaseBoxModel boxModel)
        {
            SetView();
            boxQueue.Add(boxModel);
            GetBoxAndStartProcessOpening();
        }

        public void OnBoxOpen()
        {
            UIController.get.storageCounter.SetCounterStartAmount();
            currentBoxModel.Collect();
            ShowNextReward(false);
        }

        public void OnItemAction()
        {
            ShowNextReward();
        }

        private void ShowNextReward(bool collectPrevious = true)
        {
            if(collectPrevious && currentReward != null)
            {
                currentReward.RunCollectAnimation();
            }
            GiftReward reward = currentBoxModel.GetReward();
            if (reward == null)
            {
                if(currentBoxModel != null)
                {
                    OnBoxClose();
                }
                openingInProgress = false;
                bool hasNextBoxToOpen = GetBoxAndStartProcessOpening();
                if (!hasNextBoxToOpen)
                {
                    view.ExitAfterLast();
                }
            }
            else
            {
                currentReward = reward;
                view.SetUpItemView(reward, currentBoxModel);
            }
        }

        private void OnBoxClose()
        {
            if (currentBoxModel != null)
                currentBoxModel.NotifyClose();
        }

        private bool GetBoxAndStartProcessOpening()
        {
            if (openingInProgress) return false;
            BaseBoxModel boxModel = boxQueue.Get();
            if (boxModel == null) return false;
            ProcessOpening(boxModel);
            return true;
        }

        private void ProcessOpening(BaseBoxModel boxModel)
        {
            currentBoxModel = boxModel;
            openingInProgress = true;
            view.SetUpBoxView(currentBoxModel);
        }

        private class BoxQueue
        {
            Queue<BaseBoxModel> queue = new Queue<BaseBoxModel>();

            public void Add(BaseBoxModel boxModel)
            {
                queue.Enqueue(boxModel);
            }

            public BaseBoxModel Get()
            {
                return queue.Count > 0 ? queue.Dequeue() : null;
            }

        }
    }
}
