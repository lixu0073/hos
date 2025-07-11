using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using AssetBundles;
using IsoEngine;
using UnityEngine.Events;

public class RedirectToMaternity : SceneLoader, LoadingGame.IProgressable
{
#pragma warning disable 0649
    [SerializeField]
    private AlertPopupController alertPopup;
    [SerializeField]
    private EngineController engine;
#pragma warning restore 0649

    public class LoadMaternitySceneOperation : LoadingGame.BasePartOperation
    {
        AsyncOperation asyncOperation = null;

        public LoadMaternitySceneOperation(MonoBehaviour progressView) : base(progressView) { }

        public override void Execute()
        {
            handler.SetProgressTitle(I2.Loc.ScriptLocalization.Get("LOADING"));
            handler.StopIntoAnimationCoroutine();
            asyncOperation = SceneManager.LoadSceneAsync("MaternityScene");
        }

        public override void OnUpdate()
        {
            if (asyncOperation != null)
                handler.SetProgressValue(LoadingGame.Normalize(StartProgress, EndProgress, asyncOperation.progress));
        }

        public override void ExecuteIfConnection() { }
    }

    public class DownloadMaternitySceneOperation : LoadingGame.BaseDownloadAssetBundleOperation
    {
        public DownloadMaternitySceneOperation(MonoBehaviour progressView) : base(progressView) { }

        protected override IEnumerator DoOperation()
        {
            yield return WaitForCachingReady();
            if (LoadingGame.maternityBundle != null)
            {
                handler.GoNextOperation();
            }
            else
            {
                yield return Download();
            }
        }

        public override string GetAssetBundleName()
        {
            return LoadingGame.MaternitySceneABName;
        }

        protected override int GetAssetBundleVersion()
        {
            return LoadingGame.MaternitySceneABVersion;
        }

        protected override void OnSuccess(AssetBundle assetBundle)
        {
            LoadingGame.maternityBundle = assetBundle;
            handler.GoNextOperation();
        }
    }

    private Queue<LoadingGame.BasePartOperation> operations = new Queue<LoadingGame.BasePartOperation>();
    private LoadingGame.BasePartOperation operation;

    protected override void Start()
    {
        SetUpOperations();
    }

    protected override void Update()
    {
        if (operation != null)
            operation.OnUpdate();
    }

    private void SetUpOperations()
    {
        operations.Clear();
        if (DeveloperParametersController.Instance().parameters.UseAssetBundles)
        {
            float lastEndProgress = 0;
            LoadingGame.BasePartOperation downloadMaternitySceneOperation = new DownloadMaternitySceneOperation(this);
            downloadMaternitySceneOperation.StartProgress = 0;
            downloadMaternitySceneOperation.EndProgress = downloadMaternitySceneOperation.ToDownload() ? 60 : lastEndProgress;
            lastEndProgress = downloadMaternitySceneOperation.EndProgress;
            downloadMaternitySceneOperation.EndProgressText = 100;
            operations.Enqueue(downloadMaternitySceneOperation);
            
            LoadingGame.BasePartOperation loadMaternitySceneOperation = new LoadMaternitySceneOperation(this);
            loadMaternitySceneOperation.StartProgress = lastEndProgress;
            loadMaternitySceneOperation.EndProgress = 100;
            operations.Enqueue(loadMaternitySceneOperation);
        }
        else
        {
            LoadingGame.BasePartOperation loadMaternitySceneOperation = new LoadMaternitySceneOperation(this);
            loadMaternitySceneOperation.StartProgress = 0;
            loadMaternitySceneOperation.EndProgress = 100;
            operations.Enqueue(loadMaternitySceneOperation);
        }
        GoNextOperation();
    }

    #region IProgressable

    public void SetProgressValue(float value)
    {
        slider.fillAmount = value;
    }

    public void SetProgressTitle(string title)
    {
        LoadingText.text = title;
    }

    public int GetMinProgress()
    {
        return 0;
    }

    public int GetMaxProgress()
    {
        return 100;
    }

    public void GoNextOperation()
    {
        operation = operations.Count > 0 ? operations.Dequeue() : null;
        if (operation != null)
            operation.Execute();
        else
            Debug.LogError("Completed!");
    }

    public AlertPopupController GetAlertPopup()
    {
        return alertPopup;
    }

    public string GetBundleURLDir(bool checkSmallAB = false)
    {
        string rootUrl = LoadingGame.GetRootURL();
        return rootUrl + Utility.GetPlatformName() + "/";
    }

    public void AplyUserLanguage() {}
    public void AplyMainFontReferences() {}
    public void StopIntoAnimationCoroutine() {}
    public void ShowIntro() {}

    #endregion

    public delegate void ConnectionState(bool isConnected);
    public void HasInternetConnection(ConnectionState connectionStateCallback)
    {
        engine.AddTask(() =>
        {
            connectionStateCallback?.Invoke(AccountManager.HasInternetConnection());
        });
    }

    public LoadingGame.Callback callback;

    public void ProcessIfInternetConnection(LoadingGame.Callback callback)
    {
        this.callback = callback;
        HasInternetConnection((isConnected) =>
        {
            if (isConnected)
            {
                callback?.Invoke();
            }
            else
            {
                StartCoroutine(alertPopup.Open(AlertType.NO_INTERNET_CONNECTION, null, null, () =>
                {
                    alertPopup.button.RemoveAllOnClickListeners();
                    alertPopup.button.onClick.AddListener(delegate
                    {
                        alertPopup.OnFinishedAnimating -= AlertPopup_OnFinishedAnimating;
                        alertPopup.OnFinishedAnimating += AlertPopup_OnFinishedAnimating;
                        alertPopup.NoSmoothExit();
                    });
                }));
            }
        });
    }

    private void AlertPopup_OnFinishedAnimating()
    {
        if (callback != null)
        {
            ProcessIfInternetConnection(callback);
        }
    }

    public Queue<LoadingGame.BasePartOperation> GetOperations()
    {
        return operations;
    }

    public void SetPlayButtonAction(UnityAction action)
    {

    }

    public void SetStartingPanelActive(bool setActive)
    {

    }
}
