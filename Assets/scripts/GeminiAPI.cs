using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Text.RegularExpressions;
using SimpleJSON;
public class GeminiAPI : MonoBehaviour
{
    [Header("Google Gemini Settings")]
    public string apiKey = "AIzaSyBAU-OT51CaK3bVVW5mjLfinrzdxehK-0U";  // <-- 這裡填入你的 Google API Key
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    void Start()
    {
        //StartCoroutine(SendRequest("我們想做一個遊戲，遊戲內容是會有數名玩家和一個AI，AI會根據題目分階段的給生圖模型關鍵字，一開始的關鍵字會讓生圖模型生出來的圖不那麼像題目，然後讓玩家看圖猜題目是什麼，如果玩家都答錯那AI就會讓關鍵字可以生的更像題目，一直持續到玩家可以透過生出的圖片猜到題目為止。例如題目是:「枕頭」，第一階段提示： 「柔軟、舒適、放鬆」→ 圖片可能僅呈現一種溫馨或安靜的氛圍。第二階段提示： 「臥室、夜晚、陪伴」→ 圖片可能開始出現臥室元素或暗示睡眠情境。第三階段提示： 「頭邊的守護者、臥室必備」→ 圖片更可能具體描繪出一個枕頭或其形象化特徵。最終提示（如果仍未猜中）： 「柔軟填充、睡眠伴侶—這就是枕頭！」 → 圖片和提示最終直指答案。你接下來將當作一名出題者，只要給我每個階段的關鍵字就好，格式為【題目:「你的題目」，第一階段:「關鍵字」，第二階段:「關鍵字」，第三階段:「關鍵字」，第四階段:「關鍵字」】，不要有多餘的字，請開始出隨機一個題目。"));

    }
    public IEnumerator SendRequest(string prompt, Action<string> onComplete)
    {
        // 準備要發送的 JSON 請求
        string jsonBody = "{\"contents\": [{\"parts\": [{\"text\": \"" + prompt + "\"}]}]}";

        using (UnityWebRequest request = new UnityWebRequest($"{apiUrl}?key={apiKey}", "POST"))
        {
            // 設定 Header
            request.SetRequestHeader("Content-Type", "application/json");

            // 設定 Body
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 發送請求
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                JSONNode response = JSON.Parse(jsonResponse);
                string reply = response["candidates"][0]["content"]["parts"][0]["text"];
                Debug.Log("Gemini 回應: " + reply);
                //List<string> extractedWords = ExtractTextInsideQuotes(reply);

                onComplete?.Invoke(reply);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }
    public IEnumerator SendPhotoRequest(string prompt, string base64Image, Action<string> onComplete)
    {
        string jsonBody;
            // 包含圖片的請求
        jsonBody = "{\"contents\": [{\"parts\": [" +
                   "{\"inline_data\": {\"data\": \"" + base64Image + "\", \"mimeType\": \"image/png\"}}," + 
                   "{\"text\": \"" + prompt + "\"}" +
                   "]}]}";

        using (UnityWebRequest request = new UnityWebRequest($"{apiUrl}?key={apiKey}", "POST"))
        {
            // 設定 Header
            request.SetRequestHeader("Content-Type", "application/json");

            // 設定 Body
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 發送請求
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                JSONNode response = JSON.Parse(jsonResponse);
                string reply = response["candidates"][0]["content"]["parts"][0]["text"];
                Debug.Log("Gemini 回應: " + reply);
                onComplete?.Invoke(reply);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }
}
