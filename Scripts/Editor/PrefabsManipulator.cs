using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

public class PrefabsManipulator
{

    private static Material mat;

    [MenuItem("Edit/Set Default Material")]
    static void SetDefaultMaterial()
    {
        
        mat = GameObject.FindWithTag("Miss").GetComponent<Image>().material;
        GameObject go = GameObject.FindWithTag("Prefabs");
        foreach (Transform child in go.transform)
        {
            TryToSetMaterialAndGoDeeper(child);
        }
        Debug.LogError("DONE");
    }

    static void TryToSetMaterialAndGoDeeper(Transform elem)
    {
        Image image = elem.GetComponent<Image>();
        if(image != null)
        {
            image.material = mat;
        }
        SpriteRenderer sr = elem.GetComponent<SpriteRenderer>();
        if(sr != null)
        {
            sr.material = mat;
        }
        foreach (Transform child in elem)
        {
            TryToSetMaterialAndGoDeeper(child);
        }
    }

}
