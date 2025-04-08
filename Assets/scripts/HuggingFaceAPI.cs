using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using SimpleJSON;
using System.Text.RegularExpressions;
public class HuggingFaceAPI : MonoBehaviour
{
    private string apiUrl = "https://api-inference.huggingface.co/models/stabilityai/stable-diffusion-3.5-large-turbo";
    private string apiKey = "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"; // 在這裡替換為您的 Hugging Face API Token
    private string SuperResolutionapiUrl = "https://api.segmind.com/v1/esrgan";
    private string SuperResolutionapiKey = "SG_deb5def02d6cc7a4";
    public GameObject generateText;
    public GameObject errorText;
    /// <summary>
    /// 生成圖片（文字到圖片）
    /// </summary>
    /// <param name="prompt">玩家輸入的文字描述</param>
    /// <param name="callback">回調函數，返回生成的 Texture2D</param>
    public IEnumerator GenerateImageFromText(string prompt, System.Action<Texture2D> callback)
    {
        // 檢查輸入是否合法
        if (string.IsNullOrWhiteSpace(prompt))
        {
            Debug.LogError("輸入描述無效，請提供一個有效的文字描述！");
            yield break;
        }
        //generateText.SetActive(true);
        //errorText.SetActive(false);
        // 正確構建 JSON 數據
        string jsonData = "{\"inputs\":\"" + prompt.Replace("\"", "\\\"") + "\"}";

        // 發送 POST 請求
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            // 設置請求頭
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            // 設置請求體
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 發送請求
            yield return request.SendWebRequest();

            // 處理回應
            if (request.result == UnityWebRequest.Result.Success)
            {
                //generateText.SetActive(false);
                byte[] imageData = request.downloadHandler.data;
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);
                callback?.Invoke(texture);
            }
            else
            {
                //errorText.SetActive(true);
                Debug.LogError($"Hugging Face API 請求失敗: {request.error}");
                Debug.LogError($"詳細錯誤訊息: {request.downloadHandler.text}");
                callback?.Invoke(null);
            }
        }
    }
    public IEnumerator UpscaleImage(byte[] imageData, System.Action<byte[]> onComplete)
    {
        // 建立請求
        UnityWebRequest request = new UnityWebRequest(SuperResolutionapiUrl, "POST");
        request.SetRequestHeader("x-api-key",  SuperResolutionapiKey);
        request.SetRequestHeader("Content-Type", "application/json");

        // 構造 JSON 請求體
        string jsonPayload = "{\"image\":\"" + System.Convert.ToBase64String(imageData) + "\"}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // 發送請求
        yield return request.SendWebRequest();

        // 檢查是否發生錯誤
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            onComplete?.Invoke(null);
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            // 解析返回的圖片數據
            byte[] imageBytes = request.downloadHandler.data;
            //Texture2D texture = new Texture2D(2, 2);
            //texture.LoadImage(imageBytes);
            onComplete?.Invoke(imageBytes);
        }
    }

}
