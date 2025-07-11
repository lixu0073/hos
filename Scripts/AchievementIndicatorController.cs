using UnityEngine;

/// <summary>
/// 成就指示器控制器，用于处理成就系统在游戏世界中的显示和交互。
/// </summary>
public class AchievementIndicatorController : MonoBehaviour
{
    // 鼠标第一次点击的位置
    private Vector3 firstMousePos;
    // 触摸持续时间
    private float touchTime;

    /// <summary>
    /// 处理点击事件，打开成就弹窗。
    /// </summary>
	public void Clicked()
    {
        StartCoroutine(UIController.getHospital.AchievementsPopUp.Open(() =>
        {
            gameObject.SetActive(false);
        }));
    }

    /// <summary>
    /// 激活或禁用成就指示器。
    /// </summary>
    /// <param name="setActive">是否激活</param>
    public void Activate(bool setActive)
    {
        if (Hospital.HospitalAreasMapController.HospitalMap.VisitingMode)
        {
            gameObject.SetActive(false);
        }
        else
        {
            if (Game.Instance.gameState().GetHospitalLevel() > 2 && setActive)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    // 当鼠标按下时调用
    void OnMouseDown()
    {
        touchTime = Time.time;
        firstMousePos = Input.mousePosition;
    }

    // 当鼠标释放时调用
    void OnMouseUp()
    {
        if (!IsoEngine.BaseCameraController.IsPointerOverInterface())
            if ((Input.mousePosition - firstMousePos).magnitude < 10.0f && Time.time - touchTime < 0.5f)
                Clicked();
    }
}