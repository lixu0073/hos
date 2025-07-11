using SimpleUI;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Hospital
{
    public class PassFriendCodeController : UIElement
    {
        private const int CODE_LENGHT = 6;
        private const string EMPTY = "";
        private const string ANIMATION_ENTRY_KEY = "PopUpEntryExit";
#pragma warning disable 0649
        [SerializeField] private PassFriendCodeView view;
        [SerializeField] private Animator animator;
#pragma warning restore 0649
        private float animationTime;

        private void OnEnable()
        {
            animationTime = animator.runtimeAnimatorController.animationClips.First(x => x.name == ANIMATION_ENTRY_KEY).length;
            view.CodeInputField.text = EMPTY;
            view.SetButtonClickable(false);
 
            view.OnInputFieldValueChange =
                (inputValue) =>
                    {
                        OnInputFieldValueChange(inputValue);
                    };
            view.ApplyCode = ProccesCode;
            view.Exit = () => Exit();
            StartCoroutine(SelectInputField());

            PersonalFriendCodeProvider.OnFollowerGet += OpenFriendAddedPopup;
        }

        private void OnDisable()
        {
            PersonalFriendCodeProvider.OnFollowerGet -= OpenFriendAddedPopup;
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public void OnInputFieldValueChange(string inputValue)
        {
            view.SetButtonClickable(ContainsValidSingnsAndLenght(inputValue));
        }

        private void ProccesCode()
        {
            ReferenceHolder.Get().personalFriendCodeProvider.GetFollowerByFriendCode(view.CodeInputField.text.ToUpper());
            ShowIap(true);
        }

        private void OpenFriendAddedPopup()
        {
            ShowIap(false);
            Exit();
            StartCoroutine(UIController.getHospital.friendAddingResult.Open());
        }

        private void ShowIap(bool show)
        {
            ReferenceHolder.Get().iapFade.gameObject.SetActive(show);
            ReferenceHolder.Get().iapFade.ToggleText();
        }

        private IEnumerator SelectInputField()
        {
            yield return new WaitForSeconds(animationTime);
            view.CodeInputField.Select();
            TouchScreenKeyboard.Open(EMPTY);
        }

        bool ContainsValidSingnsAndLenght(string input)
        {
            if (input != view.CodeInputField.text.ToUpper())            
                view.CodeInputField.text = view.CodeInputField.text.ToUpper();

            //recursive calling
            input = view.CodeInputField.text;

            if (input.Length != CODE_LENGHT || GameState.Get().PersonalFriendCode == input)            
                return false;

            for (int i = 0; i < input.Length; i++)
            {
                if( !Char.IsUpper(input[i]) && !Char.IsDigit(input[i]))                
                    return false;
            }

            return true;
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            UIController.getHospital.addFriendsPopupController.transform.SetAsLastSibling();
            base.Exit(hidePopupWithShowMainUI);
        }
    }
}
