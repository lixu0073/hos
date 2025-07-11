using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleUI
{

    public interface ICard
    {
        void OnDestroy();
    }

    public enum Direction
    {
        NONE,
        NEXT,
        PREV
    }

    public enum Anim
    {
        TOP,
        LEFT,
        RIGHT,
        BOTTOM,
        NONE
    }

    public abstract class MasterDetailElementUI<Card, Model> : UIElement where Card : ICard
    {
        public List<Card> cards = new List<Card>();
        protected Model currentModel = default(Model);

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            PreOpen();
            yield return base.Open(isFadeIn, preservesHovers, null);
            RefreshCollection();
            PostOpen();

            whenDone?.Invoke();
        }

        public void Open(Model model, bool isFadeIn = true, bool preservesHovers = false)
        {
            PreOpen();
            gameObject.SetActive(true);
            StartCoroutine(base.Open(isFadeIn, preservesHovers, () =>
            {
                currentModel = model;
                RefreshCollection();
                PostOpen();
            }));
        }

        protected abstract void PreOpen();
        protected abstract void PostOpen();

        protected void SetCurrent(Model model)
        {
            currentModel = model;
        }

        public Direction GetDirection(Model newModel)
        {
            if(currentModel == null || currentModel.Equals(newModel))
                return Direction.NONE;

            List<Model> models = GetModels();
            int count = models.Count;
            int newModelIndex = models.IndexOf(newModel);
            int prevModelIndex = models.IndexOf(currentModel);
            
            if(prevModelIndex == count - 1 && newModelIndex == 0)
                return Direction.NEXT;

            if(prevModelIndex == 0 && newModelIndex == count - 1)
                return Direction.PREV;

            if(newModelIndex > prevModelIndex)
                return Direction.NEXT;
            
            if(newModelIndex < prevModelIndex)
                return Direction.PREV;

            return Direction.NONE;
        }
        
        public Model GetNextModel()
        {
            List<Model> models = GetModels();
            if (currentModel == null)
            {
                if (models.Count > 0)
                    return models[0];
                else
                    return default(Model);
            }
            else
            {
                bool returnNext = false;
                foreach(Model model in models)
                {
                    if (returnNext)
                        return model;
                    if (model.Equals(currentModel))
                        returnNext = true;
                }
            }
            if (models.Count > 0)
                return models[0];
            return default(Model);
        }

        public Model GetPrevModel()
        {
            List<Model> models = GetModels();
            if (currentModel == null)
            {
                if (models.Count > 0)
                    return models[0];
                else
                    return default(Model);
            }
            else
            {
                bool returnNext = false;
                for(int i = models.Count - 1; i >= 0; --i)
                {
                    if (returnNext)
                        return models[i];
                    if (models[i].Equals(currentModel))
                        returnNext = true;
                }
            }
            if (models.Count > 0)
                return models[models.Count - 1];
            return default(Model);
        }

        protected void ClearContent()
        {
            foreach (Card card in cards)
            {
                card.OnDestroy();
            }
            cards.Clear();
            OnClearContent();
        }

        private void OnDestroy()
        {
            currentModel = default(Model);
        }

        protected abstract void OnClearContent();
        protected abstract List<Model> GetModels();
        protected abstract Card GetDetailCard(Model model, bool IsSelected);
        public abstract void UpdateMasterCard(Model model, bool instant = true, Anim anim = Anim.NONE);
        protected abstract void SetMasterCardEmpty();
        
        protected virtual IEnumerator<float> UpdateMasterCardCoroutine(Model model, bool instant, Anim anim = Anim.NONE)
        {
            yield return 0;
        }

        private void CreateDetailCard(Model model, bool IsSelected)
        {
            cards.Add(GetDetailCard(model, IsSelected));
        }

        public void RefreshCollection()
        {
            ClearContent();
            List<Model> models = GetModels();
            int modelsCount = models.Count;
            if (modelsCount == 0)
            {
                currentModel = default(Model);
                SetMasterCardEmpty();
                return;
            }
            bool elementFound = false;
            if (currentModel != null)
            {
                foreach (Model model in models)
                {
                    if (model.Equals(currentModel))
                    {
                        elementFound = true;
                        break;
                    }
                }
            }
            bool first = true;
            foreach (Model model in models)
            {
                bool selected = (!elementFound && first) || (elementFound && model.Equals(currentModel));
                if (selected)
                {
                    currentModel = model;
                    UpdateMasterCard(model);
                }
                CreateDetailCard(model, selected);
                first = false;
            }
        }

    }
}
