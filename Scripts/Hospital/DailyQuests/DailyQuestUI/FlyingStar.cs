using System.Collections.Generic;
using UnityEngine;
using SimpleUI;
using MovementEffects;

class FlyingStar : MonoBehaviour
{
    public bool isInUse = false;

    [SerializeField] AnimationCurve speedCurve = null;
    [SerializeField] float[] bezierFactor = new float[7];
    [SerializeField] float[] flyTime = new float[7];
#pragma warning disable 0649
    [SerializeField] Animator anim;
#pragma warning restore 0649
    int day;
    float timeFromStart = 0;
    Vector2 startSize;
    Vector2 endSize;
    Vector2 from;
    Vector2 to;
    Vector2 middle;
    OnEvent start = null;
    OnEvent end = null;
    RectTransform rect;

	//flying to position
    public void Init(int day, Vector2 from, Vector2 to, Vector2 startSize, Vector2 endSize, float delay, OnEvent onStart, OnEvent onEnd)
    {
        isInUse = true;
        this.day = day;
        this.startSize = startSize;
        this.endSize = endSize;
        this.from = from;
        this.to = to;
        start = onStart;
        end = onEnd;

        if(!rect)
            rect = GetComponent<RectTransform>();
        rect.sizeDelta = startSize;
        
        Timing.RunCoroutine(Fly(delay));
    }

    IEnumerator<float> Fly(float delay)
    {
        yield return Timing.WaitForSeconds(delay);
		SoundsController.Instance.PlayFlyingStar ();
		
        start?.Invoke();

        transform.position = from;
        gameObject.SetActive(true);
		SetTriggers ("Idle", "Born");
        SetCurveMiddlePosition();

        timeFromStart = 0;
        while (Time.deltaTime < flyTime[day] - timeFromStart)
        {
            timeFromStart += Time.deltaTime;
            float t = speedCurve.Evaluate(timeFromStart / flyTime[day]);
            
            Vector2 mid1 = Vector2.Lerp(from, middle, t);
            Vector2 mid2 = Vector2.Lerp(middle, to, t);
            Vector2 pos = Vector2.Lerp(mid1, mid2, t);
            transform.position = pos;
            
            if (startSize != endSize)
            {
                Vector2 tempSize = startSize - (startSize - endSize) * t;
                rect.sizeDelta = tempSize;
            }
            yield return 0f;
        }

        transform.position = from;
        
        end?.Invoke();

        gameObject.SetActive(false);
        isInUse = false;
    }
    
    void SetCurveMiddlePosition()
    {
        middle = (from + to) / 2;
        var temp = to - from;
        var temp2 = new Vector2(-temp.y, temp.x);
        middle = (to + from) / 2;
        
        middle += temp2 * bezierFactor[day];
    }

	void SetTriggers (string toReset, string trigger)
    {
		anim.ResetTrigger(toReset);
		anim.SetTrigger(trigger);
	}
}
