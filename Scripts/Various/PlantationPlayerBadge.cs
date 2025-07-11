using UnityEngine;
using System.Collections.Generic;
using MovementEffects;
using UnityEngine.UI;
using TMPro;

namespace Hospital
{
    public class PlantationPlayerBadge : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private Image frame;
#pragma warning restore 0649
        public TextMeshProUGUI login;
        public TextMeshProUGUI level;
        public Image avatar;
        public Sprite defaultAvatar;


        private void Awake()
        {
            Canvas canva = GetComponent<Canvas>();
            if (canva != null)
            {
                canva.worldCamera = ReferenceHolder.Get().worldUICamera;
                canva = null;
            }
        }

        public void SetLogin(string login)
        {
            this.login.text = login;
        }

        public void SetLevel(int level)
        {
            this.level.text = level.ToString();
        }

        public void SetAvatarSprite(Sprite AvatarSprite)
        {
            avatar.sprite = AvatarSprite;
        }

        public void SetFrameSprite(Sprite frameSprite)
        {
            frame.sprite = frameSprite;
        }

        public void SetDefaultAvatar()
        {
            SetAvatarSprite(defaultAvatar);
        }
        
        public void SetDefaultFrame()
        {
            SetFrameSprite(ResourcesHolder.Get().frameData.basicFrame);
        }

        public void LoadFacebookData(string FacebookID)
        {
            CacheManager.GetUserDataByFacebookID(FacebookID, (login, avatar) =>
            {
                if(this != null && this.gameObject != null)
                {
                    SetLogin(login);
                    SetAvatarSprite(avatar);
                    SetFrameSprite(ResourcesHolder.Get().frameData.basicFrame);
                    gameObject.SetActive(true);
                }
            }, (ex) =>
            {
                Debug.LogError(ex.Message);
            });
        }

        public void OnClick()
        {
            Timing.RunCoroutine(OnClickAnimation());
        }

        IEnumerator<float> OnClickAnimation()
        {
            Animator anim = GetComponent<Animator>();
            anim.SetTrigger("Tap");

            yield return Timing.WaitForSeconds(2f);
            gameObject.SetActive(false);
            MessageController.instance.ShowMessage(60);
        }
    }
}
