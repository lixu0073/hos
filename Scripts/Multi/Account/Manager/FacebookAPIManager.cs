using UnityEngine;
using Facebook.Unity;
using System;
using System.Collections.Generic;

namespace Hospital
{
    public class FacebookAPIManager
    {
        public static void GetUserName(ConnectFacebookUseCase.OnSuccessGetParam onSuccess, OnFailure onFailure, string facebookID = null)
        {
            string query = "/me?fields=first_name";
            if(!string.IsNullOrEmpty(facebookID))
            {
                query = "/"+facebookID+ "?fields=first_name";
            }
            FB.API(query, HttpMethod.GET, (result) =>
            {
                if(result.Error == null)
                {
                    string name = result.ResultDictionary["first_name"].ToString();
                    onSuccess?.Invoke(name);
                }
                else
                {
                    onFailure?.Invoke(new Exception(result.Error));
                }
            });
        }

        public static void GetUserAvatar(GetFacebookUserDataUseCase.OnSuccessSprite onSuccess, OnFailure onFailure, string facebookID = null)
        {
            string query = "/me/picture?type=square&height=100&width=100";
            try
            {
                if (!string.IsNullOrEmpty(facebookID))
                {
                    query = "/" + facebookID + "/picture?type=square&height=100&width=100";
                }
            }
            catch (Exception) { }

            try
            {
                FB.API(query, HttpMethod.GET, (result) =>
                {
                    if (result.Error == null)
                    {
                        if (result.Texture == null)
                        {
                            onFailure?.Invoke(new Exception("no avatar texture"));
                            return;
                        }
                        Sprite avatar = null;
                        if (result != null && result.Texture != null && result.Texture.width >= 100 && result.Texture.height >= 100)
                        {
                            avatar = Sprite.Create(result.Texture, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
                        }
                        onSuccess?.Invoke(avatar);
                    }
                    else
                    {
                        onFailure?.Invoke(new Exception(result.Error));
                    }
                });
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }

        }

        public static void GetFacebookID(ConnectFacebookUseCase.OnSuccessGetParam onSuccess, OnFailure onFailure)
        {
            FB.API("/me?fields=id", HttpMethod.GET, (result) =>
            {
                if (result.Error == null)
                {
                    string facebookID = result.ResultDictionary["id"].ToString();
                    if(facebookID == null)
                    {
                        onFailure?.Invoke(new Exception("parse error"));
                        return;
                    }
                    onSuccess?.Invoke(facebookID);
                }
                else
                {
                    onFailure?.Invoke(new Exception(result.Error));
                }
            });
        }

        
        public static string GetDataValueForKey(Dictionary<string, object> dict, string key)
        {
            object objectForKey;
            return dict.TryGetValue(key, out objectForKey) ? (string)objectForKey : "";
        }

        public static string GetDataPictureForKey(Dictionary<string, object> dict, string key)
        {
            object objectForPictureData;
            if (dict.TryGetValue(key, out objectForPictureData))
            {
                Dictionary<string, object> pictureData = objectForPictureData as Dictionary<string, object>;
                object data;
                string url = "";
                if (pictureData.TryGetValue("data", out data))
                {
                    Dictionary<string, object> data2 = data as Dictionary<string, object>;
                    url = GetDataValueForKey(data2, "url");
                }
                return url;
            }
            else
            {
                return null;
            }
        }

    }
}