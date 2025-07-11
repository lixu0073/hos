using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Hospital;

namespace SimpleUI
{
    public class DrawerTabScript : MonoBehaviour
    {
        public Image background;
        public Image icon;
        public Sprite activeImage;
        public Sprite activeImageBg;
        public Sprite inactiveImage;
        public Sprite inactiveImageBg;
        public ScrollRect content;
        [SerializeField]
        public List<DrawerTabEntry> subTabs;
        private int activeTab = -1;

        public void ChangeTabButton(int index)
        {
            //if (UIController.get.drawer.GetActiveTabOnActualTab() == index)
            //{
            //   return;
            //}

            if (Game.Instance.gameState().GetHospitalLevel() < 9 && index == 2)
            {
                MessageController.instance.ShowMessage(string.Format(I2.Loc.ScriptLocalization.Get("UNLOCKS_AT_LEVEL"), 9));
                return;
            }

            if (TutorialController.Instance.CurrentStepBlocksDrawerUI() || (TutorialController.Instance.CurrentTutorialStepTag == StepTag.patio_tidy_5 && TutorialController.Instance.ConditionFulified))
            {
                return;
            }

            ChangeTab(index);
        }

        public void ChangeTab(int index, bool hideBadges = true)
        {
            if (Game.Instance.gameState() is MaternityGameState)
            {
                //if shit is on maternity and tab with hsopital decos is selected
                //code here is such a nigtmare that im affraid to deriver it 
                //Szmury
                if (Game.Instance.gameState().GetMaternityLevel() < 3 && index == 1)
                {
                    MessageController.instance.ShowMessage(string.Format(I2.Loc.ScriptLocalization.Get("UNLOCKS_AT_LEVEL"), "3"));
                    return;
                }
            }

            //SoundsController.Instance.PlayButtonClick(UIController.get.drawer.IsInitalizing());
            if ((index >= 0 && index < subTabs.Count))
            {
                if (activeTab < 0)
                {
                    for (int i = 0; i < subTabs.Count; i++)
                        subTabs[i].SetActive(false);
                }
                else
                {
                    if (hideBadges)
                    {
                        //UIController.get.drawer.HideAllBadgeOnCurrentTab();
                    }
                    subTabs[activeTab].SetActive(false);
                }

                subTabs[index].SetActive(true, content);
                //if (false)
                //    LayoutRebuilder.MarkLayoutForRebuild(content.content);
                activeTab = index;

                //UIController.get.drawer.CenterAtBadgeOnCurrentTab();
            }

            //ResetScrollRectPosition();
            //Invoke("ResetScrollRectPosition", 1f);
        }

        void ResetScrollRectPosition()
        {
            Debug.LogError("ResetScrollRectPosition");
            content.verticalNormalizedPosition = 1;
        }

        public int GetActiveTab()
        {
            return activeTab;
        }

        public void ChangeImage(bool isActive)
        {
            if (icon == null || background == null)
                return;
            if (isActive)
            {
                icon.sprite = activeImage;
                background.sprite = activeImageBg;
            }
            else
            {
                icon.sprite = inactiveImage;
                background.sprite = inactiveImageBg;
            }
        }

        public void AddElement(GameObject element, int tabNumber)
        {
            if (tabNumber < 0 || tabNumber >= subTabs.Count)
                return;
            element.transform.SetParent(subTabs[tabNumber].TabContent.transform);
            element.transform.localPosition = Vector3.zero;
            element.transform.GetComponentInChildren<Hospital.DrawerItem>().SetContainer(subTabs[tabNumber].TabContent.transform.parent.gameObject);
            element.transform.localScale = new Vector3(1, 1, 1);

            element.SetActive(true);
        }

        public void AddElementCopy(GameObject element)
        {
            var p = GameObject.Instantiate(element);

            p.transform.SetParent(GetComponentInChildren<GridLayoutGroup>().gameObject.transform, true);
            p.transform.localScale = Vector3.one;

            p.SetActive(true);

        }

        public void RemoveElement(int index)
        {
            if (index >= 10 && index < transform.childCount)
            {
                transform.GetChild(index).transform.SetParent(null);
            }
        }

        public void SortTabs()
        {
            if (subTabs != null && subTabs.Count > 0)
            {
                for (int i = 0; i < subTabs.Count; i++)
                    subTabs[i].SortBought();
            }
        }

        public void HidePaintBadgeClinic()
        {
            UIController.get.drawer.SetPaintBadgeClinicVisible(false);
        }

        public void HidePaintBadgeLab()
        {
            UIController.get.drawer.SetPaintBadgeLabVisible(false);
        }
    }

    [System.Serializable]
    public struct DrawerTabEntry
    {
        public GameObject TabContent;
        public Image ButtonImage;
        public Sprite ButtonActiveSprite;
        public Sprite ButtonInactiveSprite;
        public Image IconImage;
        public Sprite IconActiveSprite;
        public Sprite IconInactiveSprite;
        public GameObject badge;

        public bool sortListWithBought;

        public void SetActive(bool state, ScrollRect content = null)
        {
            if (IconImage != null)
            {
                if (state)
                {
                    IconImage.sprite = IconActiveSprite;
                    ButtonImage.sprite = ButtonActiveSprite;
                }
                else
                {
                    IconImage.sprite = IconInactiveSprite;
                    ButtonImage.sprite = ButtonInactiveSprite;
                }
            }

            SortBought();

            TabContent.SetActive(state);
            if (state)
            {
                content.content = TabContent.GetComponent<RectTransform>();
                LayoutRebuilder.MarkLayoutForRebuild(content.content);
            }
            TabContent.transform.SetAsFirstSibling();
        }

        public void SortBought()
        {
            if (sortListWithBought)
            {
                int posId = 0;

                foreach (Transform child in TabContent.gameObject.transform)
                {
                    var currentItem = child.gameObject.GetComponentInChildren<DrawerItem>();
                    if (currentItem.isStored())
                    {
                        child.SetSiblingIndex(posId);
                        posId++;
                    }
                }
            }
        }
    }
}