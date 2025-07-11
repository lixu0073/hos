using UnityEngine;

public class SizeByScale : MonoBehaviour
{
    public Transform scaleParent;
    public bool scaleX = true;
    public bool scaleY = true;
    //float minScale = 0.05f;
    //float maxScale = 1f;

    float normalized = 0f;
    float scale = 1f;
    bool waitForAnim = true;

    void OnEnable()
    {
        transform.localScale = Vector3.one;
        Invoke("Anim", .05f);
    }

    void OnDisable()
    {
        waitForAnim = true;
    }

    void Anim()
    {
        waitForAnim = false;
    }

	void Update()
    {
        if (waitForAnim)
            return;

        normalized = scaleParent.localScale.x / 2f;
        scale = Mathf.Lerp(0.05f, 1f, 1 - normalized);
        if (scaleX && scaleY)
        {
            transform.localScale = Vector3.one * scale;
        }
        else if (scaleX)
        {
            transform.localScale = new Vector3(scale, 1f, 1f);
        }
        else if (scaleY)
        {
            transform.localScale = new Vector3(1f, scale, 1f);
        }
    }
}