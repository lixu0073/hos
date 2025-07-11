using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SplashScreenController : MonoBehaviour, IPointerClickHandler
{
    public Animator TapToPlayAC;
    public TutorialController TCinstance;
    private AnimationClip hideTapToContinue;
	void Start () 
	{
        TapToPlayAC.SetBool("IsVisible", true);
        foreach (AnimationClip ac in TapToPlayAC.runtimeAnimatorController.animationClips)
        {
            if (ac.name == "HideTapToContinue")
            {
                hideTapToContinue = ac;
                break;
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        TapToPlayAC.SetBool("IsVisible", false);
        yield return new WaitForSeconds(hideTapToContinue.length);
        this.gameObject.SetActive(false);
        //TCinstance.gameObject.SetActive(true);
    }
}
