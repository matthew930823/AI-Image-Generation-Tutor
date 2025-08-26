using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIAgentMode : MonoBehaviour
{
    string[][] AgentFlow = new string[][]
    {
        new string[] { "選主題", "你有沒有想要什麼特定的圖片主題呢"},
        new string[] { "選漢服", "你希望漢服是什麼朝代的呢"},
        new string[] { "選食物", "你希望是什麼食物的照片呢"},
        new string[] { "選風格", "你希望圖片的風格是怎麼樣的呢" },
        new string[] { "選主體", "你希望主體是什麼呢" },
        new string[] { "選年齡", "你希望人物的年齡如何呢" },
        new string[] { "選姿勢", "你希望人物的姿勢是什麼呢" },
        new string[] { "選參考圖", "你希望人物的跳躍/跑步/坐著/站立的姿勢長怎麼樣呢" },
        new string[] { "選表情", "你希望人物的表情是怎麼樣的呢" },
        new string[] { "選色調", "你希望圖片的色調是怎麼樣的呢" },
        new string[] { "選背景", "你希望圖片的背景是怎麼樣的呢" },
        new string[] { "選解析度", "你希望生成的圖片解析度如何呢，我們生成的圖片固定為正方形喔" },
        new string[] { "額外細節", "你還有什麼細節想補充的嗎，請一次填寫一個細節喔" },
    };
    public string[] Select = new string[13];
    public TMP_Text MainText;
    public TMP_Text InfoText;
    private bool Next = false;
    private bool FinishAgent = false;
    private int Step = -1;
    public MultiChoiceQuestion multi;

    public GameObject SkipButton;

    public Image Result_Image;

    bool IsAgent = false;
    public Text content;
    public Text suggestion;
    int AgentCount = 5;

    public GameObject BigScene;
    public GameObject GameScene;
    public GameObject AgentScene;
    public GameObject ResultScene;
    public GameObject AgentCheckScene;
    public GameObject Option;
    public GameObject AllHint;


    public GameObject InputButton;
    public GameObject OtherDetailButton;

    public TMP_Text[] Result;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartAgentMode());
    }
    public void AgentNext()
    {
        Next = true;
        Step++;
    }
    public void OnAgentButtonClick(Text buttontext)
    {
        Select[Step] = buttontext.text;
        AgentNext();
    }
    public void OnAgentInputButton(InputField inputField)
    {
        if (inputField.text != "")
        {
            Select[Step] = inputField.text;
            inputField.text = "";
            if (Step == 6)
            {
                Next = true;
                Step = 8;
            }
            else if(Step == 4)
            {
                Next = true;
                Step = 9;
            }
            else
                AgentNext();
        }
        
    }

    public void GoAgentMode()
    {
        IsAgent = true;

        AgentScene.SetActive(true);
        AgentNext();
    }
    public void OnAddOtherButton(InputField inputField)
    {
        if (inputField.text != "")
        {
            Select[Step] += ", " + inputField.text;
            inputField.text = "";
        }
    }
    Dictionary<string, string> LoraMap = new Dictionary<string, string>()
        {
            { "女性漢服", "Hanfu" },
            { "黑白漫畫", "Manga" },
            { "可愛貓咪", "Cutecat" },
            { "中國水墨畫", "Inkpainting" },
            { "盒玩人偶", "Doll" },
            { "吉卜力", "Ghibli" },
            { "漂亮眼睛", "Eye" },
            { "食物照片", "Foodphoto" },
            { "", "None" }
        };
    Dictionary<string, string> CheckpointMap = new Dictionary<string, string>()
        {
            { "anime_cute.safetensors", "CuteYukiMix" },
            { "anime-real_hybrid.safetensors", "ReV Animated" },
            { "anime_soft.safetensors", "Cetus-Mix" },
            { "realistic_anything.safetensors", "DreamShaper" },
            { "anime_bold.safetensors", "Counterfeit" },
            { "","None"}
        };
    IEnumerator ConversionInfo(bool AgentMode)//Lora 漢服 食物 checkpoint 主體 年齡 姿勢 參考圖 表情 色調 背景 解析度 細節
    {
        //Key_Prompt + Main_Prompt + Other_Prompt + LoRa_Prompt + Default_Prompt
        string Key_Prompt = "";
        string Main_Prompt = "";
        string Other_Prompt = "";
        string LoRa_Prompt = "";
        string Default_Prompt = ", BREAK, (masterpiece:1.2),  best quality, highres, highly detailed, perfect lighting , <lora:add_detail:0.5> ";

        string Neg_Prompt = "easynegative, (badhandv4:1.2), NSFW, (worst quality:2), (low quality:2), (normal quality:2), lowres, normal quality, ((monochrome)), ((grayscale)), skin spots, acnes, skin blemishes, age spot, (ugly:1.331), (duplicate:1.331), watermark jpeg artifacts signature watermark username blurry, Stable_Yogis_SD1.5_Negatives-neg";

        string Checkpoint = "";

        string Add_Detail = "";

        int Resolution = int.TryParse(Select[11], out var r) ? r : 512;

        Resolution = (Resolution > 1280) ? 1280 : Resolution;
        Sprite[] Hint = Resources.LoadAll<Sprite>("Agent模式圖片");

        string Controlnet_modelString = (Select[7]!="")?"control_v11p_sd15_openpose [cab727d4]":"";
        string Controlnet_moduleString = "none";

        string Controlnet_Image = Convert.ToBase64String(System.Array.Find(
                   Hint,
                   sprite => sprite.name.Contains(Select[7])
               ).texture.EncodeToPNG());
        
        Dictionary<string, string> dynastyMap = new Dictionary<string, string>()
        {
            { "明朝", "ming style" },
            { "宋朝", "song style" },
            { "唐朝", "tang style" },
            { "晉朝", "jin style" },
            { "漢朝", "han style" }
        };
        
 
        

        if (AgentMode)
        {
            string key = LoraMap.FirstOrDefault(kvp => kvp.Value.Contains(editResultObjects[0].GetComponent<TMP_Text>().text)).Key;
            Debug.Log(key);
            Select[0] = key;
            Select[4] = editResultObjects[1].GetComponent<TMP_Text>().text;
            Select[3] = "";//不要放後面影響到model
            Checkpoint = CheckpointMap.FirstOrDefault(kvp => kvp.Value.Contains(editResultObjects[2].GetComponent<TMP_Text>().text)).Key;
            Select[9] = editResultObjects[3].GetComponent<TMP_Text>().text;
            Select[10] = editResultObjects[4].GetComponent<TMP_Text>().text;
            Select[6] = editResultObjects[5].GetComponent<TMP_Text>().text;
        }
        switch (Select[0]) 
        {
            case "女性漢服":
                Controlnet_modelString = "control_v11f1p_sd15_depth [cfd03158]";
                Controlnet_moduleString = "none";
                LoRa_Prompt = ",<lora:hanfu40-beta-3:0.6>";
                Other_Prompt += ",hanfu," + dynastyMap[Select[1]];
                break;
            case "黑白漫畫":
                LoRa_Prompt = ",lineart, ((monochrome)),<lora:animeoutlineV4_16:1.3>";
                Neg_Prompt = "easynegative, (badhandv4:1.2), NSFW, watermark jpeg artifacts signature watermark username blurry";
                Select[3] = "插畫動畫";
                break;
            case "可愛貓咪":
                Main_Prompt = "1 cute cat"; 
                Controlnet_modelString = "";
                Select[3] = "擬真動畫";
                LoRa_Prompt = ",<lora:cat_20230627113759:0.7>";
                break;
            case "中國水墨畫":
                Main_Prompt = "potrait of ";
                Other_Prompt += ",shuimobysim, shukezouma";
                Select[3] = "可愛動畫";
                LoRa_Prompt = ",<lora:3Guofeng3_v34:0.85> <lora:shuV2:0.9>";
                break;
            case "盒玩人偶":
                Select[4] = "1 woman";
                LoRa_Prompt = ",<lora:blindbox_v1_mix:1>";
                Other_Prompt += ",full body, chibi";
                Controlnet_modelString = "";
                Select[3] = "擬真動畫";
                break;
            case "吉卜力":
                LoRa_Prompt = ",<lora:ghibli_style_offset:1>";
                Select[3] = "柔和動畫";
                Other_Prompt += ",ghibli style";
                break;
            case "漂亮眼睛":
                LoRa_Prompt = ",<lora:Loraeyes_V1:0.8>";
                Main_Prompt = "1 eye";
                Other_Prompt += ",loraeyes";
                Controlnet_modelString = "";
                Select[3] = "可愛動畫";
                Resolution = 512;
                break;
            case "食物照片":
                Main_Prompt = Select[2];
                Select[3] = "現實風格";
                LoRa_Prompt = ",<lora:foodphoto:0.6>";
                Other_Prompt += ",foodphoto";
                break;
            default:
                LoRa_Prompt = "";
                break;
        }
        switch (Select[3])
        {
            case "可愛動畫":
                Checkpoint = "anime_cute.safetensors";
                break;
            case "擬真動畫":
                Checkpoint = "anime-real_hybrid.safetensors";
                break;
            case "柔和動畫":
                Checkpoint = "anime_soft.safetensors";
                break;
            case "現實風格":
                Checkpoint = "realistic_anything.safetensors";
                break;
            case "插畫動畫":
                Checkpoint = "anime_bold.safetensors";
                break;
            default:
                break;
        }
        switch (Select[4])
        {
            case "男生":
                Add_Detail = "detail face";
                if (Select[5] == "小孩")
                {
                    Main_Prompt += "1 little boy";
                    Add_Detail = "cute face,little boy";
                }
                else if (Select[5] == "老人")
                {
                    Main_Prompt += "1 elderly man";
                    Default_Prompt += ",(AS-Elderly:1.5)";
                    Add_Detail += ",wrinkle";
                }
                else
                {
                    Main_Prompt += "1 man";
                }
                break;
            case "女生":
                Add_Detail = "detail face";
                if (Select[5] == "小孩")
                {
                    Main_Prompt += "1 little girl";
                    Add_Detail = "cute face,little girl";
                }
                else if (Select[5] == "老人")
                {
                    Main_Prompt += "1 elderly woman";
                    Default_Prompt += ",(AS-Elderly:1.5)";
                    Add_Detail += ",wrinkle";
                }
                else
                {
                    Main_Prompt += "1 woman";
                }
                break;
            default:
                if (Select[4] != "")
                    Main_Prompt += Select[4];
                break;
        }
        switch (Select[6])
        {
            case "跳躍":
                Other_Prompt += ",jumping";
                Result[5].text = "jumping";
                break;
            case "跑步":
                Other_Prompt += ",running";
                Result[5].text = "running";
                break;
            case "坐著":
                Other_Prompt += ",sitting";
                Result[5].text = "sitting";
                break;
            case "站立":
                Other_Prompt += ",standing";
                Result[5].text = "standing";
                break;
            default:
                if (Select[6] != "")
                {
                    Other_Prompt += "," + Select[6];
                    Result[5].text = Select[6];
                }
                else
                    Result[5].text = "None";
                break;
        }

        if (Select[8] != "")
        {
            Other_Prompt += "," + Select[8];
            Add_Detail += "," + Select[8];
        }
        if (Select[9] != "")
            Key_Prompt += "(" + Select[9] + ":2)";
        if (Select[10] != "")
            Key_Prompt += ",(" + Select[10] + ":2)";

        
        string Prompt = Key_Prompt + ",(" + Main_Prompt + ((Select[5] != "") ? ":1.7)" : ":1.3)") + Other_Prompt+ Select[12] + LoRa_Prompt + Default_Prompt;
        Debug.Log(  "Prompt:"+ Prompt + "\n"+
                    "Neg_Prompt:" + Neg_Prompt + "\n" +
                    "Checkpoint:" + Checkpoint + "\n" +
                    "Resolution:" + Resolution + "\n" +
                    "Add_Detail:" + Add_Detail + "\n" +
                    "Controlnet:" + Controlnet_moduleString);

        Result[0].text = LoraMap[Select[0]];
        Result[1].text = Main_Prompt;
        Result[2].text = CheckpointMap[Checkpoint];
        Result[3].text = (Select[9] != "") ? Select[9] : "None";
        Result[4].text = (Select[10] != "") ? Select[10] : "None";
        Texture2D img1 = null;
        Option.SetActive(false);
        GameScene.SetActive(false);
        AgentScene.SetActive(false);
        ResultScene.SetActive(true);
        Result_Image.sprite = System.Array.Find(
                    Hint,
                    sprite => sprite.name.Contains("生成圖片")
                );
        yield return StartCoroutine(multi.stableDiffusionRegionPrompt.GenerateImageForAgent(Resolution, Resolution, Prompt, Neg_Prompt, Checkpoint, Controlnet_modelString, Controlnet_moduleString, Controlnet_Image, Add_Detail,
            texture =>
            {
                img1 = texture;
            }));
        Result_Image.sprite = Sprite.Create(img1, new Rect(0, 0, img1.width, img1.height), new Vector2(0.5f, 0.5f));
    }

    IEnumerator StartAgentMode()
    {
        Step = -1;
        multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(0);
        Next = false;
        AllHint.SetActive(true);
        SkipButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Skip";
        yield return new WaitUntil(() => Next);
        Sprite[] Hint = Resources.LoadAll<Sprite>("Agent模式圖片");
        if (IsAgent)
        {
            Debug.Log("NO");
            FinishAgent = false;
            yield return new WaitUntil(() => FinishAgent);
            if (Select[3] == "")
            {
                Step = 3;
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(2);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
                multi.ChangeAgentButton(new string[] { "可愛動畫", "擬真動畫", "柔和動畫", "現實風格", "插畫動畫" }, 4, true);
                for (int i = 0; i < 4; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                Next = false;
                SkipButton.SetActive(false);
                InputButton.SetActive(false);
                multi.buttons[3].gameObject.SetActive(true);
            }
            yield return new WaitUntil(() => Next);
            if (Select[7] == "yes")
            {
                Step = 7;
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(6);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
                multi.ChangeAgentButton(new string[] { Select[6] + "1", Select[6] + "2", Select[6] + "3", Select[6] + "4" }, 4);

                for (int i = 0; i < 4; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                Next = false;
                SkipButton.SetActive(true);
                InputButton.SetActive(false);
                multi.buttons[3].gameObject.SetActive(true);
            }
            yield return new WaitUntil(() => Next);
            if (Select[11] == "")
            {
                Step = 11;
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(10);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
                multi.ChangeAgentButton(new string[] { "384", "1024", "512", "768" }, 3, true);

                for (int i = 0; i < 3; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                multi.HintImage[3].sprite = System.Array.Find(
                       Hint,
                       sprite => sprite.name.Contains("自己決定")
                   );
                Next = false;
                SkipButton.SetActive(false);
                InputButton.SetActive(true);
                multi.buttons[3].gameObject.SetActive(false);
            }
            yield return new WaitUntil(() => Next);
            editResultObjects[0].GetComponent<TMP_Text>().text = LoraMap[Select[0]];
            editResultObjects[1].GetComponent<TMP_Text>().text = Select[4];
            string Checkpoint="";
            switch (Select[3])
            {
                case "可愛動畫":
                    Checkpoint = "anime_cute.safetensors";
                    break;
                case "擬真動畫":
                    Checkpoint = "anime-real_hybrid.safetensors";
                    break;
                case "柔和動畫":
                    Checkpoint = "anime_soft.safetensors";
                    break;
                case "現實風格":
                    Checkpoint = "realistic_anything.safetensors";
                    break;
                case "插畫動畫":
                    Checkpoint = "anime_bold.safetensors";
                    break;
                default:
                    break;
            }
            editResultObjects[2].GetComponent<TMP_Text>().text = CheckpointMap[Checkpoint];
            editResultObjects[3].GetComponent<TMP_Text>().text = Select[9];
            editResultObjects[4].GetComponent<TMP_Text>().text = Select[10];
            switch (Select[6])
            {
                case "跳躍":
                    editResultObjects[5].GetComponent<TMP_Text>().text = "jumping";
                    break;
                case "跑步":
                    editResultObjects[5].GetComponent<TMP_Text>().text = "running";
                    break;
                case "坐著":
                    editResultObjects[5].GetComponent<TMP_Text>().text = "sitting";
                    break;
                case "站立":
                    editResultObjects[5].GetComponent<TMP_Text>().text = "standing";
                    break;
                default:
                    if (Select[6] != "")
                    {
                        editResultObjects[5].GetComponent<TMP_Text>().text = Select[6];
                    }
                    else
                        editResultObjects[5].GetComponent<TMP_Text>().text = "None";
                    break;
            }
            GameScene.SetActive(false);
            AgentCheckScene.SetActive(true);
        }
        else
        {
            
            OtherDetailButton.SetActive(false);
            for (int i = 0; i < 4; i++)
            {
                multi.buttons[i].gameObject.SetActive(true);
                multi.HintImage[i].gameObject.SetActive(true);
            }
            InputButton.SetActive(false);
            OtherDetailButton.SetActive(false);
            if (Step == 0)
            {
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(1);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
                multi.ChangeAgentButton(new string[] { "女性漢服", "黑白漫畫", "可愛貓咪", "中國水墨畫", "盒玩人偶", "吉卜力", "漂亮眼睛", "食物照片" },4 , true);
                for(int i = 0; i < 4; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                Next = false;
                SkipButton.SetActive(true);
                InputButton.SetActive(false);
                multi.buttons[3].gameObject.SetActive(true);
            }
            else if(Step == 0)
            {
                Step += 1;
            }
            yield return new WaitUntil(() => Next);
            if (Step == 1 && (new[] {"女性漢服"}.Contains(Select[0])))
            {
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(13);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
                Select[4] = "女生";
                Select[5] = "成年人";
                Select[7] = "nohand";
                multi.ChangeAgentButton(new string[] { "明朝", "宋朝", "唐朝", "晉朝", "漢朝" }, 4, true);
                for (int i = 0; i < 4; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                Next = false;
                SkipButton.SetActive(false);
                InputButton.SetActive(false);
                multi.buttons[3].gameObject.SetActive(true);
            }
            else if(Step == 1)
            {
                Step += 1;
            }
            yield return new WaitUntil(() => Next);
            if (Step == 2 && (new[] { "食物照片" }.Contains(Select[0])))
            {
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(14);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
            
                Select[4] = "";

                multi.ChangeAgentButton(new string[] { "hot pot", "shumai", "hamburger", "pizza", "spaghetti", "fried chicken", "ice cream", "pancakes", "apple" }, 4, true);
                for (int i = 0; i < 4; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                Next = false;
                SkipButton.SetActive(false);
                InputButton.SetActive(false);
                multi.buttons[3].gameObject.SetActive(true);
            }
            else if (Step == 2)
            {
                Step += 1;
            }
            yield return new WaitUntil(() => Next);
            if (Step == 3 && (!new[] {"盒玩人偶", "吉卜力", "食物照片" , "黑白漫畫", "可愛貓咪", "中國水墨畫", "盒玩人偶", "漂亮眼睛" }.Contains(Select[0])))
            {
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(2);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
                multi.ChangeAgentButton(new string[] { "可愛動畫", "擬真動畫", "柔和動畫", "現實風格", "插畫動畫" }, 4, true);
                for (int i = 0; i < 4; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                Next = false;
                SkipButton.SetActive(false);
                InputButton.SetActive(false);
                multi.buttons[3].gameObject.SetActive(true);
            }
            else if (Step == 3)
            {
                Step += 1;
            }
            yield return new WaitUntil(() => Next);
            if (Step == 4 && (!new[] { "女性漢服", "可愛貓咪", "盒玩人偶", "漂亮眼睛", "食物照片" }.Contains(Select[0])))
            {
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(3);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
                multi.ChangeAgentButton(new string[] { "男生", "女生", "隨機"}, 3);

                for (int i = 0; i < 2; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(Select[3] + multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                for (int i = 2; i < 3; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                multi.HintImage[3].sprite = System.Array.Find(
                       Hint,
                       sprite => sprite.name.Contains("自己決定")
                   );
                Next = false;
                SkipButton.SetActive(false);
                InputButton.SetActive(true);
                multi.buttons[3].gameObject.SetActive(false);
            }
            else if (Step == 4)
            {
                Step += 1;
            }
            yield return new WaitUntil(() => Next);
            multi.buttons[3].gameObject.SetActive(true);
            if (Step == 5 && (!new[] { "女性漢服", "可愛貓咪", "盒玩人偶", "漂亮眼睛", "食物照片" }.Contains(Select[0])))
            {
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(4);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
                multi.ChangeAgentButton(new string[] { "小孩", "成年人", "老人", "隨機" }, 4);
                string[] Comparison = new string[] { "男孩", "女孩", "男生" , "女生", "阿公", "阿嬤" };
                if (Select[4] == "隨機")
                {
                    int rand = UnityEngine.Random.Range(0, 2);
                    Select[4] = (rand == 0) ? "男生" : "女生";
                }
                for (int i = 0; i < 3; i++)
                {
                    int ADD = 0;
                    if (Select[4] == "男生"|| Select[4] == "女生")
                        ADD = i * 2 + ((Select[4] == "男生") ? 0 : 1);
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(Select[3] + Comparison[ADD] )
                    );
                }
                for (int i = 3; i < 4; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                Next = false;
                SkipButton.SetActive(false);

                InputButton.SetActive(false);
                multi.buttons[3].gameObject.SetActive(true);
            }
            else if (Step == 5)
            {
                Step += 1;
            }
            yield return new WaitUntil(() => Next);
            if (Step == 6 && (!new[] { "女性漢服", "可愛貓咪", "漂亮眼睛", "食物照片" }.Contains(Select[0])))
            {
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(5);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1]; 
                multi.ChangeAgentButton(new string[] { "跳躍", "跑步", "坐著", "站立" }, 3, true);
            
                for (int i = 0; i < 3; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                multi.HintImage[3].sprite = System.Array.Find(
                       Hint,
                       sprite => sprite.name.Contains("自己決定")
                   );
                Next = false;
                SkipButton.SetActive(false);

                InputButton.SetActive(true);
                multi.buttons[3].gameObject.SetActive(false);
            }
            else if (Step == 6)
            {
                Step += 1;
            }
            yield return new WaitUntil(() => Next);
            if (Step == 7 && (!new[] { "女性漢服", "可愛貓咪", "盒玩人偶", "漂亮眼睛", "食物照片" }.Contains(Select[0])))
            {
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(6);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
                multi.ChangeAgentButton(new string[] { Select[6]+"1", Select[6] + "2", Select[6] + "3", Select[6] + "4" }, 4);

                for (int i = 0; i < 4; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                Next = false;
                SkipButton.SetActive(true);
                InputButton.SetActive(false);
                multi.buttons[3].gameObject.SetActive(true);
            }
            else if (Step == 7)
            {
                Step += 1;
            }
            yield return new WaitUntil(() => Next);
            if (Step == 8 && (!new[] { "可愛貓咪", "漂亮眼睛", "食物照片" }.Contains(Select[0])))
            {
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(7);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
                multi.ChangeAgentButton(new string[] { "laughing", "smiling", "relaxed", "crying", "scared", "angry" }, 3, true);
            
                for (int i = 0; i < 3; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                multi.HintImage[3].sprite = System.Array.Find(
                       Hint,
                       sprite => sprite.name.Contains("自己決定")
                   );
                Next = false;
                SkipButton.SetActive(true);
                InputButton.SetActive(true);
                multi.buttons[3].gameObject.SetActive(false);
            }
            else if (Step == 8)
            {
                Step += 1;
            }
            yield return new WaitUntil(() => Next);
            if (Step == 9 && (!new[] { "黑白漫畫", "中國水墨畫" }.Contains(Select[0])))
            {
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(8);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
                multi.ChangeAgentButton(new string[] { "red", "blue", "green", "yellow", "purple","orange", "pink", "black", "white", "gray", "brown" }, 3, true);
            
                for (int i = 0; i < 3; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                multi.HintImage[3].sprite = System.Array.Find(
                       Hint,
                       sprite => sprite.name.Contains("自己決定")
                   );
                Next = false;
                SkipButton.SetActive(true);
                InputButton.SetActive(true);
                multi.buttons[3].gameObject.SetActive(false);
            }
            else if (Step == 9)
            {
                Step += 1;
            }
            yield return new WaitUntil(() => Next);
            if (Step == 10 && (!new[] { "女性漢服", "中國水墨畫" ,"漂亮眼睛", "盒玩人偶", "食物照片" }.Contains(Select[0])))
            {
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(9);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
                multi.ChangeAgentButton(new string[] {"desert", "forest", "beach", "grassland", "lake", "blizzard", "sunset", "foggy", "thunderstorm","god rays", "downtown", "oil painting", "japanese temple", "castle","classroom", "bedroom", "magic forest"}, 3, true);
            
                for (int i = 0; i < 3; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                multi.HintImage[3].sprite = System.Array.Find(
                       Hint,
                       sprite => sprite.name.Contains("自己決定")
                   );
                Next = false;
                SkipButton.SetActive(true);
                InputButton.SetActive(true);
                multi.buttons[3].gameObject.SetActive(false);
            }
            else if (Step == 10)
            {
                Step += 1;
            }
            yield return new WaitUntil(() => Next);
            if (Step == 11 && (!new[] { "漂亮眼睛" }.Contains(Select[0])))
            {
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(10);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
                multi.ChangeAgentButton(new string[] { "384", "1024", "512", "768" }, 3, true);
            
                for (int i = 0; i < 3; i++)
                {
                    multi.HintImage[i].sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                    );
                }
                multi.HintImage[3].sprite = System.Array.Find(
                       Hint,
                       sprite => sprite.name.Contains("自己決定")
                   );
                Next = false;
                SkipButton.SetActive(false);
                InputButton.SetActive(true);
                multi.buttons[3].gameObject.SetActive(false);
            }
            else if (Step == 11)
            {
                Step += 1;
            }
            yield return new WaitUntil(() => Next);
            if (Step == 12)
            {
                multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(11);
                MainText.text = AgentFlow[Step][0];
                InfoText.text = AgentFlow[Step][1];
           
                for (int i = 0; i < 4; i++)
                {
                    multi.buttons[i].gameObject.SetActive(false);
                    //multi.HintImage[i].gameObject.SetActive(false);
                }
                AllHint.SetActive(false);
                Next = false;
                SkipButton.SetActive(true); SkipButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Generate";
                InputButton.SetActive(false);
                OtherDetailButton.SetActive(true);
            }
            else if (Step == 12)
            {
                Step += 1;
            }
            yield return new WaitUntil(() => Next);
            multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(12);
            StartCoroutine(ConversionInfo(false));
        }

    }

    public void OnAgentActive(InputField inputField)
    {
        string LLM_Prompt = @"
            接下來我會給你上一次的對話結果(可能無)、上一次的建議(可能無)和這一次的對話內容，你需要根據這些對話用文字描述圖片的內容，並給予一些建議
            範例輸入:上一次的對話結果:無,上一次的建議:無,這一次的對話內容:可以幫我構想圖片內容嗎，主角會是一個穿著漢服的女性，且圖片風格為動畫風
            範例輸出:圖片內容:{一張日系動畫風格的圖片躍然眼前，描繪了一位身穿齊胸襦裙的漢服少女，她站立於一片翠綠欲滴、竹影婆娑的竹林深處。陽光穿透茂密的竹葉，灑下斑駁的光影，輕輕柔柔地灑落在少女身上，也點亮了蜿蜒流淌的清澈小溪。溪水清澈見底，幾片竹葉隨波輕舞，為這片靜謐的竹林增添了幾分生機。}建議:{要不要再加入一些顏色來豐富圖片呢}
            輸入:上一次的對話結果:{1},上一次的建議:{2},這一次的對話內容:{3}
            輸出格式:圖片內容:{},建議:{}
            圖片內容和建議皆須使用大括號括起來，不要生成圖像，僅使用文字進行回覆，圖片內容字數不要超過100
        ";

        LLM_Prompt = LLM_Prompt.Replace("{1}", "{" + content.text +"}").Replace("{2}", "{" + suggestion.text + "}").Replace("{3}", "{" + inputField.text + "}");

        inputField.text = "";
        Debug.Log(LLM_Prompt);
        StartCoroutine(multi.stableDiffusionRegionPrompt.geminiAPI.SendRequest(LLM_Prompt, (result) => {
            MatchCollection matches = Regex.Matches(result, @"\{(.*?)\}");
            List<string> results = new List<string>();

            foreach (Match match in matches)
            {
                results.Add(match.Groups[1].Value);
            }
            content.text = (results[0] != null) ? results[0] : "抱歉我不太懂你的意思";
            suggestion.text = (results[1] != null)? results[1] : "抱歉我不太懂你的意思";
            AgentCount--;
            if(AgentCount == 0)
            {
                SkipChat();
            }
        }));
        
    }
    public void SkipChat()
    {
        string LLM_Prompt = @"接下來我會給你一段話來描述希望的圖片內容，你需要分別告訴我這段話中有沒有:

1.主題:限定以下八種主題(1)女性漢服(2)黑白漫畫(3)可愛貓咪(4)中國水墨畫(5)盒玩人偶(6)吉卜力(7)漂亮眼睛(8)食物照片，若有的話回答對應編號，若不是這八種則回答none

2.風格:限定以下五種風格(1)可愛動畫風格(2)擬真動畫風格(3)柔和動畫風格(4)現實風格(5)插畫動畫風格，，若有的話回答對應編號，若不是這五種則回答none

3.主體:圖片中最主要的角色，只須回答一個，若沒有則回答none，並根據是不是人類回答yes/no

4.姿勢:圖片中最主要角色的姿勢，若沒有則回答none，需額外判斷姿勢是否為跳/跑/坐/站(如果是的話則用中文回答 跳躍/跑步/坐著/站立，如果不是則用英文回答)

5.色調:圖片中最主要的色調，只須回答一個，若沒有則回答none

6.背景:圖片中最主要的背景，只須回答一個，若沒有則回答none

7.其他敘述:圖片中的其他細節描述，可以回答多個，請作為圖片提示詞一個一個列出來，若沒有則無需回答

範例輸入:在灑滿陽光的窗邊，一隻毛茸茸的橘色小貓，正興奮地撲向一個鮮豔的紅色毛線球。牠圓滾滾的大眼睛專注地盯著毛線球，小小的爪子輕輕撥弄著，而一小段毛線已經纏繞在牠的腳邊。藍色的毛毯襯托著牠活潑的身影，整個畫面充滿了童趣與溫馨。

範例輸出:主題:{3}, 風格:{1}, 主體:{cat}, 主體是否為人:{no}, 姿勢:{playing}, 姿勢是否為特定姿勢:{none}, 色調:{orange}, 背景{window}, 其他敘述1:{sunlight}, 其他敘述2:{fluffy cat}, 其他敘述3:{red yarn ball}, 其他敘述4:{blue blanket}, 其他敘述5:{warm atmosphere}

輸入:{內容}

輸出格式:輸出格式:主題:{1/2/3/4/5/6/7/8或none}, 風格:{1/2/3/4/5或none},主體:{main character或none},主體是否為人:{yes或no},姿勢:{main character's pose或none}, 姿勢是否為特定姿勢:{yes或none},色調:{main color或none},背景{main background或none},其他敘述1:{image prompt},其他敘述1:{image prompt},其他敘述2:{image prompt},其他敘述3:{image prompt}...

輸出需使用英文回答(除了特定姿勢用中文)，且每個回答皆須使用大括號括起來
                ";
        LLM_Prompt = LLM_Prompt.Replace("{內容}", "{" + content.text + "}");
        StartCoroutine(multi.stableDiffusionRegionPrompt.geminiAPI.SendRequest(LLM_Prompt, (result) => {
            MatchCollection matches = Regex.Matches(result, @"\{(.*?)\}");
            List<string> results = new List<string>();

            foreach (Match match in matches)
            {
                results.Add(match.Groups[1].Value);
            }
            string[] Loralist = new string[] { "", "女性漢服", "黑白漫畫", "可愛貓咪", "中國水墨畫", "盒玩人偶", "吉卜力", "漂亮眼睛", "食物照片" };
            string[] Modellist = new string[] { "", "可愛動畫", "擬真動畫", "柔和動畫", "現實風格", "插畫動畫" };
            //主題:{3}, 風格:{4}, 主體:{young woman}, 主體是否為人:{yes}, 姿勢:{gently petting a cat}, 姿勢是否為特定姿勢:{none}, 色調:{blue}, 背景:{cozy home interior}, 其他敘述1:{blue dress}, 其他敘述2:{fluffy cat}, 其他敘述3:{sparkling eyes}, 其他敘述4:{bell on the cat's neck}, 其他敘述5:{soft sunlight}, 其他敘述6:{warm and peaceful atmosphere}, 其他敘述7:{silky hair}, 其他敘述8:{loving smile}
            Select[0] = Loralist[int.TryParse(results[0], out int index) ? index : 0];//主題
            Select[1] = "明朝";
            Select[3] = Modellist[int.TryParse(results[1], out int index2) ? index2 : 0];//風格
            Select[4] = (results[2]!="none")? results[2] : "";//主體
            Select[6] = (results[4] != "none") ? results[4] : "";//姿勢
            Select[7] = (results[3] == "yes" && results[5] == "yes") ? "yes" : "";//特定姿勢
            Select[9] = (results[6] != "none") ? results[6] : "";//色調
            Select[10] = (results[7] != "none") ? results[7] : "";//背景
            Select[11] = "";
            string combined = string.Join(",", results.Skip(7));
            Select[12] = combined;
            FinishAgent = true;
            GameScene.SetActive(true);
            AgentScene.SetActive(false);
            BigScene.SetActive(true);
        }));
    }

    public GameObject[] editObjects;
    public GameObject[] editResultObjects;
    public void EditButton()
    {
        for (int i = 0; i < editObjects.Length; i++)
        {
            editObjects[i].SetActive(true);
            if (editObjects[i].GetComponent<InputField>() != null)
            {
                var input = editObjects[i].GetComponent<InputField>();
                if (editResultObjects[i].GetComponent<TMP_Text>().text != null)
                {
                    input.text = editResultObjects[i].GetComponent<TMP_Text>().text;
                }
            }
            else
            {
                var dropdown = editObjects[i].GetComponent<TMP_Dropdown>();
                if (editResultObjects[i].GetComponent<TMP_Text>().text != null)
                {
                    int index = dropdown.options.FindIndex(option => option.text == editResultObjects[i].GetComponent<TMP_Text>().text);
                    dropdown.value = index;
                }

            }
        }
    }

    public void TickButton()
    {
        for(int i=0;i< editObjects.Length;i++)
        {
            editObjects[i].SetActive(false);
            if (editObjects[i].GetComponent<InputField>() != null)
            {
                var input = editObjects[i].GetComponent<InputField>();
                if (input.text != "")
                {
                    editResultObjects[i].GetComponent<TMP_Text>().text = input.text;
                }
                else
                {
                    editResultObjects[i].GetComponent<TMP_Text>().text = "none";
                }
            }
            else
            {
                var dropdown = editObjects[i].GetComponent<TMP_Dropdown>();
                if (dropdown.options[dropdown.value].text != "")
                {
                    editResultObjects[i].GetComponent<TMP_Text>().text = dropdown.options[dropdown.value].text;
                }
                else
                {
                    editResultObjects[i].GetComponent<TMP_Text>().text = "none";
                }

            }
        }
    }
    public void CheckSceneButton()
    {
        StartCoroutine(ConversionInfo(true));
    }
}
