using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IsoEngine;
using TMPro;
using MovementEffects;
using UnityEngine.SceneManagement;

namespace SimpleUI
{
    [RequireComponent(typeof(RectTransform))]
    class GiftMoving : MonoBehaviour
    {
        public Image giftIcon = null;
        public TextMeshProUGUI text = null;
        // public TextMeshProUGUI bonus = null;
        [SerializeField] GameObject bonus = null;
        public AnimationCurve moveCurve = null;
        float time;
        float timeFromStart = 0;
        GiftType type;
        Vector3 worldFromPos;
        Vector3 worldToPos;
        Vector2 from;
        Vector2 to;
        Vector2 middle;
        OnEvent end = null;
        OnEvent start = null;
        RectTransform rect;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            giftIcon.CrossFadeAlpha(0, 0, true);
            text.CrossFadeAlpha(0, 0, true);
            bonus.GetComponent<Image>().CrossFadeAlpha(1, .25f, true);
        }

        public void Start()
        {
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        //flying to UI Canvas position
        public void StartAnimation(GiftType type, Vector3 worldFromPos, Vector2 to, float time, float delay, Vector3 startScale, Vector3 endScale, string text = "", OnEvent onStart = null, OnEvent onEnd = null)
        {
            this.text.text = text;
            this.time = time;
            this.worldFromPos = worldFromPos;
            this.to = to;
            this.type = type;
            start = onStart;
            end = onEnd;
            giftIcon.transform.localScale = startScale;

            if (type == GiftType.Panacea)
            {
                transform.localScale = Vector3.one * 0.8f;
            }

            if (type == GiftType.Special)
            {
                bonus.gameObject.SetActive(true);
                this.time *= 1.5f;
            }
            else
            {
                bonus.gameObject.SetActive(false);
            }

            Timing.RunCoroutine(Anim(delay, startScale, endScale, false));
        }


        //flying to world position (i.e. kids room)
        public void StartAnimation(GiftType type, Vector3 worldFromPos, Vector3 worldToPos, float time, float delay, Vector3 startScale, Vector3 endScale, string text = "", OnEvent onStart = null, OnEvent onEnd = null)
        {
            //Debug.LogError("StartAnimation world pos: " + worldToPos);
            this.text.text = text;
            this.time = time;
            this.worldFromPos = worldFromPos;
            this.worldToPos = worldToPos;
            this.type = type;
            start = onStart;
            end = onEnd;
            giftIcon.transform.localScale = startScale;

            if (type == GiftType.Panacea)
            {
                transform.localScale = Vector3.one * 0.8f;
            }

            if (type == GiftType.Special)
            {
                bonus.gameObject.SetActive(true);
                this.time *= 1.5f;
            }
            else
            {
                bonus.gameObject.SetActive(false);
            }

            Timing.RunCoroutine(Anim(delay, startScale, endScale, true));
        }

        public void StartItemMoveAnimation(GiftType type, Vector2 from, Vector2 to, float time, float delay, Vector3 startScale, Vector3 endScale, string text = "", OnEvent onStart = null, OnEvent onEnd = null)
        {
            this.text.text = text;
            this.time = time;
            this.from = from;
            this.to = to;
            this.type = type;
            start = onStart;
            end = onEnd;
            giftIcon.transform.localScale = startScale;

            if (type == GiftType.Panacea)
            {
                transform.localScale = Vector3.one * 0.8f;
            }

            if (type == GiftType.Special)
            {
                bonus.gameObject.SetActive(true);
                this.time *= 1.5f;
            }
            else
            {
                bonus.gameObject.SetActive(false);
            }

            Timing.RunCoroutine(AnimItemMove(delay, startScale, endScale));
        }

        private Vector2 GetStartPos()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name == "MainScene")
                return Utils.GetScreenPosition(worldFromPos);
            return new Vector2(0, 0);
        }

        IEnumerator<float> Anim(float delay, Vector3 startScale, Vector3 endScale, bool isWorldTo)
        {
            yield return Timing.WaitForSeconds(delay);
            start?.Invoke();
            from = GetStartPos();
            transform.position = from;
            // Workaround for text sometimes not fading in correctly
            gameObject.SetActive(false);
            gameObject.SetActive(true);

            if (!isWorldTo)
                SetCurveMiddlePosition();

            giftIcon.CrossFadeAlpha(1, .25f, true);
            text.CrossFadeAlpha(1, .25f, true);
            if (bonus.gameObject.activeSelf)
                bonus.GetComponent<Image>().CrossFadeAlpha(1, .25f, true);

            bool alphaFaded = false;

            while (Time.deltaTime < time - timeFromStart)
            {
                timeFromStart += Time.deltaTime;
                float t = moveCurve.Evaluate(timeFromStart / time);

                if (isWorldTo)
                {
                    //Debug.LogError("Flying to world pos! " + worldFromPos + worldToPos);
                    rect.anchoredPosition = Vector3.Lerp(GetStartPos(), Utils.GetScreenPosition(worldToPos), t);
                }
                else
                {
                    Vector2 mid1 = Vector2.Lerp(from, middle, t);
                    Vector2 mid2 = Vector2.Lerp(middle, to, t);
                    Vector2 pos = Vector2.Lerp(mid1, mid2, t);
                    rect.anchoredPosition = pos;
                }

                if (!alphaFaded && t > .935f)
                {
                    giftIcon.CrossFadeAlpha(0, time * 0.65f, true);
                    alphaFaded = true;
                }
                if (startScale != endScale)
                {
                    giftIcon.transform.localScale = new Vector3(startScale.x - (startScale.x - endScale.x) * t, startScale.y - (startScale.y - endScale.y) * t, startScale.z - (startScale.z - endScale.z) * t);
                }
                if (type == GiftType.Special)
                {
                    float scale = 1;
                    if (t < .5f)
                    {
                        scale = 1 + t * 2;
                    }
                    else
                    {
                        scale = 3 - t * 2;
                    }
                    transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(scale, scale, scale), .2f);
                }
                yield return 0f;
            }


            RunItemAddedAnim();

            end?.Invoke();
            Destroy(gameObject);
        }

        IEnumerator<float> AnimItemMove(float delay, Vector3 startScale, Vector3 endScale)
        {
            yield return Timing.WaitForSeconds(delay);
            start?.Invoke();

            transform.position = from;

            SetCurveMiddlePosition();

            giftIcon.CrossFadeAlpha(1, .25f, true);
            text.CrossFadeAlpha(1, .25f, true);
            if (bonus.gameObject.activeSelf)
            {
                bonus.GetComponent<Image>().CrossFadeAlpha(1, .25f, true);
            }

            bool alphaFaded = false;

            while (Time.deltaTime < time - timeFromStart)
            {
                timeFromStart += Time.deltaTime;
                float t = moveCurve.Evaluate(timeFromStart / time);
                Vector2 mid1 = Vector2.Lerp(from, middle, t);
                Vector2 mid2 = Vector2.Lerp(middle, to, t);
                Vector2 pos = Vector2.Lerp(mid1, mid2, t);
                rect.anchoredPosition = pos;

                if (!alphaFaded && t > .935f)
                {
                    giftIcon.CrossFadeAlpha(0, time * 0.65f, true);
                    alphaFaded = true;
                }
                if (startScale != endScale)
                {
                    giftIcon.transform.localScale = new Vector3(startScale.x - (startScale.x - endScale.x) * t, startScale.y - (startScale.y - endScale.y) * t, startScale.z - (startScale.z - endScale.z) * t);
                }
                if (type == GiftType.Special)
                {
                    float scale = 1;
                    if (t < .5f)
                    {
                        scale = 1 + t * 2;
                    }
                    else
                    {
                        scale = 3 - t * 2;
                    }
                    transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(scale, scale, scale), .2f);
                }
                yield return 0f;
            }

            RunItemAddedAnim();

            end?.Invoke();
            Destroy(gameObject);
        }

        void SetCurveMiddlePosition()
        {
            middle = (from + to) / 2;
            var temp = to - from;
            var temp2 = new Vector2(-temp.y, temp.x);
            middle = (to + from) / 2;

            if (!Hospital.AreaMapController.Map.VisitingMode)
            {
                if (type == GiftType.Panacea)
                    middle += temp2 * 0.25f * BaseGameState.RandomFloat(-3f, 0f);
                else if (type == GiftType.Exp)
                    middle += temp2 * 0.25f * BaseGameState.RandomFloat(1f, 4f);        //exp only curves to the left
                else if (type == GiftType.Medicine || type == GiftType.Reputation)
                    middle += temp2 * 0.25f * BaseGameState.RandomFloat(-3f, 0f);       //meds only curve mostly to the right
                else if (type == GiftType.Drawer)
                    middle += temp2 * 0.25f * BaseGameState.RandomFloat(-2f, 2f);
                else if (type == GiftType.Coin)
                    middle += temp2 * 0.25f * BaseGameState.RandomFloat(-2f, 2f);
                else if (type == GiftType.Diamond)
                    middle += temp2 * 0.25f * BaseGameState.RandomFloat(-2f, 2f);
            }
            else
            {
                middle += temp2 * 0.25f * BaseGameState.RandomFloat(-1.5f, -0f); //visiting mode
            }
        }

        void RunItemAddedAnim()
        {
            switch (type)
            {
                case GiftType.Coin:
                    ReferenceHolder.Get().giftSystem.giftables[0].RunItemAddedAnimation();
                    break;
                case GiftType.Diamond:
                    ReferenceHolder.Get().giftSystem.giftables[1].RunItemAddedAnimation();
                    break;
                case GiftType.Exp:
                    ReferenceHolder.Get().giftSystem.giftables[2].RunItemAddedAnimation();
                    break;
                case GiftType.Medicine:
                    ReferenceHolder.Get().giftSystem.giftables[4].RunItemAddedAnimation();
                    break;
                case GiftType.Panacea:
                    ReferenceHolder.Get().giftSystem.giftables[4].RunItemAddedAnimation();
                    break;
                case GiftType.Special:
                    ReferenceHolder.Get().giftSystem.giftables[4].RunItemAddedAnimation();
                    break;
                case GiftType.Drawer:
                    ReferenceHolder.Get().giftSystem.giftables[3].RunItemAddedAnimation();
                    break;
                case GiftType.Booster:
                    ReferenceHolder.Get().giftSystem.giftables[5].RunItemAddedAnimation();
                    break;
                case GiftType.BloodTest:
                    ReferenceHolder.Get().giftSystem.giftables[9].RunItemAddedAnimation();
                    break;
            }
        }

    }
}
