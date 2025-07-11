using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Hospital;

public class GlobalEventContributorUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] Button button;
    [SerializeField] Image avatarImage;
    [SerializeField] Image frame;
    [SerializeField] Sprite defaultAvatar;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Sprite[] frames;
#pragma warning restore 0649

    public void Setup(IFollower contributor, int score, UnityAction buttonAction, int frameID)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(buttonAction);
        nameText.text = contributor.Name;
        levelText.text = contributor.Level.ToString();
        scoreText.text = score.ToString();
        frame.sprite = frames[Mathf.Clamp(frameID, 0, frames.Length - 1)];
        if (DisplayInFacebookMode(contributor))
        {
            if (contributor.IsFacebookDataDownloaded())
            {
                if (contributor.IsFacebookAvatarDownloaded())
                {
                    nameText.text = contributor.FacebookLogin;
                    TryToDisplayAvatar(contributor.Avatar);
                }
                else
                {
                    DownloadAvatar(contributor);
                }
            }
            else
            {
                contributor.DownloadFacebookData(() =>
                {
                    nameText.text = contributor.FacebookLogin;
                    TryToDisplayAvatar(contributor.Avatar);
                }, (ex) =>
                {

                });
            }
        }
        else
        {
            avatarImage.sprite = defaultAvatar;
        }
    }

    public void Setup(Contributor contributor, UnityAction buttonAction, int frameID)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(buttonAction);
        nameText.text = contributor.Name;
        levelText.text = contributor.Level.ToString();
        scoreText.text = contributor.GetScore().ToString();
        frame.sprite = frames[Mathf.Clamp(frameID, 0, frames.Length - 1)];
        if (DisplayInFacebookMode(contributor))
        {
            if (contributor.IsFacebookDataDownloaded())
            {
                if (contributor.IsFacebookAvatarDownloaded())
                {
                    nameText.text = contributor.FacebookLogin;
                    TryToDisplayAvatar(contributor.Avatar);
                }
                else
                {
                    DownloadAvatar(contributor);
                }
            }
            else
            {
                contributor.DownloadFacebookData(() =>
                {
                    nameText.text = contributor.FacebookLogin;
                    TryToDisplayAvatar(contributor.Avatar);
                }, (ex) =>
                {

                });
            }
        }
        else
        {
            avatarImage.sprite = defaultAvatar;
        }
    }

    private void DownloadAvatar(Contributor person)
    {
        person.DownloadAvatar(() =>
        {
            TryToDisplayAvatar(person.Avatar);
        }, (ex) =>
        {
            if (ex == null)
            {
                Debug.Log("No Avatar URL for: " + person.Name);
            }
        });
    }

    private void DownloadAvatar(IFollower person)
    {
        person.DownloadAvatar(() =>
        {
            TryToDisplayAvatar(person.Avatar);
        }, (ex) =>
        {
            if (ex == null)
            {
                Debug.Log("No Avatar URL for: " + person.Name);
            }
        });
    }

    private bool DisplayInFacebookMode(Contributor contributor, bool FromFb = false)
    {
        return (!string.IsNullOrEmpty(contributor.FacebookID) || FromFb) && AccountManager.Instance.IsFacebookConnected;
    }

    private bool DisplayInFacebookMode(IFollower contributor, bool FromFb = false)
    {
        return (!string.IsNullOrEmpty(contributor.FacebookID) || FromFb) && AccountManager.Instance.IsFacebookConnected;
    }

    private void TryToDisplayAvatar(Sprite sprite, bool setNull = false)
    {
        if ((sprite != null || setNull))
        {
            avatarImage.sprite = sprite == null ? defaultAvatar : sprite;
        }
    }
}
