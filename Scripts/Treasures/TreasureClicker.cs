using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Hospital {

    public class TreasureClicker : MonoBehaviour, IPointerClickHandler
    {

        public void OnPointerClick(PointerEventData pointerEvent)
        {
            HospitalAreasMapController.HospitalMap.treasureManager.TreasureClick();

            if (VisitingController.Instance.IsVisiting)
            {
                if (SaveLoadController.SaveState.ID == "SuperWise")
                    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.DoctorWiseTreasure));

                    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.FindTreasureChestsWhenVisiting));
            }
            else
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.FindTreasureChests));

            NotificationCenter.Instance.TreasureClicked.Invoke(new BaseNotificationEventArgs());
        }
    }
}
