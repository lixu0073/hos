using UnityEngine;
using System.Collections;
using TMPro;
using SimpleUI;

public class CorruptedSaveInfoPopupController : UIElement
{
    public TextMeshProUGUI exceptionMsg;
    private bool hasBeenOpened = false;

    public IEnumerator Open(string txt, bool stopGame = false)
    {
        yield return base.Open(true, true);
        exceptionMsg.text = txt;
        hasBeenOpened = true;
        this.isBlockedGame = stopGame;
    }

    public void Exit()
    {
        if (!this.isBlockedGame)
        {
            base.Exit();
        }
    }

    public void Update()
    {
        if (isStopVisibleAnim() && this.isBlockedGame && hasBeenOpened)
        {
            Time.timeScale = 0;
        }
    }
}
