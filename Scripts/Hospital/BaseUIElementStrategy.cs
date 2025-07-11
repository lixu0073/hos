using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class BaseUIElementStrategy<UIElementData, UIElement>
{
    public virtual void SetupPopupAccordingToData(UIElementData data, UIElement uiElement)
    {
        ClearPopup(uiElement);
    }

    public abstract void ClearPopup(UIElement uIElement);
}
