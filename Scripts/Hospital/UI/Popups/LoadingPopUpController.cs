using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;

namespace Hospital
{
    public class LoadingPopUpController : MonoBehaviour
    {
        public Image progressFill;
        public TextMeshProUGUI progressText;
        public TextMeshProUGUI tipText;
        public TextMeshProUGUI supportCodeText;

        private float globalProgress = 0;
        private float loadAssetsTime = 0;

        public GameObject InternetConnectionProblemIcon;
        public GameObject ServerProblemIcon;
        public GameObject ProgressBarContainer;

        private int fromProgress = 0;
        private int toProgress = 100;
        private float estimatedTime = 10;

        public enum State
        {
            NONE = 1,
            LOADING = 2,
            SERVER_ERROR = 3,
            NO_INTERNET = 4
        };

        private State currentState = State.NONE;

        void Start()
        {
            CognitoEntry.OnUserIDRetrieval -= CognitoEntry_OnRetrieval;
            CognitoEntry.OnUserIDRetrieval += CognitoEntry_OnRetrieval;

            Open(90, 100, 4);
        }

        private void CognitoEntry_OnRetrieval(string hash)
        {
            string supportCode = LoadingGame.GenerateSupportCode(hash);
            if (!string.IsNullOrEmpty(supportCode))
            {
                supportCodeText.text = "Id: " + supportCode;
                LoadingGame.TryToSendSupportCodeToAnalytics(supportCode);
            }
        }
        
        public void Exit()
        {
            gameObject.SetActive(false);
            StopTask();
        }

        public void Open(int from, int to, float time)
        {
            TryToActivate();
            fromProgress = from;
            toProgress = to;
            estimatedTime = time;
            progressText.text = I2.Loc.ScriptLocalization.Get("LOADING");
            if (IsProgressbarCompleted())
                return;

            ShowProgress();
        }

        private void TryToActivate()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        private bool IsProgressbarCompleted()
        {
            if (globalProgress >= 100)
            {
                RenderComplete();
                StopTask();
                return true;
            }
            return false;
        }

        private void RenderComplete()
        {
            progressFill.fillAmount = 1;
            //progressText.text = "100%";
        }
        
        public void GetPartialProgress(int minProgress, int maxProgress, float deltaTime, float expectedTime = 1)
        {
            double result = Math.Atan(deltaTime * 10 / expectedTime) / (Math.PI * 0.5);
            globalProgress = ((maxProgress - minProgress) * (float)result + minProgress) / 100;
        }

        void Update()
        {
            TryToUpdateProgressBar();
        }

        public void RunFakeLoading()
        {
            if(globalProgress >= 100 || !isActiveAndEnabled)
            {
                SaveSynchronizer.Instance.HideClouds();
                Exit();
            }
            else
            {
                StopTask();
                StartCoroutine("RunFakeLoadingCoroutine");
            }
        }

        float loadFakeTime = 0;
        private IEnumerator RunFakeLoadingCoroutine()
        {
            float cacheGlobalProgress = globalProgress;
            loadFakeTime = 0;
            while (true)
            {
                GetPartialProgress((int)(cacheGlobalProgress*100), 100, loadFakeTime, 0.2f);
                loadFakeTime += 0.01f;
                yield return 0;
                if (globalProgress >= 0.99f)
                {
                    globalProgress = 1;
                    SaveSynchronizer.Instance.HideClouds();
                    try
                    { 
                        StopCoroutine("RunFakeLoadingCoroutine");
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                    }
                    Exit();
                }
            }
        }

        private void TryToUpdateProgressBar()
        {
            if (currentState == State.LOADING)
            {
                progressFill.fillAmount = globalProgress;
                //progressText.text = (globalProgress * 100).ToString("F0") + "%";
                IsProgressbarCompleted();
            }
        }

        IEnumerator LoadCoroutine()
        {
            loadAssetsTime = 0;
            while (true)
            {
                GetPartialProgress(fromProgress, toProgress, loadAssetsTime, estimatedTime);
                loadAssetsTime += 0.01f;
                yield return 0;
            }
        }

        public void ShowInternetConnectionProblem()
        {
            StopTask();
            currentState = State.NO_INTERNET;
            ShowInternetConnectionProblemIcon();
        }

        public void ShowServerProblem(string message = null)
        {
            StopTask();
            currentState = State.SERVER_ERROR;
            ShowServerProblemIcon();
            if (!string.IsNullOrEmpty(message))
            {
				tipText.gameObject.GetComponent<LoadingHintController>().HideHints();
                tipText.text = message;
            }
        }

        public void ShowProgress()
        {
            globalProgress = fromProgress / 100;
            currentState = State.LOADING;
            ShowProgressBar();
            StartTask();
        }

        private void ShowInternetConnectionProblemIcon()
        {
			tipText.gameObject.GetComponent<LoadingHintController>().HideHints();
            ProgressBarContainer.SetActive(false);
            ServerProblemIcon.SetActive(false);
            InternetConnectionProblemIcon.SetActive(true);
        }

        private void ShowServerProblemIcon()
        {
			tipText.gameObject.GetComponent<LoadingHintController>().HideHints();
            ProgressBarContainer.SetActive(false);
            InternetConnectionProblemIcon.SetActive(false);
            ServerProblemIcon.SetActive(true);
        }

        private void ShowProgressBar()
        {
           // tipText.text = string.IsNullOrEmpty(customText) ? LoadingGame.LoadingText : customText;
			tipText.gameObject.GetComponent<LoadingHintController>().ShowHints();
            InternetConnectionProblemIcon.SetActive(false);
            ServerProblemIcon.SetActive(false);
            ProgressBarContainer.SetActive(true);
        }

        private void StopTask()
        {
            try
            { 
                StopCoroutine("LoadCoroutine");
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }

        private void StartTask()
        {
            StartCoroutine("LoadCoroutine");
        }
    }
}