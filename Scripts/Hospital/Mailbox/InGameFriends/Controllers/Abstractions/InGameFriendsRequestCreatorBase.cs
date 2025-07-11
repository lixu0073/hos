using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public abstract class InGameFriendsRequestCreatorBase : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private GameObject CardPrefab;
        [SerializeField] private Transform content;
#pragma warning restore 0649
        protected List<IFollower> friendsRequests;

        private BaseFriendCardController controller;
        private bool initalized = false;

        //Only overide those
        protected abstract void SubscribeToEvents();
        protected abstract void UnsubscribeFromEvents();
        protected abstract void GetList();
        protected abstract void OnInitalize();
        protected abstract System.Type GetCardType();


        private void Start()
        {
            OnInitalize();
            SubscribeToEvents();
            OnChange();
            initalized = true;
        }

        public void OnEnable()
        {
            if (initalized)
                OnChange();
        }

        protected void OnChange()
        {
            GetList();

            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            foreach (IFollower request in friendsRequests)
            {
                (Instantiate(CardPrefab, content).AddComponent(GetCardType()) as InGameFriendCardController)
                    .Initialize(request, VisitingEntryPoint.InGameFriendRequest);
            }

            AdditionalSetup();
        }

        protected virtual void AdditionalSetup() { }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
    }
}