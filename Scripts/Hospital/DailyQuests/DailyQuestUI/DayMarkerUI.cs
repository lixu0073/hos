using UnityEngine;
using System.Collections.Generic;
using MovementEffects;

public class DayMarkerUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] Animator anim;
#pragma warning restore 0649

    void SetAnimator (string toReset, string toSet)
	{
		anim.ResetTrigger (toSet);
		anim.SetTrigger (toSet);
	}

    public void SetPosition(DailyQuestUI target, bool instant)
    {
        if (instant)
            transform.position = new Vector2(target.transform.position.x, transform.position.y);
        else
            Timing.RunCoroutine(LerpPosition(target.transform));

        transform.SetParent(target.backgroundTransform);
        transform.SetSiblingIndex(2);
        transform.localScale = Vector3.one;

        //reset Y position to 0, because after the quest card scales up and down it may be wrong
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0);
    }

    IEnumerator<float> LerpPosition(Transform target)
    {
        yield return Timing.WaitForSeconds(.6f);    //time the pop up needs to open
        float startPos = transform.position.x;
        float timer = 0f;
        float duration = 0.7f;
        transform.position = new Vector2(target.position.x, transform.position.y);

		SetAnimator("Idle", "DateChange");

        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = new Vector2(Mathf.Lerp(startPos, target.position.x, timer / duration), transform.position.y);
            yield return Timing.WaitForSeconds(0);
        }

        transform.position = new Vector2(target.position.x, transform.position.y);
    }
}
