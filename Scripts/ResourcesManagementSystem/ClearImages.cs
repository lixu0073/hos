using UnityEngine;
using UnityEngine.UI;

public class ClearImages : MonoBehaviour
{
    public void ClearImagesInChildren()
    {
        Image[] images = GetComponentsInChildren<Image>();

        for (int i = 0; i < images.Length; ++i)
        {
            if (images[i].GetComponent<SpriteAssetManager>() != null)
            {
                images[i].sprite = null;
            }
        }
    }
}
