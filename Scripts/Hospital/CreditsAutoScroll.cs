using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Adds autoscroll behaviour, to the required ScrollRect component, in the GameObject where this is used
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class CreditsAutoScroll : MonoBehaviour
{
    public float timeToStartScrolling = 3f;         // Waiting time until the scrollRect starts to be autoscrolled
    public float timeToResetScrollingPosition = 3f; // Waiting time since scroll reaches the bottom and returns to its top position
    public Vector2 scrollVelocity = new Vector2(0f, 15f); // Scrolling velocity
    public bool scrollRewind = false;
    [HideInInspector]
    public Vector2 scrollRewindVelocity = new Vector2(0f, -800f); // Scrolling rewing (down to top) velocity
    private ScrollRect _scrollRectComponent;
    private bool _startScrolling = false, _startRewing = false; 
    private Coroutine _lerpCoroutine = null, _resetCoroutine = null;
    [HideInInspector]
    public GameObject creditsListObj = null;
    private TMPro.TextMeshProUGUI creditsListText = null;

    public void Start()
    {
        _scrollRectComponent = GetComponent<ScrollRect>();
    }

    public void OnEnable()
    {        
        _lerpCoroutine = StartCoroutine(LerpCredits()); // Launches the AutoScroll process
    }

    public void OnDisable()
    {        
        if (_lerpCoroutine != null)
        {
            StopCoroutine(_lerpCoroutine);
            _lerpCoroutine = null;
            _startScrolling = false;
        }
        if (_resetCoroutine != null)
        {
            StopCoroutine(_resetCoroutine);
            _resetCoroutine = null;
            _startRewing = false;
        }
    }

    // Waits "timeToStartScrolling" secs and enables the scrolling process
    IEnumerator LerpCredits()
    {
        yield return new WaitForSeconds(timeToStartScrolling);
        _startScrolling = true;
    }

    // Waits "timeToResetScrollingPosition" secs and resets the scrollRect position to its top position
    IEnumerator ResetCredits()
    {
        yield return new WaitForSeconds(timeToResetScrollingPosition);
        if (scrollRewind)
        {
            _scrollRectComponent.verticalNormalizedPosition = 0.01f;
            _startRewing = true;
        }
        else
        {
            creditsListText = creditsListObj.GetComponent<TMPro.TextMeshProUGUI>();
            if (creditsListText)
                creditsListText.enabled = false;
            _scrollRectComponent.verticalNormalizedPosition = 1f;
            if (creditsListText)
                creditsListText.enabled = true;            
        }
            
    }

    private void Update()
    {
        if (_startScrolling)
        {
            //Debug.LogErrorFormat("<color=cyan> SB - VerticalNomalizedPosition: {0} </color>", _scrollRectComponent.verticalNormalizedPosition);
            _scrollRectComponent.velocity = scrollVelocity;

            if (_scrollRectComponent.verticalNormalizedPosition <= 0.0f)
            {
                _scrollRectComponent.velocity = Vector2.zero;
                _startScrolling = false;                
                _resetCoroutine = StartCoroutine(ResetCredits());                
            }
        }
        else if (_startRewing)
        {
            _scrollRectComponent.velocity = scrollRewindVelocity;
            if (_scrollRectComponent.verticalNormalizedPosition >= 1.0f)
            {
                _scrollRectComponent.velocity = Vector2.zero;
                _startRewing = false;
            }             
        }
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(CreditsAutoScroll))]
public class CreditsAutoScrollInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CreditsAutoScroll myScript = target as CreditsAutoScroll;

        if (myScript.scrollRewind)        
            myScript.scrollRewindVelocity = EditorGUILayout.Vector2Field("Scroll Rewing Velocity", myScript.scrollRewindVelocity);
        else
            myScript.creditsListObj = (GameObject)EditorGUILayout.ObjectField("CreditList Object", myScript.creditsListObj, typeof(GameObject), true);

        if (GUI.changed)        
            EditorUtility.SetDirty(target);        
    }
}
#endif
