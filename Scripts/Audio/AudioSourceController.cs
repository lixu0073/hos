using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 音频源控制器，负责管理音频播放、支持多线程安全的音频剪辑播放控制
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioSourceController : MonoBehaviour
{
    private AudioSource audioSource;
#pragma warning disable 0649
    [SerializeField] private AudioClip[] clips;
#pragma warning restore 0649
    public int currentAudioClipIndex = 0;

    #region EXAMPLE ON MAIN THREAD DISPATCHER
    public void CallExample()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(XXXOnTheMainThread());
    }

    public IEnumerator XXXOnTheMainThread()
    {
        //call to main thread
        yield return null;
    }
    #endregion

    private void Awake()
    {
        try
        {
            audioSource = this.GetComponent<AudioSource>();
        }
        catch (Exception e) 
        {
            Debug.LogException(e, this);
        }
    }

    public void PlayClip(int clipIndex)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(PlayClipOnTheMainThread(clipIndex));
    }

    public IEnumerator PlayClipOnTheMainThread(int clipIndex)
    {
        currentAudioClipIndex = clipIndex;
        PlayCurrentClip();
        yield return null;
    }

    public void PlayCurrentClip()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(PlayCurrentClipOnTheMainThread());  
    }

    public IEnumerator PlayCurrentClipOnTheMainThread()
    {
        audioSource.PlayOneShot(clips[currentAudioClipIndex]);
        yield return null;
    }
}
