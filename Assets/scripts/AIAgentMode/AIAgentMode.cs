using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public int Step = 0;
    public MultiChoiceQuestion multi;

    public GameObject SkipButton;

    public Image Result_Image;

    public GameObject GameScene;
    public GameObject ResultScene;
    public GameObject Option;

    public GameObject InputButton;
    public GameObject OtherDetailButton;
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

    public void OnAddOtherButton(InputField inputField)
    {
        if (inputField.text != "")
        {
            Select[Step] += ", " + inputField.text;
            inputField.text = "";
        }
    }
    IEnumerator ConversionInfo()//Lora 漢服 食物 checkpoint 主體 年齡 姿勢 參考圖 表情 色調 背景 解析度 細節
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

        int Resolution = int.Parse(Select[11]);
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
                Checkpoint = "anime_bold.safetensors";
                break;
            case "可愛貓咪":
                Main_Prompt = "1 cute cat"; 
                Controlnet_modelString = "";
                Checkpoint = "anime-real_hybrid.safetensors";
                LoRa_Prompt = ",<lora:cat_20230627113759:0.7>";
                break;
            case "中國水墨畫":
                Main_Prompt = "potrait of ";
                Other_Prompt += ",shuimobysim, shukezouma";
                Checkpoint = "anime_cute.safetensors";
                LoRa_Prompt = ",<lora:3Guofeng3_v34:0.85> <lora:shuV2:0.9>";
                break;
            case "盒玩人偶":
                int rand = UnityEngine.Random.Range(0, 2);
                Select[4] = (rand == 0) ? "男生" : "女生";
                LoRa_Prompt = ",<lora:blindbox_v1_mix:1>";
                Select[3] = "柔和動畫";
                Other_Prompt += ",full body, chibi";
                Controlnet_modelString = "";
                Checkpoint = "anime-real_hybrid.safetensors";
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
                Checkpoint = "anime_cute.safetensors";
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
                break;
            case "跑步":
                Other_Prompt += ",running";
                break;
            case "坐著":
                Other_Prompt += ",sitting";
                break;
            case "站立":
                Other_Prompt += ",standing";
                break;
            default:
                if(Select[6]!="")
                    Other_Prompt += "," + Select[6];
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
        Texture2D img1 = null;
        yield return StartCoroutine(multi.stableDiffusionRegionPrompt.GenerateImageForAgent(Resolution, Resolution, Prompt, Neg_Prompt, Checkpoint, Controlnet_modelString, Controlnet_moduleString, Controlnet_Image, Add_Detail,
            texture =>
            {
                img1 = texture;
            }));
        Result_Image.sprite = Sprite.Create(img1, new Rect(0, 0, img1.width, img1.height), new Vector2(0.5f, 0.5f));
        Option.SetActive(false);
        GameScene.SetActive(false);
        ResultScene.SetActive(true);
    }

    IEnumerator StartAgentMode()
    {
        Sprite[] Hint = Resources.LoadAll<Sprite>("Agent模式圖片");
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
            MainText.text = AgentFlow[Step][0];
            InfoText.text = AgentFlow[Step][1];
            Select[4] = "女生";
            Select[5] = "成年人";
            Select[6] = "nohand";
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
            MainText.text = AgentFlow[Step][0];
            InfoText.text = AgentFlow[Step][1];
            
            Select[4] = "食物";

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
            MainText.text = AgentFlow[Step][0];
            InfoText.text = AgentFlow[Step][1];
            multi.ChangeAgentButton(new string[] { Select[6]+"1", Select[6] + "2", Select[6] + "3" }, 3);

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
            InputButton.SetActive(false);
            multi.buttons[3].gameObject.SetActive(false);
        }
        else if (Step == 7)
        {
            Step += 1;
        }
        yield return new WaitUntil(() => Next);
        if (Step == 8 && (!new[] { "可愛貓咪", "漂亮眼睛", "食物照片" }.Contains(Select[0])))
        {   
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
            MainText.text = AgentFlow[Step][0];
            InfoText.text = AgentFlow[Step][1];
           
            for (int i = 0; i < 4; i++)
            {
                multi.buttons[i].gameObject.SetActive(false);
                multi.HintImage[i].gameObject.SetActive(false);
            }
            Next = false;
            SkipButton.SetActive(true);
            InputButton.SetActive(false);
            OtherDetailButton.SetActive(true);
        }
        else if (Step == 12)
        {
            Step += 1;
        }
        yield return new WaitUntil(() => Next);
        StartCoroutine(ConversionInfo());
    }

    
}
