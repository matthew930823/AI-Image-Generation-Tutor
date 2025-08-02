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
        new string[] { "��D�D", "�A���S���Q�n����S�w���Ϥ��D�D�O"},
        new string[] { "��~�A", "�A�Ʊ�~�A�O����¥N���O"},
        new string[] { "�ﭹ��", "�A�Ʊ�O���򭹪����Ӥ��O"},
        new string[] { "�ﭷ��", "�A�Ʊ�Ϥ�������O���˪��O" },
        new string[] { "��D��", "�A�Ʊ�D��O����O" },
        new string[] { "��~��", "�A�Ʊ�H�����~�֦p��O" },
        new string[] { "�﫺��", "�A�Ʊ�H�������լO����O" },
        new string[] { "��Ѧҹ�", "�A�Ʊ�H�������D/�]�B/����/���ߪ����ժ����˩O" },
        new string[] { "���", "�A�Ʊ�H�������O���˪��O" },
        new string[] { "����", "�A�Ʊ�Ϥ�����լO���˪��O" },
        new string[] { "��I��", "�A�Ʊ�Ϥ����I���O���˪��O" },
        new string[] { "��ѪR��", "�A�Ʊ�ͦ����Ϥ��ѪR�צp��O�A�ڭ̥ͦ����Ϥ��T�w������γ�" },
        new string[] { "�B�~�Ӹ`", "�A�٦�����Ӹ`�Q�ɥR���ܡA�Ф@����g�@�ӲӸ`��" },
    };
    public string[] Select = new string[13];
    public TMP_Text MainText;
    public TMP_Text InfoText;
    private bool Next = false;
    private int Step = -1;
    public MultiChoiceQuestion multi;

    public GameObject SkipButton;

    public Image Result_Image;

    public GameObject GameScene;
    public GameObject ResultScene;
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

    public void OnAddOtherButton(InputField inputField)
    {
        if (inputField.text != "")
        {
            Select[Step] += ", " + inputField.text;
            inputField.text = "";
        }
    }
    IEnumerator ConversionInfo()//Lora �~�A ���� checkpoint �D�� �~�� ���� �Ѧҹ� �� ��� �I�� �ѪR�� �Ӹ`
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
        Sprite[] Hint = Resources.LoadAll<Sprite>("Agent�Ҧ��Ϥ�");

        string Controlnet_modelString = (Select[7]!="")?"control_v11p_sd15_openpose [cab727d4]":"";
        string Controlnet_moduleString = "none";

        string Controlnet_Image = Convert.ToBase64String(System.Array.Find(
                   Hint,
                   sprite => sprite.name.Contains(Select[7])
               ).texture.EncodeToPNG());
        
        Dictionary<string, string> dynastyMap = new Dictionary<string, string>()
        {
            { "����", "ming style" },
            { "����", "song style" },
            { "���", "tang style" },
            { "�ʴ�", "jin style" },
            { "�~��", "han style" }
        };
        Dictionary<string, string> CheckpointMap = new Dictionary<string, string>()
        {
            { "anime_cute.safetensors", "CuteYukiMix" },
            { "anime-real_hybrid.safetensors", "ReV Animated" },
            { "anime_soft.safetensors", "Cetus-Mix" },
            { "realistic_anything.safetensors", "DreamShaper" },
            { "anime_bold.safetensors", "Counterfeit" }
        };
        Dictionary<string, string> LoraMap = new Dictionary<string, string>()
        {
            { "�k�ʺ~�A", "Hanfu" },
            { "�¥պ��e", "Manga" },
            { "�i�R�߫}", "Cutecat" },
            { "��������e", "Inkpainting" },
            { "�����H��", "Doll" },
            { "�N�R�O", "Ghibli" },
            { "�}�G����", "Eye" },
            { "�����Ӥ�", "Foodphoto" },
            { "", "None" }
        };
        switch (Select[0]) 
        {
            case "�k�ʺ~�A":
                Controlnet_modelString = "control_v11f1p_sd15_depth [cfd03158]";
                Controlnet_moduleString = "none";
                LoRa_Prompt = ",<lora:hanfu40-beta-3:0.6>";
                Other_Prompt += ",hanfu," + dynastyMap[Select[1]];
                break;
            case "�¥պ��e":
                LoRa_Prompt = ",lineart, ((monochrome)),<lora:animeoutlineV4_16:1.3>";
                Neg_Prompt = "easynegative, (badhandv4:1.2), NSFW, watermark jpeg artifacts signature watermark username blurry";
                Checkpoint = "anime_bold.safetensors";
                break;
            case "�i�R�߫}":
                Main_Prompt = "1 cute cat"; 
                Controlnet_modelString = "";
                Checkpoint = "anime-real_hybrid.safetensors";
                LoRa_Prompt = ",<lora:cat_20230627113759:0.7>";
                break;
            case "��������e":
                Main_Prompt = "potrait of ";
                Other_Prompt += ",shuimobysim, shukezouma";
                Checkpoint = "anime_cute.safetensors";
                LoRa_Prompt = ",<lora:3Guofeng3_v34:0.85> <lora:shuV2:0.9>";
                break;
            case "�����H��":
                Select[4] = "1 woman";
                LoRa_Prompt = ",<lora:blindbox_v1_mix:1>";
                Other_Prompt += ",full body, chibi";
                Controlnet_modelString = "";
                Checkpoint = "anime-real_hybrid.safetensors";
                break;
            case "�N�R�O":
                LoRa_Prompt = ",<lora:ghibli_style_offset:1>";
                Select[3] = "�X�M�ʵe";
                Other_Prompt += ",ghibli style";
                break;
            case "�}�G����":
                LoRa_Prompt = ",<lora:Loraeyes_V1:0.8>";
                Main_Prompt = "1 eye";
                Other_Prompt += ",loraeyes";
                Controlnet_modelString = "";
                Checkpoint = "anime_cute.safetensors";
                Resolution = 512;
                break;
            case "�����Ӥ�":
                Main_Prompt = Select[2];
                Select[3] = "�{�ꭷ��";
                LoRa_Prompt = ",<lora:foodphoto:0.6>";
                Other_Prompt += ",foodphoto";
                break;
            default:
                LoRa_Prompt = "";
                break;
        }
        switch (Select[3])
        {
            case "�i�R�ʵe":
                Checkpoint = "anime_cute.safetensors";
                break;
            case "���u�ʵe":
                Checkpoint = "anime-real_hybrid.safetensors";
                break;
            case "�X�M�ʵe":
                Checkpoint = "anime_soft.safetensors";
                break;
            case "�{�ꭷ��":
                Checkpoint = "realistic_anything.safetensors";
                break;
            case "���e�ʵe":
                Checkpoint = "anime_bold.safetensors";
                break;
            default:
                break;
        }
        switch (Select[4])
        {
            case "�k��":
                Add_Detail = "detail face";
                if (Select[5] == "�p��")
                {
                    Main_Prompt += "1 little boy";
                    Add_Detail = "cute face,little boy";
                }
                else if (Select[5] == "�ѤH")
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
            case "�k��":
                Add_Detail = "detail face";
                if (Select[5] == "�p��")
                {
                    Main_Prompt += "1 little girl";
                    Add_Detail = "cute face,little girl";
                }
                else if (Select[5] == "�ѤH")
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
            case "���D":
                Other_Prompt += ",jumping";
                Result[5].text = "jumping";
                break;
            case "�]�B":
                Other_Prompt += ",running";
                Result[5].text = "running";
                break;
            case "����":
                Other_Prompt += ",sitting";
                Result[5].text = "sitting";
                break;
            case "����":
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
        ResultScene.SetActive(true);
        Result_Image.sprite = System.Array.Find(
                    Hint,
                    sprite => sprite.name.Contains("�ͦ��Ϥ�")
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
        Sprite[] Hint = Resources.LoadAll<Sprite>("Agent�Ҧ��Ϥ�");
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
            multi.ChangeAgentButton(new string[] { "�k�ʺ~�A", "�¥պ��e", "�i�R�߫}", "��������e", "�����H��", "�N�R�O", "�}�G����", "�����Ӥ�" },4 , true);
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
        if (Step == 1 && (new[] {"�k�ʺ~�A"}.Contains(Select[0])))
        {
            multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(13);
            MainText.text = AgentFlow[Step][0];
            InfoText.text = AgentFlow[Step][1];
            Select[4] = "�k��";
            Select[5] = "���~�H";
            Select[7] = "nohand";
            multi.ChangeAgentButton(new string[] { "����", "����", "���", "�ʴ�", "�~��" }, 4, true);
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
        if (Step == 2 && (new[] { "�����Ӥ�" }.Contains(Select[0])))
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
        if (Step == 3 && (!new[] {"�����H��", "�N�R�O", "�����Ӥ�" , "�¥պ��e", "�i�R�߫}", "��������e", "�����H��", "�}�G����" }.Contains(Select[0])))
        {
            multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(2);
            MainText.text = AgentFlow[Step][0];
            InfoText.text = AgentFlow[Step][1];
            multi.ChangeAgentButton(new string[] { "�i�R�ʵe", "���u�ʵe", "�X�M�ʵe", "�{�ꭷ��", "���e�ʵe" }, 4, true);
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
        if (Step == 4 && (!new[] { "�k�ʺ~�A", "�i�R�߫}", "�����H��", "�}�G����", "�����Ӥ�" }.Contains(Select[0])))
        {
            multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(3);
            MainText.text = AgentFlow[Step][0];
            InfoText.text = AgentFlow[Step][1];
            multi.ChangeAgentButton(new string[] { "�k��", "�k��", "�H��"}, 3);

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
                   sprite => sprite.name.Contains("�ۤv�M�w")
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
        if (Step == 5 && (!new[] { "�k�ʺ~�A", "�i�R�߫}", "�����H��", "�}�G����", "�����Ӥ�" }.Contains(Select[0])))
        {
            multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(4);
            MainText.text = AgentFlow[Step][0];
            InfoText.text = AgentFlow[Step][1];
            multi.ChangeAgentButton(new string[] { "�p��", "���~�H", "�ѤH", "�H��" }, 4);
            string[] Comparison = new string[] { "�k��", "�k��", "�k��" , "�k��", "����", "����" };
            if (Select[4] == "�H��")
            {
                int rand = UnityEngine.Random.Range(0, 2);
                Select[4] = (rand == 0) ? "�k��" : "�k��";
            }
            for (int i = 0; i < 3; i++)
            {
                int ADD = 0;
                if (Select[4] == "�k��"|| Select[4] == "�k��")
                    ADD = i * 2 + ((Select[4] == "�k��") ? 0 : 1);
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
        if (Step == 6 && (!new[] { "�k�ʺ~�A", "�i�R�߫}", "�}�G����", "�����Ӥ�" }.Contains(Select[0])))
        {
            multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(5);
            MainText.text = AgentFlow[Step][0];
            InfoText.text = AgentFlow[Step][1]; 
            multi.ChangeAgentButton(new string[] { "���D", "�]�B", "����", "����" }, 3, true);
            
            for (int i = 0; i < 3; i++)
            {
                multi.HintImage[i].sprite = System.Array.Find(
                    Hint,
                    sprite => sprite.name.Contains(multi.buttons[i].GetComponentInChildren<Text>().text)
                );
            }
            multi.HintImage[3].sprite = System.Array.Find(
                   Hint,
                   sprite => sprite.name.Contains("�ۤv�M�w")
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
        if (Step == 7 && (!new[] { "�k�ʺ~�A", "�i�R�߫}", "�����H��", "�}�G����", "�����Ӥ�" }.Contains(Select[0])))
        {
            multi.stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(6);
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
                   sprite => sprite.name.Contains("�ۤv�M�w")
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
        if (Step == 8 && (!new[] { "�i�R�߫}", "�}�G����", "�����Ӥ�" }.Contains(Select[0])))
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
                   sprite => sprite.name.Contains("�ۤv�M�w")
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
        if (Step == 9 && (!new[] { "�¥պ��e", "��������e" }.Contains(Select[0])))
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
                   sprite => sprite.name.Contains("�ۤv�M�w")
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
        if (Step == 10 && (!new[] { "�k�ʺ~�A", "��������e" ,"�}�G����", "�����H��", "�����Ӥ�" }.Contains(Select[0])))
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
                   sprite => sprite.name.Contains("�ۤv�M�w")
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
        if (Step == 11 && (!new[] { "�}�G����" }.Contains(Select[0])))
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
                   sprite => sprite.name.Contains("�ۤv�M�w")
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
        StartCoroutine(ConversionInfo());
    }

    
}
