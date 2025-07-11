using UnityEngine;
using SimpleUI;
using UnityEngine.UI;
using TMPro;
using MovementEffects;
using System.Collections.Generic;

namespace Hospital
{
	public class RenovatePopUpController : UIElement
	{
		[SerializeField] private TextMeshProUGUI cost = null;
		[SerializeField] private TextMeshProUGUI title = null;
		[SerializeField] private Button confirm = null;
#pragma warning disable 0649
        [SerializeField] private Transform ArtImage;
#pragma warning restore 0649
        private string graphicPath = "";
        private GameObject retrievedGameObject;

        private enum State
		{
			idle,
			creation
		}

		public void Open(ExternalRoomInfo externalRoomInfo, OnEvent confirmation)
		{
            StartCoroutine(base.Open(true, false, () =>
            {
                confirm.gameObject.SetActive(true);
                confirm.transform.localScale = new Vector3(1, 1, 1);

                cost.text = externalRoomInfo.RenovationCost.ToString();

                switch (externalRoomInfo.ExternalRoomType)
                {
                    case ExternalRoomType.PlayRoom:
                        graphicPath = "LockedFeatureGameObjects/Kids_Room_Art";
                        Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                        break;
                    case ExternalRoomType.VIPRoom:
                        graphicPath = "LockedFeatureGameObjects/Vip_Art";
                        Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                        break;
                    case ExternalRoomType.Epidemy:
                        graphicPath = "LockedFeatureGameObjects/Epidemic_Art";
                        Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                        break;
                    default:
                        break;
                }
                //lol "RENOWATE"
                title.text = (I2.Loc.ScriptLocalization.Get("RENOWATE") + " " + externalRoomInfo.ExternalRoomType.ToString() + " ?").ToUpper();

                confirm.onClick.RemoveAllListeners();
                confirm.onClick.AddListener(() =>
                {
                    confirmation?.Invoke();
                    Exit();
                });
            }));
		}

        private IEnumerator<float> CreateGraphicObject(string graphicPath)
        {
            ResourceRequest rr = Resources.LoadAsync(graphicPath);
            while (!rr.isDone)
                yield return 0;
            if (!UIController.get.PoPUpArtsFromResources.TryGetValue(graphicPath, out retrievedGameObject))
            {
                retrievedGameObject = Instantiate(rr.asset) as GameObject;
                UIController.get.PoPUpArtsFromResources.Add(graphicPath, retrievedGameObject);
            }
            retrievedGameObject.transform.SetParent(ArtImage);
            retrievedGameObject.transform.localScale = Vector3.one;
            RectTransform rt = retrievedGameObject.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector3.zero;
            rt.offsetMax = Vector3.zero;
            rt.offsetMin = Vector3.zero;
            rt.offsetMax = new Vector2(rt.offsetMin.x, -27);
            rt.offsetMin = new Vector2(rt.offsetMax.x, 26);
            retrievedGameObject.SetActive(true);
            graphicPath = "";
        }

        public void Exit()
		{
			base.Exit();
            NotificationCenter.Instance.KidsUIClosed.Invoke(new KidsUIClosedEventArgs());
        }

        protected override void DisableOnEnd()
        {
            base.DisableOnEnd();
            if (!IsVisible && !IsAnimating)
            {
                HideGameObjectArt();
            }
        }

        private void HideGameObjectArt()
        {
            if (retrievedGameObject)
            {
                retrievedGameObject.SetActive(false);
                retrievedGameObject = null;
            }
        }
    }
}
