using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BacteriaAvatarBackground : MonoBehaviour
{
#pragma warning disable 0649    
    [SerializeField] Image bgImage;
    [SerializeField] Sprite bgNormal;
    [SerializeField] Sprite bgVip;
#pragma warning restore 0649
    float frequency = 1f;
    bool isActive = false;
    
    float t = 0f;
    Color colorClear = new Color(1, 1, 1, .25f);
    Color colorOpaque = new Color(1, 1, 1, 2f);


    void Awake()
    {
        bgImage.enabled = isActive;
    }

    public void SetBlinking(bool isActive, float frequency, bool isVip)
    {
        this.frequency = frequency;
        this.isActive = isActive;
        bgImage.enabled = isActive;

        if (isVip)
            bgImage.sprite = bgVip;
        else
            bgImage.sprite = bgNormal;
    }

    void Update()
    {
        if (isActive)
            ProcessBlinking();
    }

    void ProcessBlinking()
    {
        if (frequency == 0)
        {
            bgImage.color = colorOpaque;
            return;
        }
        
        t = Time.time % frequency;

        if (t <= .5f)
        {
            t *= 2;
            bgImage.color = Color.Lerp(colorClear, colorOpaque, t);
        }
        else
        {
            t -= .5f;
            t *= 2;
            bgImage.color = Color.Lerp(colorOpaque, colorClear, t);
        }
    }
}
