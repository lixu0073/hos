using UnityEngine;

public class CameraMovementVisualEffect : MonoBehaviour, VisualEffect
{
#pragma warning disable 0649
    [SerializeField] float cameraMoveTime;
    [SerializeField] Vector3 cameraMoveOffsetFromCenter;
#pragma warning restore 0649

    public bool HasEnded()
    {
        return ReferenceHolder.Get().engine.MainCamera.CameraMovementStoped;
    }

    public void RunVisualEffect()
    {
        Vector3 gameObjectPosition = gameObject.transform.position;
        Vector3 targetPosition = new Vector3(gameObjectPosition.x + cameraMoveOffsetFromCenter.x, gameObjectPosition.y + cameraMoveOffsetFromCenter.y, gameObjectPosition.z + cameraMoveOffsetFromCenter.z);
        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(targetPosition, cameraMoveTime, TutorialController.Instance.GetCurrentStepData().CameraLocked);
    }
}
