using Hospital;
using SmartUI;
using UnityEngine;
using UnityEngine.UI;
using System;

public abstract class BaseFriendCardController : IItemViewHolder<IFollower>
{
    protected const string ADD_ERRROR = "can't add friend error ";
    protected const string REMOVE_ERRROR = "can't remove friend error ";
    protected const string TAP_AGAIN_KEY = "SOCIAL_MAILBOX_REQUEST_CANCEL_CONFIRM";
    protected const string REQUEST_CANCELED_KEY = "SOCIAL_MAILBOX_REQUEST_CANCELED";
    protected const string REQUEST_REJECTED_KEY = "SOCIAL_REQUEST_DENIED";
    protected const string REQUEST_ACCEPTED_KEY = "SOCIAL_REQUEST_ACCEPTED";

    protected abstract void OnVisiting();
    protected abstract void AddAdditionalBehaviours(IFollower person);
    protected IFollower person;
    protected VisitingEntryPoint entryPoint;
    protected FriendCardUI friendUI;

    public void Initialize(IFollower person, VisitingEntryPoint visitingEntryPoint)
    {
        entryPoint = visitingEntryPoint;
        this.person = person;
        OnViewCreate();
    }

    public override void OnViewCreate()
    {
        gameObject.SetActive(true);
        SetCardData(person);
        if (DisplayInFacebookMode(person))
            CheckAndDnownloadFacebook(person);

        ResubcbeToEvents();
        AddOnImageClick(entryPoint);
        AddAdditionalBehaviours(person);
    }

    public override void OnViewDestroy()
    {
        gameObject.SetActive(false);
    }

    public override void SetModel(IFollower model)
    {
        person = model;
    }

    private void CacheManager_onPublicSaveUpdate(PublicSaveModel model)
    {
        if (model.SaveID != person.GetSaveID())
            return;
        if (person != null)
        {
            person.SetSave(model);
            SetCardData(person);
        }
    }

    private void CheckAndDnownloadFacebook(IFollower personFB)
    {
        if (!personFB.IsFacebookDataDownloaded())
        {
            personFB.DownloadFacebookData(
                () =>
                {
                    if (personFB != null)
                    {
                        SetCardData(personFB);
                    }
                }
                , (ex) =>
                {
                    if (personFB != null)
                    {
                        SetCardData(personFB);
                    }
                });
        }
    }

    protected virtual void AddOnImageClick(VisitingEntryPoint visitingEntryPoint)
    {
        Button button = gameObject.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            Visit(visitingEntryPoint);
        });
    }

    public void Visit(VisitingEntryPoint visitingEntryPoint)
    {
        if (!VisitingController.Instance.canVisit)
            return;

        if (!VisitingController.Instance.IsVisiting || (VisitingController.Instance.IsVisiting && SaveLoadController.SaveState.ID != person.GetSaveID()))
        {
            OnVisiting();
            VisitingController.Instance.Visit(person.GetSaveID());
            AnalyticsController.instance.ReportSocialVisit(visitingEntryPoint, person.GetSaveID());
        }
    }

    private void ResubcbeToEvents()
    {
        ((BaseFollower)person).onHelpRequestStatusChanged -= BaseFriendCardController_onHelpRequestStatusChanged;
        ((BaseFollower)person).onHelpRequestStatusChanged += BaseFriendCardController_onHelpRequestStatusChanged;
        CacheManager.onPublicSaveUpdate -= CacheManager_onPublicSaveUpdate;
        CacheManager.onPublicSaveUpdate += CacheManager_onPublicSaveUpdate;
    }

    protected virtual void SetCardData(IFollower personFB)
    {
        if (this != null)
        {
            GetFriendUI();
            if (friendUI == null)
                return;

            friendUI.SetHospitalNameText(personFB.Name);
            friendUI.SetHospitalLevelText(personFB.Level.ToString());
            try
            {
                if (personFB.Avatar != null)
                {
                    friendUI.SetHospitalPicture(personFB.Avatar);
                }
            }
            catch(Exception)
            {
                //Debug.LogWarning("Something was wrong with Fb pic.: " + e.Message);
            }
            friendUI.SetHelpBadges(personFB.HasPlantationHelpRequest, personFB.HasEpidemyHelpRequest, personFB.HasTreatmentHelpRequest);
            friendUI.frame.sprite = personFB.GetFrame();
        }
        else
        {
            Debug.LogError("controller is null");
        }
    }

    private void BaseFriendCardController_onHelpRequestStatusChanged(bool isPlantationHelpRequested, bool isEpidemyHelpRequested, bool isTreatmentHelpRequested)
    {
        DisplayHelpRequests(isPlantationHelpRequested, isEpidemyHelpRequested, isTreatmentHelpRequested);
    }

    private void DisplayHelpRequests(bool isPlantationHelpRequested, bool isEpidemyHelpRequested, bool isTreatmentHelpRequested)
    {
        if (gameObject != null)
        {
            FriendCardUI controller = gameObject.GetComponent<FriendCardUI>();
            controller.SetHelpBadges(isPlantationHelpRequested, isEpidemyHelpRequested, isTreatmentHelpRequested);
        }
        else
            Debug.LogError("controller is null");
    }

    protected virtual bool DisplayInFacebookMode(IFollower personFB)
    {
        return personFB.IsFacebookConnected() && AccountManager.Instance.IsFacebookConnected;
    }

    protected virtual void OnDestroy()
    {
        ((BaseFollower)person).onHelpRequestStatusChanged -= BaseFriendCardController_onHelpRequestStatusChanged;
        CacheManager.onPublicSaveUpdate -= CacheManager_onPublicSaveUpdate;
    }

    protected void GetFriendUI()
    {
        if (friendUI == null)
            friendUI = gameObject.GetComponent<FriendCardUI>();
    }
}
