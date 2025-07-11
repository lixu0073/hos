using UnityEngine;
using System.Collections;
using System;
using System.Text;
using UnityEngine.Purchasing;

/// <summary>
/// 应用内购买收据处理辅助类，负责处理iOS和Android平台的IAP收据验证。
/// 提供收据解析、Base64编码解码、签名提取等功能，用于IAP安全验证。
/// </summary>
public static class IAPReceiptHelper {


    public static string GetAppleTransactionId(string unityReceipt)
    {
        JsonReceipt jr = JsonUtility.FromJson<JsonReceipt>(unityReceipt);
        return jr.TransactionID;
    }

    public static string GetBase64EncodedReceipt(string unityReceipt)
    {
        JsonReceipt jr = JsonUtility.FromJson<JsonReceipt>(unityReceipt);
        return jr.Payload;
    }

    public static string GetBase64DecodedReceipt(string unityReceipt)
    {
        JsonReceipt jr = JsonUtility.FromJson<JsonReceipt>(unityReceipt);
        byte[] decodedBytes = Convert.FromBase64String(jr.Payload);
        string decodedText = System.Text.Encoding.UTF8.GetString(decodedBytes);
        return decodedText;
    }

    public static string GetSignatureFromReceipt(string unityReceipt)
    {
        if (string.IsNullOrEmpty(unityReceipt))
            return "null";

        JsonReceipt jr = JsonUtility.FromJson<JsonReceipt>(unityReceipt);
        JsonAndroidPayload jap = JsonUtility.FromJson<JsonAndroidPayload>(jr.Payload);
        return jap.signature;
    }
    
    public static string GetPurchaseDataFromReceipt(string unityReceipt)
    {
        JsonReceipt jr = JsonUtility.FromJson<JsonReceipt>(unityReceipt);
        JsonAndroidPayload jap = JsonUtility.FromJson<JsonAndroidPayload>(jr.Payload);
        return jap.json;
    }
}



[Serializable]
public class JsonReceipt
{
    public string Store;
    public string TransactionID;
    public string Payload;
}

[Serializable]
public class JsonAndroidPayload
{
    public string json;
    public string signature;
}