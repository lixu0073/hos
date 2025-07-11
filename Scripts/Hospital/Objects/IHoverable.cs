using UnityEngine;

namespace Hospital
{
    public interface IHoverable
    {
        RectTransform GetHoverFrame();
        void MoveCameraToShowHover();
    }
}
