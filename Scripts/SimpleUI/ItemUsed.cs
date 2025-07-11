using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MovementEffects;

namespace SimpleUI
{
	[RequireComponent(typeof(RectTransform))]
	class ItemUsed : MonoBehaviour
	{
		public TextMeshProUGUI text = null;
		public AnimationCurve moveCurve = null;
		//private static System.Random rand = null;
		float time;
		float timeFromStart;
		Vector3 from;
		Vector3 to;
		Image img;
        bool isWorldSpace;

        void Awake()
		{
			img = GetComponent<Image>();
			img.CrossFadeAlpha(1, 0, true);
			text.CrossFadeAlpha(1, 0, true);
		}

		public void Start()
		{
			//if (rand == null)
			//	rand = new System.Random();
		}

		public void StartAnimation(Vector3 Pos, float Time, float Delay, string text = "", bool isWorldSpace = true)
		{
			//if (rand == null)
			//	rand = new System.Random();
            this.isWorldSpace = isWorldSpace;
            this.text.text = text;
			time = Time;
			timeFromStart = 0;
            if(isWorldSpace)
                from = Pos + new Vector3(1, 0, 1);
            else
                from = Pos + new Vector3(0, 125, 0);
            to = Pos;
			gameObject.SetActive(true);

			Timing.RunCoroutine(Anim(Delay));
		}

		IEnumerator<float> Anim(float delay)
        {
            Vector3 pos = from;
            Vector3 startScale = new Vector3(0.02f, 0.02f, 0.02f);
            if (!isWorldSpace)
                startScale = Vector3.one;
            Vector3 targetScale = startScale * .75f;
            Vector3 scale = startScale;

            transform.localScale = scale;
            
            yield return Timing.WaitForSeconds(delay);
            
			float t = 0;

			img.CrossFadeAlpha(0, time, true);
			text.CrossFadeAlpha(0, time, true);

			transform.position = from;
			while (Time.deltaTime < time - timeFromStart)
			{
				timeFromStart += Time.deltaTime;
				t = moveCurve.Evaluate(timeFromStart / time);
				pos = Vector3.Lerp(from, to, t);
				scale = Vector3.Lerp(startScale, targetScale, t);
				transform.position = pos;
				transform.localScale = scale;
				yield return 0f;
			}

			Destroy(gameObject);
		}
	}
}
