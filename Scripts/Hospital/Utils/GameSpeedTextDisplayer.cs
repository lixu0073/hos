using UnityEngine;

public class GameSpeedTextDisplayer : MonoBehaviour
{
    public UnityEngine.UI.Text GameSpeedTxt;
    private void Update()
    {
        GameSpeedTxt.text = Time.timeScale.ToString("00.00000");
    }

}
