using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hospital
{
    namespace TreatmentRoomHelpRequest
    {

        public class TreatmentHelpNotificationCenter : MonoBehaviour
        {

            public delegate void OnRefresh(List<TreatmentHelpPackage> requests);

            public event OnRefresh OnRequestsRefreshed;
            public event OnRefresh OnRequestsGet;

            public void NotifyRequests(List<TreatmentHelpPackage> requests)
            {
                OnRequestsRefreshed?.Invoke(requests);
            }

            public void NotifyRequestsGet(List<TreatmentHelpPackage> requests)
            {
                OnRequestsGet?.Invoke(requests);
            }

        }
    }
}
