using Hospital;
using SmartUI;
using UnityEngine;

public class RecycledViewMenagerFriends : RecycledViewManager
{
#pragma warning disable 0649
    [SerializeField] private GameObject drWiseGameobject;
#pragma warning restore 0649
    private IFollower drWiseFriend;

    protected override void Start()
    {
        adapter = new ScrollRectAdapterHorizontal<IFollower>(scrollRect, scrollSettings);
        drWiseFriend = FriendsDataZipper.GetDrWise();
        adapter.onListUpdate += MoveWiseToTop;
        drWiseGameobject.transform.localPosition = new Vector3(
            drWiseGameobject.transform.localPosition.x + scrollSettings.spacingStart,
            drWiseGameobject.transform.localPosition.y,
            drWiseGameobject.transform.localPosition.z
            );
    }

    //it is hack for wise
    //Wise item can't be added to recyler view with so much unqiue contraints
    //it would require a lot of refactor and it would made a code non readable
    //it makes second wise icon appear on top of first one 
    private void MoveWiseToTop()
    {
        drWiseGameobject.transform.SetAsLastSibling();
    }

    private void OnDestroy()
    {
        try
        {
            adapter.onListUpdate -= MoveWiseToTop;
        }
        catch { }
    }
}
