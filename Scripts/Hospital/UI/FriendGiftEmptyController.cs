using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendGiftEmptyController : MonoBehaviour {
    [SerializeField]
    private GameObject emptyText = null;
    [SerializeField]
    private GameObject loader = null;

    public void SetLoaderVisible(bool setVisible) {
        SetEmptyTextActive(!setVisible);
        SetLoaderActive(setVisible);
    }

    private void SetEmptyTextActive(bool setActive) {
        if (emptyText == null) {
            Debug.LogError("emptyText is null");
            return;
        }
        emptyText.SetActive(setActive);
    }

    private void SetLoaderActive(bool setActive) {
        if (loader == null)
        {
            Debug.LogError("loader is null");
            return;
        }
        loader.SetActive(setActive);
    }

}
