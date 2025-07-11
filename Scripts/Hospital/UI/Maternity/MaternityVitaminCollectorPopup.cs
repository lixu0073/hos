using Hospital;
using SimpleUI;
using System.Collections.Generic;
using UnityEngine;

public class MaternityVitaminCollectorPopup : UIElement
{
#pragma warning disable 0649
    [SerializeField] private StarsController starController;
    [SerializeField] private PopupTab vitaminContent;
#pragma warning restore 0649
    private VitaminDropAnimationController plusOneDropController;

    protected override void Start()
    {
        base.Start();
    }

    public void Open(MaternityVitaminMaker vitaminMaker, List<VitaminCollectorModel> vitaminModels)
    {
        gameObject.SetActive(true);
        StartCoroutine(base.Open(true, true, () =>
        {
            vitaminContent.SetTabSelected(true);
            SetStars(vitaminMaker.GetMasteryLevel());
            vitaminContent.PopupOpen();
        }));
    }

    public void Exit()
    {
        base.Exit(true);
        vitaminContent.PopupClose();
    }

    private void SetStars(int level)
    {
        //TODO trzeba sprawdzic czy level jest zero na starcie
        //if (level == 0)
        //{
        //    return;
        //}
        starController.SetStarsVisible(level);
    }

    public void SetupVitaminView(List<VitaminCollectorModel> vitaminModels)
    {
        if (vitaminContent is VitaminsPopupTab)
        {
            VitaminsPopupTab vitTab = vitaminContent as VitaminsPopupTab;
            vitTab.SetupVitaminViews(vitaminModels);
        }
    }
}
