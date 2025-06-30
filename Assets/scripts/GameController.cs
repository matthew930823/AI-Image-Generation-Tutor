using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System.Linq;

public class GameController : MonoBehaviourPun
{
    //public HuggingFaceAPI huggingFaceAPI; // HuggingFaceAPI 的引用
    //public InputField descriptionInput;  // 玩家描述的輸入框
    public Image resultImage;            // 用於顯示生成圖片的 UI Image
    public Text roomNumber;

    public GeminiAPI geminiAPI;
    private List<string> keywords = null;

    public List<Button> OptionButton;

    public string answer;
    //public Image uiImage;
    public Sprite trueAns;
    public Sprite falseAns;

    public Animator characteranimator;

    public StableDiffusionRegionPrompt stablediffusionregionprompt;

    public VoiceAudioPlayer voiceAudioPlayer;

    private String NowDifficulty;

    public MultiChoiceQuestion multiChoiceQuestion;
    private int NowQuestion = 1;

    public TTSClone tTSClone;
    void Start()
    {
        //roomNumber.text = "Room : " + PhotonNetwork.CurrentRoom.Name;
    }
    public void ApplyDifficultySettings(string diff)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            
            return;
        }
        Debug.Log(diff);
        NowDifficulty = diff;
        if (diff == "Easy") { /* 設定簡單模式參數 */
            StartCoroutine(GetGeminiKeywords(1));
        }
        else if (diff == "Hard") { /* 設定困難模式參數 */
            StartCoroutine(GetGeminiKeywords(2));
        }
        else
        {
            StartCoroutine(GetGeminiKeywords(3));
        }
    }

    private void OnImageGenerated(Texture2D generatedTexture)
    {
        if (generatedTexture != null)
        {
            byte[] imageBytes = generatedTexture.EncodeToJPG();
            //StartCoroutine(huggingFaceAPI.UpscaleImage(imageBytes, (result) =>
            //{
            //    imageBytes = result;
            //}));
            photonView.RPC("SyncImage", RpcTarget.All, imageBytes);
            // 開始延遲傳送圖片
            //StartCoroutine(DelayedSyncImage(imageBytes, 30f)); // 300 秒延遲
        }
        else
        {
            Debug.LogError("圖片生成失敗！");
        }
    }
    //private IEnumerator DelayedSyncImage(byte[] imageBytes, float delaySeconds)
    //{
    //    yield return new WaitForSecondsRealtime(delaySeconds);
    //    ApplyDifficultySettings(NowDifficulty);
    //    photonView.RPC("SyncImage", RpcTarget.All, imageBytes);
    //}
    [PunRPC]
    void SyncImage(byte[] imageBytes)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);
        Debug.Log("圖片加載完成");
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
            //yield return StartCoroutine(geminiAPI.SendRequest("我們想做一個遊戲，遊戲內容是會有數名玩家和一個AI，然後讓玩家看圖猜題目是什麼，如果玩家都答錯那AI就會讓關鍵字可以生的更像題目，一直持續到玩家可以透過生出的圖片猜到題目為止。回答會是選擇題的樣式，會有四個選項，一個正確，三個錯誤，請你出題目，你要回答的樣式為，題目:「提示字」，選項A:「答案提示字」，選項B:「提示字」，選項C:「提示字」，選項D:「提示字」，正確答案為:「提示字」，盡量為明確物品，題目用英文，選項用中文，不要有多餘的字，給我一題就好了，標點符號不要變，一定要使用「」框住提示字。", (result) =>
            //{
            //    keywords = ExtractTextInsideQuotes(result);
            //}));
            //Debug.Log("獲取到的關鍵字: " + string.Join(", ", keywords));
            ////GetOption();
            //answer = keywords[1];
            //StartCoroutine(huggingFaceAPI.GenerateImageFromText(keywords[0], OnImageGenerated));

            //answer = multiChoiceQuestion.ChangeQuestion(NowQuestion++);//題庫模式
            StartCoroutine(multiChoiceQuestion.stableDiffusionRegionPrompt.StartAutoImageUpdate());
        }
        else if(mode == 2)
        {
            yield return StartCoroutine(geminiAPI.SendRequest("我們想做一個遊戲，遊戲內容是會有數名玩家和一個AI，然後讓玩家看圖猜題目是什麼，如果玩家都答錯那AI就會讓關鍵字可以生的更像題目，一直持續到玩家可以透過生出的圖片猜到題目為止。回答會是選擇題的樣式，會有四個選項，一個正確，三個錯誤，請你出題目，每次都不一樣的題目，題目為中文成語用英文去表達，你要回應的樣式為，題目:「提示字」，選項A:「答案提示字」，選項B:「提示字」，選項C:「提示字」，選項D:「提示字」，正確答案為:「提示字」，題目用英文描述，選項用中文，不要有多餘的字，給我一題就好了，標點符號不要變，一定要使用「」框住提示字。", (result) =>
            {
                keywords = ExtractTextInsideQuotes(result);
            }));
            //Debug.Log("獲取到的關鍵字: " + string.Join(", ", keywords));
            ////GetOption();
            //answer = keywords[1];
            //StartCoroutine(huggingFaceAPI.GenerateImageFromText(keywords[0], OnImageGenerated));
        }
        else
        {
            StartCoroutine(ChainCoroutines());
        }

        // 等待 API 回應
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
    public string SpriteToBase64String(Sprite sprite)
    {
        // 建立新的 Texture2D（僅限區域）
        Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.RGBA32, false);

        // 從 sprite 原始 texture 中擷取像素到新 Texture2D
        Color[] pixels = sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height
        );
        texture.SetPixels(pixels);
        texture.Apply();

        // 將 Texture2D 轉為 PNG byte[]
        byte[] pngBytes = texture.EncodeToPNG();

        // 將 byte[] 轉為 base64 字串
        return Convert.ToBase64String(pngBytes);
    }

    bool ResultCool = false;
    IEnumerator CountdownCoroutine(int seconds)
    {
        int remaining = seconds;
        ResultCool = true;
        while (remaining > 0)
        {
            yield return new WaitForSeconds(1);
            remaining--;
        }
        ResultCool = false;
    }
    public void CheckAns(Text Buttontext)
    {
        if (multiChoiceQuestion.IsResultScreen && !ResultCool)
        {
            voiceAudioPlayer.AudioPlay(3);
            StartCoroutine(CountdownCoroutine(30));
            string[] twoImages; twoImages = new string[] { SpriteToBase64String(multiChoiceQuestion.AfterImage.sprite) };
            string resultText = "";
            if (Buttontext.text == answer)
            {
                if (Buttontext.text == "depth" || Buttontext.text == "openpose" || Buttontext.text == "canny" || Buttontext.text == "shuffle")
                {
                    twoImages = new string[] { SpriteToBase64String(multiChoiceQuestion.BeforeImage.sprite), SpriteToBase64String(multiChoiceQuestion.AfterImage.sprite) };
                    StartCoroutine(SendPhotoRequestCoroutine(stablediffusionregionprompt.ResultLLM[Buttontext.text][0] + "，Depth特色:Depth會將人物的形狀控制住，但人物還是會跟參考圖有一些細節上的區別，同時背景也會受到參考圖影響，Openpose特色: 只將人物的姿勢控制住，所以人物對比參考圖可以有更多自己的特色，同時背景也能由生圖模型自由發揮，Canny特色: 會被參考圖的所有邊緣細節控制住，所以在有線條的地方幾乎都會和參考圖一樣，和depth的主要區別在於depth的人物在細節上來是跟參考圖有差異，但canny則會和參考圖相同(例如服裝上的細節)，Shuffle特色: 只會被參考圖的色彩風格控制住，所以人物和背景都會自由發揮，只有色調會和參考圖有關" + "，直接回答我解釋就可以了", twoImages));
                }
                else if (Buttontext.text == "Hanfu" || Buttontext.text == "Cutecat" || Buttontext.text == "Blindbox" || Buttontext.text == "Eye" || Buttontext.text == "MoXin" || Buttontext.text == "Ghibli" || Buttontext.text == "Foodphoto" || Buttontext.text == "Lineart")
                {
                    twoImages = new string[] { SpriteToBase64String(multiChoiceQuestion.BeforeImage.sprite), SpriteToBase64String(multiChoiceQuestion.AfterImage.sprite) };
                    StartCoroutine(SendPhotoRequestCoroutine(stablediffusionregionprompt.ResultLLM[Buttontext.text][0] + "，直接回答我解釋就可以了", twoImages));
                }
                else if (Buttontext.text == "128")
                {
                    multiChoiceQuestion.Explain.text = "128x128解析度生成的圖片會模糊，所以如果圖片糊糊的，就可以判斷圖片的解析度應該是128x128";
                }
                else if (Buttontext.text == "384")
                {
                    multiChoiceQuestion.Explain.text = "384x384解析度生成的圖片會缺少圖片的內容物，所以如果少了些什麼但有不會到很模糊，就可以判斷是384x384解析度的圖片";
                }
                else if (Buttontext.text == "512")
                {
                    multiChoiceQuestion.Explain.text = "512x512解析度生成的圖片通常不會有問題，但有時會缺少一些圖片的細節，所以如果覺得圖片差不多只有一些細小的差異，就可以判斷圖片的解析度為512x512";
                }
                else if (Buttontext.text == "1024")
                {
                    multiChoiceQuestion.Explain.text = "1024x1024解析度生成的圖片有時會多出許多不需要的內容，所以如果發現圖片有一些奇怪的內容，就可以判斷圖片的解析度應該是1024";
                }
                else if (stablediffusionregionprompt.ResultLLM.ContainsKey(Buttontext.text))
                {
                    StartCoroutine(SendPhotoRequestCoroutine(stablediffusionregionprompt.ResultLLM[Buttontext.text][0]+"，直接回答我解釋就可以了", twoImages));
                }
                else
                {
                    StartCoroutine(SendPhotoRequestCoroutine(stablediffusionregionprompt.ResultLLM["Prompt"][0].Replace("(正確答案提示詞)", Buttontext.text) + "，直接回答我解釋就可以了", twoImages));
                }
            }
            else
            {
                if (Buttontext.text == "depth" || Buttontext.text == "openpose" || Buttontext.text == "canny" || Buttontext.text == "shuffle")
                {
                    twoImages = new string[] { SpriteToBase64String(multiChoiceQuestion.BeforeImage.sprite), SpriteToBase64String(multiChoiceQuestion.AfterImage.sprite) };
                    StartCoroutine(SendPhotoRequestCoroutine(stablediffusionregionprompt.ResultLLM[Buttontext.text][1].Replace("(填入正確答案)", Buttontext.text) + "，Depth特色:Depth會將人物的形狀控制住，但人物還是會跟參考圖有一些細節上的區別，同時背景也會受到參考圖影響，Openpose特色: 只將人物的姿勢控制住，所以人物對比參考圖可以有更多自己的特色，同時背景也能由生圖模型自由發揮，Canny特色: 會被參考圖的所有邊緣細節控制住，所以在有線條的地方幾乎都會和參考圖一樣，和depth的主要區別在於depth的人物在細節上來是跟參考圖有差異，但canny則會和參考圖相同(例如服裝上的細節)，Shuffle特色: 只會被參考圖的色彩風格控制住，所以人物和背景都會自由發揮，只有色調會和參考圖有關"  + "，直接回答我解釋就可以了", twoImages));
                }
                else if (Buttontext.text == "128")
                {
                    multiChoiceQuestion.Explain.text = "128x128解析度生成的圖片會模糊，所以如果圖片糊糊的，就可以判斷圖片的解析度應該是128x128";
                }
                else if (Buttontext.text == "384")
                {
                    multiChoiceQuestion.Explain.text = "384x384解析度生成的圖片會缺少圖片的內容物，所以如果少了些什麼但有不會到很模糊，就可以判斷是384x384解析度的圖片";
                }
                else if (Buttontext.text == "512")
                {
                    multiChoiceQuestion.Explain.text = "512x512解析度生成的圖片通常不會有問題，但有時會缺少一些圖片的細節，所以如果覺得圖片差不多只有一些細小的差異，就可以判斷圖片的解析度為512x512";
                }
                else if (Buttontext.text == "1024")
                {
                    multiChoiceQuestion.Explain.text = "1024x1024解析度生成的圖片有時會多出許多不需要的內容，所以如果發現圖片有一些奇怪的內容，就可以判斷圖片的解析度應該是1024";
                }
                else if (stablediffusionregionprompt.ResultLLM.ContainsKey(Buttontext.text))
                {
                    StartCoroutine(SendPhotoRequestCoroutine(stablediffusionregionprompt.ResultLLM[Buttontext.text][1] + "，直接回答我解釋就可以了", twoImages));
                }
                else
                {
                    resultText = stablediffusionregionprompt.ResultLLM["Prompt"][1];
                    resultText = resultText.Replace("(正確答案提示詞)", answer);
                    resultText = resultText.Replace("(選擇詳解提示詞)", Buttontext.text);
                    StartCoroutine(SendPhotoRequestCoroutine(resultText + "，直接回答我解釋就可以了", twoImages));
                }
            }
        }

        else if (multiChoiceQuestion.IsResultScreen && ResultCool)
        {
            voiceAudioPlayer.AudioPlay(4);//詳解生成冷卻時點及生成詳解
        }
        else if(!multiChoiceQuestion.IsResultScreen)
        {
            if (Buttontext.text == answer)
            {
                characteranimator.SetTrigger("correct");
                voiceAudioPlayer.AudioPlay(1);
                //answer = multiChoiceQuestion.ChangeQuestion(NowQuestion);
                //if (NowQuestion < 17) NowQuestion++;
                //stablediffusionregionprompt.skipWait = true;
                StartCoroutine(multiChoiceQuestion.ChangeButtonColor(4f));
            }
            else
            {
                //characteranimator.Play("Wrong", 0, 0f);
                characteranimator.SetTrigger("wrong");
                voiceAudioPlayer.AudioPlay(2);

                StartCoroutine(multiChoiceQuestion.ChangeButtonColor(6f));
            }

            multiChoiceQuestion.IsResultScreen = true;
        
        }
    }
    private IEnumerator SendPhotoRequestCoroutine(string prompt, string[] images)
    {
        yield return StartCoroutine(geminiAPI.SendMorePhotoRequest(prompt, images, (result) =>
        {
            result = result.Replace("[", "");
            result = result.Replace("]", "");
            multiChoiceQuestion.Explain.text = result;
        }));
    }
    private string Uncheckedprompt;
    private string checkedprompt;
    IEnumerator ReadSettingFileAndSend()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "LLM設定.txt");

    #if UNITY_ANDROID && !UNITY_EDITOR
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("讀檔失敗: " + www.error);
            yield break;
        }
        string fileContent = www.downloadHandler.text;
    #else
            string fileContent = System.IO.File.ReadAllText(path);
    #endif

        // 呼叫 Gemini API 並傳入檔案內容
        yield return StartCoroutine(geminiAPI.SendRequest(fileContent, (result) =>
        {
            Uncheckedprompt = result;
            //keywords = ExtractContentsSeparatedByDash(result);

            ////foreach (var content in keywords)
            ////{
            ////    Debug.Log(content);
            ////}
            ////Debug.Log(keywords.Count);
            //for (int i=0;i< keywords.Count / 7; i++)
            //{
            //    stablediffusionregionprompt.InputRegion(keywords[i * 7], float.Parse(keywords[i * 7 + 1]), float.Parse(keywords[i * 7 + 2]), float.Parse(keywords[i * 7 + 3]), float.Parse(keywords[i * 7 + 4]), keywords[i * 7 + 5], keywords[i * 7 + 6]);
            //    //Debug.Log(i);
            //}
        }));
    }
    IEnumerator ReadCheckFileAndSend()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "檢查訊息.txt");

