using UnityEngine;

public class VisualsUpgradeController : MonoBehaviour
{
    [SerializeField]
    private LevelVisualsController[] visualsPerLevel = null;
    
    public void SetLevel(int level)
    {
        if (visualsPerLevel == null)
        {
            return;
        }

        if (level > visualsPerLevel.Length - 1)
        {
            return;
        }

        for (int i = 0; i < visualsPerLevel.Length; ++i)
        {
            visualsPerLevel[i].HideVisuals();
        }

        visualsPerLevel[level].SetVisuals();
        visualsPerLevel[level].ShowVisuals();
    }

    [System.Serializable]
    public class LevelVisualsController
    {
        [SerializeField]
        private VisualControllerSpritePair[] visuals = null;

        public void HideVisuals()
        {
            if (visuals == null)
            {
                return;
            }

            for (int i = 0; i < visuals.Length; ++i)
            {
                visuals[i].HideObject();
            }
        }

        public void ShowVisuals()
        {
            if (visuals == null)
            {
                return;
            }

            for (int i = 0; i < visuals.Length; ++i)
            {
                visuals[i].ShowObject();
            }
        }

        public void SetVisuals()
        {
            if (visuals == null)
            {
                return;
            }

            for (int i = 0; i < visuals.Length; ++i)
            {
                visuals[i].SetObject();
            }
        }
    }

    [System.Serializable]
    public class VisualControllerSpritePair
    {
        [SerializeField]
        private VisualController visualController = null;

        [SerializeField]
        private Sprite spriteToSet = null;

        public void HideObject()
        {
            visualController.HideObject();
        }

        public void ShowObject()
        {
            visualController.ShowObject();
        }

        public void SetObject()
        {
            visualController.SetObject(spriteToSet);
        }
    }

    [System.Serializable]
    public abstract class VisualController : MonoBehaviour
    {
        public abstract void HideObject();
        public abstract void ShowObject();
        public abstract void SetObject(Sprite spriteToSet);
    }
}


