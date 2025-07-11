using UnityEngine;
using System.Collections;
using TMPro;
using System;

public class Message : MonoBehaviour
{
    RectTransform tr;
    [SerializeField]
    TextMeshProUGUI text = null;
#pragma warning disable 0649
    [SerializeField]
    Animator anim;
#pragma warning restore 0649
    float timeout;
    Vector2 speed = new Vector2(0, .75f);
    Coroutine floatUpCoroutine;
    Coroutine fadeOutCoroutine;

    void Awake()
    {
        tr = GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void Initialize(string message)
    {
        tr.anchoredPosition = Vector3.zero;
        text.text = message;
        timeout = 2.25f;
        try
        { 
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
        fadeOutCoroutine = StartCoroutine(FadeOut());

        try
        { 
            if (floatUpCoroutine != null)
            {
                StopCoroutine(floatUpCoroutine);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
        floatUpCoroutine = StartCoroutine(FloatUp());

        tr.SetAsLastSibling();
        anim.SetTrigger("Bump");
    }

    IEnumerator FloatUp()
    {
        while (timeout > 0)
        {
            tr.anchoredPosition += speed;
            timeout -= Time.deltaTime;
            yield return null;
        }

        floatUpCoroutine = null;
        gameObject.SetActive(false);
    }

    IEnumerator FadeOut()
    {
        text.CrossFadeAlpha(1, 0, true);
        yield return new WaitForSeconds(.5f);
        text.CrossFadeAlpha(0, timeout - .5f, true);
        yield return new WaitForSeconds(timeout - .5f);
        fadeOutCoroutine = null;
    }

    public bool isEnded()
    {
        return fadeOutCoroutine == null;
    }
}