#if UNITY_ANDROID && !UNITY_EDITOR
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("讀檔失敗: " + www.error);
            yield break;
        }
        string fileContent = www.downloadHandler.text;
#else
        string fileContent = System.IO.File.ReadAllText(path);
#endif
        checkedprompt = Uncheckedprompt + fileContent;

        // 呼叫 Gemini API 並傳入檔案內容
        yield return StartCoroutine(geminiAPI.SendRequest(checkedprompt, (result) =>
        {
            Debug.Log(result);
            answer = result;
            keywords = ExtractContentsSeparatedByDash(result);

            ////foreach (var content in keywords)
            ////{
            ////    Debug.Log(content);
            ////}
            ////Debug.Log(keywords.Count);
            for (int i = 0; i < keywords.Count / 7; i++)
            {
                stablediffusionregionprompt.InputRegion(keywords[i * 7], float.Parse(keywords[i * 7 + 1]), float.Parse(keywords[i * 7 + 2]), float.Parse(keywords[i * 7 + 3]), float.Parse(keywords[i * 7 + 4]), keywords[i * 7 + 5], keywords[i * 7 + 6]);
                //Debug.Log(i);
            }
        }));
    }
    List<string> ExtractContentsSeparatedByDash(string text)
    {
        List<string> allItems = new List<string>();
        MatchCollection matchCollection = Regex.Matches(text, @"\{(.*?)\}");

        foreach (Match match in matchCollection)
        {
            string insideBraces = match.Groups[1].Value;
            // 用 '-' 分割
            string[] parts = insideBraces.Split('=');

            foreach (var part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    allItems.Add(part.Trim());
                }
            }
        }

        return allItems;
    }
    static List<string> ExtractTextInsideQuotes(string input)
    {
        List<string> results = new List<string>();
        MatchCollection matches = Regex.Matches(input, "[「\"](.*?)[」\"]");

        foreach (Match match in matches)
        {
            results.Add(match.Groups[1].Value);
        }

        return results;
    }
    private IEnumerator ChainCoroutines()
    {
        yield return StartCoroutine(ReadSettingFileAndSend());
        yield return StartCoroutine(ReadCheckFileAndSend());
        yield return StartCoroutine(stablediffusionregionprompt.GenerateImageWithRegions(OnImageGenerated));
    }

    public void SubmitButton(Text MyAnswer)
    {
        StartCoroutine(CheckAnswer(MyAnswer.text));
    }
    private IEnumerator CheckAnswer(String MyAnswer)
    {
        String CheckAnswerPrompt = $"你是負責比對圖像敘述正確性的助手。請參考以下題目所包含的關鍵詞描述，每個項目各代表一個物件或特徵，例如:{{Background=0=0=1=1=lush green rainforest, dense foliage, dappled sunlight, mist hanging low=0}}應該要記錄[1.lush green rainforest 2.dense foliage 3.dappled sunlight 4.mist hanging low]，以下為題目:{answer}，使用者回答如下:{MyAnswer}，請檢查使用者的回答中是否有描述到上述哪些項目。即使使用的是同義詞，只要語意相同也算命中。每命中一項就得一分。請回覆一個整數，代表該回答命中了幾個項目（0~N 分），然後簡要列出命中的項目。只需要簡單清楚的說明即可。";
        
        yield return StartCoroutine(geminiAPI.SendRequest(CheckAnswerPrompt, (result) => {
            Match match = Regex.Match(result, @"\d+");
            int score = int.Parse(match.Value);
            Debug.Log($"你拿到了{score}分。");
            if (score > 0)
            {
                characteranimator.SetTrigger("correct");
                voiceAudioPlayer.AudioPlay(1);
            }
            else
            {
                characteranimator.SetTrigger("wrong");
                voiceAudioPlayer.AudioPlay(2);
            }
        }
        ));

    }
}