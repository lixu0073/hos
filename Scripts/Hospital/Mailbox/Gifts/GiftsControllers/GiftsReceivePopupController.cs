using Hospital;
using System.Collections.Generic;
using UnityEngine;

public class GiftsReceivePopupController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private GiftsReceivePopupUI UI;
#pragma warning restore 0649
    private bool processingGiftCollection = false;

    public void OnEnable()
    {
        UI.RefeshViewAndSetLoaders(GiftsAPI.Instance.GetGifts(), GiftsReceiveController.Instance.ShouldAddGiftFromWise());
        GiftsAPI.Instance.GetGivers(GiftsAPI.Instance.GetGifts(), (givers) =>
        {
            RefreshGivers(givers);
        }, (ex) =>
        {
            Debug.LogError(ex.Message);
            RefreshGivers(new List<Giver>());
        });
        GiftsReceiveController.onGiversUpdate += GiftsReceiveController_onGiftsUpdate;
    }

    public void OnOpenAllGifts()
    {
        if (!processingGiftCollection)
        {
            processingGiftCollection = true;
            GiftsAPI.Instance.GetGivers(GiftsAPI.Instance.GetGifts(), (givers) =>
            {
                GiftsReceiveController.Instance.CollectAll(givers, GiftsReceiveController.Instance.ShouldAddGiftFromWise());
                processingGiftCollection = false;
            }, (ex) =>
            {
                Debug.LogError(ex.Message);
                processingGiftCollection = false;
            });
        }
    }

    private void GiftsReceiveController_onGiftsUpdate(List<Giver> givers)
    {
        RefreshGivers(givers);
    }

    private void RefreshGivers(List<Giver> givers)
    {
        UI.RefreshView(givers, GiftsReceiveController.Instance.ShouldAddGiftFromWise());
    }

    public void OnDisable()
    {
        GiftsReceiveController.onGiversUpdate -= GiftsReceiveController_onGiftsUpdate;
    }
}
