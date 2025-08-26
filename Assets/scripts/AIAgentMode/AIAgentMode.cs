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
    Dictionary<string, string> CheckpointMap = new Dictionary<string, string>()
        {
            { "anime_cute.safetensors", "CuteYukiMix" },
            { "anime-real_hybrid.safetensors", "ReV Animated" },
            { "anime_soft.safetensors", "Cetus-Mix" },
            { "realistic_anything.safetensors", "DreamShaper" },
            { "anime_bold.safetensors", "Counterfeit" },
            { "","None"}
        };
    IEnumerator ConversionInfo(bool AgentMode)//Lora �~�A ���� checkpoint �D�� �~�� ���� �Ѧҹ� �� ��� �I�� �ѪR�� �Ӹ`
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
        
 
        

        if (AgentMode)
        {
            string key = LoraMap.FirstOrDefault(kvp => kvp.Value.Contains(editResultObjects[0].GetComponent<TMP_Text>().text)).Key;
            Debug.Log(key);
            Select[0] = key;
            Select[4] = editResultObjects[1].GetComponent<TMP_Text>().text;
            Select[3] = "";//���n��᭱�v�T��model
            Checkpoint = CheckpointMap.FirstOrDefault(kvp => kvp.Value.Contains(editResultObjects[2].GetComponent<TMP_Text>().text)).Key;
            Select[9] = editResultObjects[3].GetComponent<TMP_Text>().text;
            Select[10] = editResultObjects[4].GetComponent<TMP_Text>().text;
            Select[6] = editResultObjects[5].GetComponent<TMP_Text>().text;
        }
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
                Select[3] = "���e�ʵe";
                break;
            case "�i�R�߫}":
                Main_Prompt = "1 cute cat"; 
                Controlnet_modelString = "";
                Select[3] = "���u�ʵe";
                LoRa_Prompt = ",<lora:cat_20230627113759:0.7>";
                break;
            case "��������e":
                Main_Prompt = "potrait of ";
                Other_Prompt += ",shuimobysim, shukezouma";
                Select[3] = "�i�R�ʵe";
                LoRa_Prompt = ",<lora:3Guofeng3_v34:0.85> <lora:shuV2:0.9>";
                break;
            case "�����H��":
                Select[4] = "1 woman";
                LoRa_Prompt = ",<lora:blindbox_v1_mix:1>";
                Other_Prompt += ",full body, chibi";
                Controlnet_modelString = "";
                Select[3] = "���u�ʵe";
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
                Select[3] = "�i�R�ʵe";
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
        AgentScene.SetActive(false);
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
                       sprite => sprite.name.Contains("�ۤv�M�w")
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
            editResultObjects[2].GetComponent<TMP_Text>().text = CheckpointMap[Checkpoint];
            editResultObjects[3].GetComponent<TMP_Text>().text = Select[9];
            editResultObjects[4].GetComponent<TMP_Text>().text = Select[10];
            switch (Select[6])
            {
                case "���D":
                    editResultObjects[5].GetComponent<TMP_Text>().text = "jumping";
                    break;
                case "�]�B":
                    editResultObjects[5].GetComponent<TMP_Text>().text = "running";
                    break;
                case "����":
                    editResultObjects[5].GetComponent<TMP_Text>().text = "sitting";
                    break;
                case "����":
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
            StartCoroutine(ConversionInfo(false));
        }

    }

    public void OnAgentActive(InputField inputField)
    {
        string LLM_Prompt = @"
            ���U�ӧڷ|���A�W�@������ܵ��G(�i��L)�B�W�@������ĳ(�i��L)�M�o�@������ܤ��e�A�A�ݭn�ھڳo�ǹ�ܥΤ�r�y�z�Ϥ������e�A�õ����@�ǫ�ĳ
            �d�ҿ�J:�W�@������ܵ��G:�L,�W�@������ĳ:�L,�o�@������ܤ��e:�i�H���ںc�Q�Ϥ����e�ܡA�D���|�O�@�Ӭ�ۺ~�A���k�ʡA�B�Ϥ����欰�ʵe��
            �d�ҿ�X:�Ϥ����e:{�@�i��t�ʵe���檺�Ϥ��D�M���e�A�yø�F�@�쨭������˸Ȫ��~�A�֤k�A�o���ߩ�@���A����w�B�˼v�C�P���˪L�`�B�C������z�Z�K���˸��A�x�U���骺���v�A�����X�X�a�x���b�֤k���W�A�]�I�G�F�d��y�I���M���p�ˡC�ˤ��M�������A�X���˸��H�i���R�A���o���R�Ī��˪L�W�K�F�X���;��C}��ĳ:{�n���n�A�[�J�@���C����״I�Ϥ��O}
            ��J:�W�@������ܵ��G:{1},�W�@������ĳ:{2},�o�@������ܤ��e:{3}
            ��X�榡:�Ϥ����e:{},��ĳ:{}
            �Ϥ����e�M��ĳ�Ҷ��ϥΤj�A���A�_�ӡA���n�ͦ��Ϲ��A�ȨϥΤ�r�i��^�СA�Ϥ����e�r�Ƥ��n�W�L100
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
            content.text = (results[0] != null) ? results[0] : "��p�ڤ������A���N��";
            suggestion.text = (results[1] != null)? results[1] : "��p�ڤ������A���N��";
            AgentCount--;
            if(AgentCount == 0)
            {
                SkipChat();
            }
        }));
        
    }
    public void SkipChat()
    {
        string LLM_Prompt = @"���U�ӧڷ|���A�@�q�ܨӴy�z�Ʊ檺�Ϥ����e�A�A�ݭn���O�i�D�ڳo�q�ܤ����S��:

1.�D�D:���w�H�U�K�إD�D(1)�k�ʺ~�A(2)�¥պ��e(3)�i�R�߫}(4)��������e(5)�����H��(6)�N�R�O(7)�}�G����(8)�����Ӥ��A�Y�����ܦ^�������s���A�Y���O�o�K�ثh�^��none

2.����:���w�H�U���ح���(1)�i�R�ʵe����(2)���u�ʵe����(3)�X�M�ʵe����(4)�{�ꭷ��(5)���e�ʵe����A�A�Y�����ܦ^�������s���A�Y���O�o���ثh�^��none

3.�D��:�Ϥ����̥D�n������A�u���^���@�ӡA�Y�S���h�^��none�A�îھڬO���O�H���^��yes/no

4.����:�Ϥ����̥D�n���⪺���աA�Y�S���h�^��none�A���B�~�P�_���լO�_����/�]/��/��(�p�G�O���ܫh�Τ���^�� ���D/�]�B/����/���ߡA�p�G���O�h�έ^��^��)

5.���:�Ϥ����̥D�n����աA�u���^���@�ӡA�Y�S���h�^��none

6.�I��:�Ϥ����̥D�n���I���A�u���^���@�ӡA�Y�S���h�^��none

7.��L�ԭz:�Ϥ�������L�Ӹ`�y�z�A�i�H�^���h�ӡA�Ч@���Ϥ����ܵ��@�Ӥ@�ӦC�X�ӡA�Y�S���h�L�ݦ^��

�d�ҿ�J:�b�x������������A�@�����񪺾��p�ߡA�����Ħa���V�@���A�v�������u�y�C�e��u�u���j�����M�`�a�n�ۤ�u�y�A�p�p�����l�������˵ۡA�Ӥ@�p�q��u�w�g��¶�b�e���}��C�Ŧ⪺���Ũ���ۨe���⪺���v�A��ӵe���R���F����P���ɡC

�d�ҿ�X:�D�D:{3}, ����:{1}, �D��:{cat}, �D��O�_���H:{no}, ����:{playing}, ���լO�_���S�w����:{none}, ���:{orange}, �I��{window}, ��L�ԭz1:{sunlight}, ��L�ԭz2:{fluffy cat}, ��L�ԭz3:{red yarn ball}, ��L�ԭz4:{blue blanket}, ��L�ԭz5:{warm atmosphere}

��J:{���e}

��X�榡:��X�榡:�D�D:{1/2/3/4/5/6/7/8��none}, ����:{1/2/3/4/5��none},�D��:{main character��none},�D��O�_���H:{yes��no},����:{main character's pose��none}, ���լO�_���S�w����:{yes��none},���:{main color��none},�I��{main background��none},��L�ԭz1:{image prompt},��L�ԭz1:{image prompt},��L�ԭz2:{image prompt},��L�ԭz3:{image prompt}...

��X�ݨϥέ^��^��(���F�S�w���եΤ���)�A�B�C�Ӧ^���Ҷ��ϥΤj�A���A�_��
                ";
        LLM_Prompt = LLM_Prompt.Replace("{���e}", "{" + content.text + "}");
        StartCoroutine(multi.stableDiffusionRegionPrompt.geminiAPI.SendRequest(LLM_Prompt, (result) => {
            MatchCollection matches = Regex.Matches(result, @"\{(.*?)\}");
            List<string> results = new List<string>();

            foreach (Match match in matches)
            {
                results.Add(match.Groups[1].Value);
            }
            string[] Loralist = new string[] { "", "�k�ʺ~�A", "�¥պ��e", "�i�R�߫}", "��������e", "�����H��", "�N�R�O", "�}�G����", "�����Ӥ�" };
            string[] Modellist = new string[] { "", "�i�R�ʵe", "���u�ʵe", "�X�M�ʵe", "�{�ꭷ��", "���e�ʵe" };
            //�D�D:{3}, ����:{4}, �D��:{young woman}, �D��O�_���H:{yes}, ����:{gently petting a cat}, ���լO�_���S�w����:{none}, ���:{blue}, �I��:{cozy home interior}, ��L�ԭz1:{blue dress}, ��L�ԭz2:{fluffy cat}, ��L�ԭz3:{sparkling eyes}, ��L�ԭz4:{bell on the cat's neck}, ��L�ԭz5:{soft sunlight}, ��L�ԭz6:{warm and peaceful atmosphere}, ��L�ԭz7:{silky hair}, ��L�ԭz8:{loving smile}
            Select[0] = Loralist[int.TryParse(results[0], out int index) ? index : 0];//�D�D
            Select[1] = "����";
            Select[3] = Modellist[int.TryParse(results[1], out int index2) ? index2 : 0];//����
            Select[4] = (results[2]!="none")? results[2] : "";//�D��
            Select[6] = (results[4] != "none") ? results[4] : "";//����
            Select[7] = (results[3] == "yes" && results[5] == "yes") ? "yes" : "";//�S�w����
            Select[9] = (results[6] != "none") ? results[6] : "";//���
            Select[10] = (results[7] != "none") ? results[7] : "";//�I��
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
