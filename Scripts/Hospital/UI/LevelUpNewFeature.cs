using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using MovementEffects;
using System;
using System.Collections.Generic;

public class LevelUpNewFeature : MonoBehaviour
{

    public TextMeshProUGUI featureNameText;
    public Transform ArtImage;
    //public Image featureArtImage;
    //public Sprite[] featureArtSprites;
    //public Transform ArtTransform;
    [HideInInspector]
    public GameObject retrievedGameObject;
    string graphicPath = "";

    public void Initialize(int level)
    {
        //Debug.LogError("Initialize");
        transform.localScale = Vector3.one;
        transform.SetAsFirstSibling();

        switch (level)
        {
            case 7:
                graphicPath = "LockedFeatureGameObjects/Pharmacy_Art";
                Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                ArtImage.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -12f);
                //featureArtImage.sprite = featureArtSprites[0];
                featureNameText.text = I2.Loc.ScriptLocalization.Get("PHARMACY");
                break;
            case 9:
                graphicPath = "LockedFeatureGameObjects/Vip_Art";
                Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                ArtImage.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20f);
                //featureArtImage.sprite = featureArtSprites[1];
                //featureArtImage.rectTransform.anchoredPosition = new Vector2(0, 20f);
                featureNameText.text = I2.Loc.ScriptLocalization.Get("VIP_ROOM");
                break;
            case 12:
                graphicPath = "LockedFeatureGameObjects/Kids_Room_Art";
                Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                ArtImage.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20f);
                //featureArtImage.sprite = featureArtSprites[2];
                //featureArtImage.rectTransform.anchoredPosition = new Vector2(0, 20f);
                featureNameText.text = I2.Loc.ScriptLocalization.Get("KIDS_ROOM");
                break;
            case 15:
                graphicPath = "LockedFeatureGameObjects/Plantation_Art";
                Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                ArtImage.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20f);
                //featureArtImage.sprite = featureArtSprites[3];
                //featureArtImage.rectTransform.anchoredPosition = new Vector2(0, 20f);
                featureNameText.text = I2.Loc.ScriptLocalization.Get("PLANTATION");
                break;
            case 17:
                graphicPath = "LockedFeatureGameObjects/Epidemic_Art";
                Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                ArtImage.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20f);
                //featureArtImage.sprite = featureArtSprites[4];
                // featureArtImage.rectTransform.anchoredPosition = new Vector2(0, 20f);
                featureNameText.text = I2.Loc.ScriptLocalization.Get("EPIDEMIC_CENTER");
                break;
            case 23:
                graphicPath = "LockedFeatureGameObjects/MaternityWard_Art";
                Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                featureNameText.text = I2.Loc.ScriptLocalization.Get("MATERNITY_CENTER");
                break;
        }
    }

    private IEnumerator<float> CreateGraphicObject(string graphicPath)
    {
        ResourceRequest rr = Resources.LoadAsync(graphicPath);
        while (!rr.isDone)
            yield return 0;
        if (!UIController.get.PoPUpArtsFromResources.TryGetValue(graphicPath, out retrievedGameObject))
        {
            retrievedGameObject = Instantiate(rr.asset) as GameObject;
            UIController.get.PoPUpArtsFromResources.Add(graphicPath, retrievedGameObject);
        }
        retrievedGameObject.transform.SetParent(ArtImage);
        retrievedGameObject.transform.localScale = Vector3.one;
        RectTransform rt = retrievedGameObject.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector3.zero;
        rt.offsetMax = Vector3.zero;
        rt.offsetMin = Vector3.zero;
        retrievedGameObject.SetActive(true);
        //rt.anchoredPosition = new Vector2(0, 20f);
        graphicPath = "";
    }
}
