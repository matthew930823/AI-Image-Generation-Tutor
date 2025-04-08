using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
public class GameController : MonoBehaviourPun
{
    public HuggingFaceAPI huggingFaceAPI; // HuggingFaceAPI 的引用
    public InputField descriptionInput;  // 玩家描述的輸入框
    public Image resultImage;            // 用於顯示生成圖片的 UI Image
    public Text roomNumber;

    public GeminiAPI geminiAPI;
    private List<string> keywords = null;

    public List<Button> OptionButton;

    private string answer;
    public Image uiImage;
    public Sprite trueAns;
    public Sprite falseAns;
    void Start()
    {
        roomNumber.text = "Room : " + PhotonNetwork.CurrentRoom.Name;
    }
    public void ApplyDifficultySettings(string diff)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            
            return;
        }
        Debug.Log(diff);
        // 根據難易度調整遊戲參數，例如敵人數量、血量等
        if (diff == "Easy") { /* 設定簡單模式參數 */
            StartCoroutine(GetGeminiKeywords(1));
        }
        else if (diff == "Hard") { /* 設定困難模式參數 */
            StartCoroutine(GetGeminiKeywords(2));
        }
    }

    private void OnImageGenerated(Texture2D generatedTexture)
    {
        if (generatedTexture != null)
        {
            byte[] imageBytes = generatedTexture.EncodeToJPG();
            StartCoroutine(huggingFaceAPI.UpscaleImage(imageBytes, (result) =>
            {
                imageBytes = result;
            }));
            photonView.RPC("SyncImage", RpcTarget.All, imageBytes);
        }
        else
        {
            Debug.LogError("圖片生成失敗！");
        }
    }

    [PunRPC]
    void SyncImage(byte[] imageBytes)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);
        Debug.Log("完成");
        resultImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private IEnumerator GetGeminiKeywords(int mode)
    {

        // 呼叫 API，等它完成
        //yield return StartCoroutine(geminiAPI.SendRequest("我們想做一個遊戲，遊戲內容是會有數名玩家和一個AI，AI會根據題目分階段的給生圖模型關鍵字，一開始的關鍵字會讓生圖模型生出來的圖不那麼像題目，然後讓玩家看圖猜題目是什麼，如果玩家都答錯那AI就會讓關鍵字可以生的更像題目，一直持續到玩家可以透過生出的圖片猜到題目為止。例如題目是:「枕頭」，第一階段提示： 「柔軟、舒適、放鬆」→ 圖片可能僅呈現一種溫馨或安靜的氛圍。第二階段提示： 「臥室、夜晚、陪伴」→ 圖片可能開始出現臥室元素或暗示睡眠情境。第三階段提示： 「頭邊的守護者、臥室必備」→ 圖片更可能具體描繪出一個枕頭或其形象化特徵。最終提示（如果仍未猜中）： 「柔軟填充、睡眠伴侶—這就是枕頭！」 → 圖片和提示最終直指答案。你接下來將當作一名出題者，只要給我每個階段的關鍵字就好，格式為【題目:「你的題目」，第一階段:「關鍵字、關鍵字、關鍵字...」，第二階段:「關鍵字、關鍵字、關鍵字...」，第三階段:「關鍵字、關鍵字、關鍵字...」，第四階段:「關鍵字、關鍵字、關鍵字...」】，不要有多餘的字，請開始出隨機一個題目，關鍵字用英文表示，但標點符號不變。", (result) =>
        //{
        //    keywords = result;
        //}));

        if(mode == 1)
        {
            yield return StartCoroutine(geminiAPI.SendRequest("我們想做一個遊戲，遊戲內容是會有數名玩家和一個AI，然後讓玩家看圖猜題目是什麼，如果玩家都答錯那AI就會讓關鍵字可以生的更像題目，一直持續到玩家可以透過生出的圖片猜到題目為止。回答會是選擇題的樣式，會有四個選項，一個正確，三個錯誤，請你出題目，你要回答的樣式為，題目:「提示字」，選項A:「提示字」，選項B:「提示字」，選項C:「提示字」，選項D:「提示字」，正確答案為:「提示字」，盡量為明確物品，題目用英文，選項用中文，不要有多餘的字，給我一題就好了，標點符號不要變，一定要使用「」框住提示字。", (result) =>
            {
                keywords = result;
            }));
        }
        else
        {
            yield return StartCoroutine(geminiAPI.SendRequest("我們想做一個遊戲，遊戲內容是會有數名玩家和一個AI，然後讓玩家看圖猜題目是什麼，如果玩家都答錯那AI就會讓關鍵字可以生的更像題目，一直持續到玩家可以透過生出的圖片猜到題目為止。回答會是選擇題的樣式，會有四個選項，一個正確，三個錯誤，請你出題目，每次都不一樣的題目，題目為中文成語用英文去表達，你要回應的樣式為，題目:「提示字」，選項A:「提示字」，選項B:「提示字」，選項C:「提示字」，選項D:「提示字」，正確答案為:「提示字」，題目用英文描述，選項用中文，不要有多餘的字，給我一題就好了，標點符號不要變，一定要使用「」框住提示字。", (result) =>
            {
                keywords = result;
            }));
        }
        
        // 等待 API 回應
        Debug.Log("獲取到的關鍵字: " + string.Join(", ", keywords));
        GetOption();
        answer = keywords[1];
        StartCoroutine(huggingFaceAPI.GenerateImageFromText(keywords[0], OnImageGenerated));
        //StartCoroutine(ChangeEvery10Seconds());
    }
    //IEnumerator ChangeEvery10Seconds()
    //{
    //    for (int i = 1; i <= 4; i++)  // 重複 4 次
    //    {
    //        Debug.Log(keywords[i]);
    //        string prompt = keywords[i];
    //        StartCoroutine(huggingFaceAPI.GenerateImageFromText(prompt, OnImageGenerated));
    //        yield return new WaitForSeconds(10f);  // 等待 10 秒
    //    }
    //}
    void GetOption()
    {
        for (int i = 1; i <= 4; i++)  // 重複 4 次
        {
            string prompt = keywords[i];
            Text buttonText = OptionButton[i-1].GetComponentInChildren<Text>();
            buttonText.text = prompt;
        }
    }
    public void CheckAns(Text Buttontext)
    {
        //if(Buttontext.text == answer)
        //{
        //    uiImage.sprite = trueAns;
        //}
        //else
        //{
        //    uiImage.sprite = falseAns;
        //}
    }
}