using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MultiChoiceQuestion : MonoBehaviour
{
    public Button[] buttons;
    public Image BeforeImage;
    public Image AfterImage;
    public TMP_Text QuestionName;
    public Image Result_BeforeImage;
    public Image Result_AfterImage;
    public TMP_Text Result_QuestionName;
    public Text Explain;
    public Image[] HintImage = new Image[4];
    public StableDiffusionRegionPrompt stableDiffusionRegionPrompt;
    private string[] AllAnswer = { "DreamShaper",
                                "Cetus-Mix",
                                "ReV Animated",
                                "Counterfeit",
                                "Blindbox",
                                "MoXin",
                                "Lineart",
                                "Ghibli",
                                "Hanfu",
                                "Eye",
                                "Cutecat",
                                "Foodphoto",
                                "depth",
                                "canny",
                                "openpose",
                                "shuffle",
                                "DreamShaper"};
    private string[] AllQuestionName = {"A young girl reads a book indoors",
                                        "A red mecha stands in a city ready for battle",
                                        "Red bird fly in a clear sky",
                                        "Two gray cats are resting",
                                        "A little girl standing with a smile",
                                        "Several ladybugs gathered on a leaf",
                                        "A beautiful blue butterfly",
                                        "Little girl running happily on the path",
                                        "A Chinese woman standing and smiling",
                                        "A woman with a beautiful blue eye",
                                        "An orange cat on the grass",
                                        "Delicious apples with clear water",
                                        "A kneeling young woman",
                                        "A  man happily drinking coffee",
                                        "A woman dancing on the beach",
                                        "A little boy playing in the park",
                                        "A young woman singing on stage with an excited expression" };
    private string[] AllType = { "LoRa", "Checkpoint", "Prompt", "Resolution","Controlnet" };

    public bool IsResultScreen = false;
    public GameObject GameScreen;
    public GameObject ResultScreen;

    public Image[] Classification;
    // Start is called before the first frame update
    void Start()
    {
        //for (int i = 0; i < 4; i++)
        //{
        //    HintImage[i].sprite = null;
        //    Text btnText = buttons[i].GetComponentInChildren<Text>();
        //    btnText.text = "";
        //}
        //BeforeImage.sprite = null;
        //AfterImage.sprite = null;
        //QuestionName.text = "";
        //Sprite[] sprites = Resources.LoadAll<Sprite>("�D�w/LoRaHint");

        //foreach (Sprite sprite in sprites)
        //{
        //    Debug.Log("���Ϥ�: " + sprite.name);
        //}
        //GenerateQuestions();
        //StartCoroutine(stableDiffusionRegionPrompt.StartAutoImageUpdate());
    }
    public string ChangeQuestion(int Question)
    {
        QuestionName.text = AllQuestionName[Question - 1];
        string Answer = AllAnswer[Question - 1];
        string type;
        if (Question < 5) type = "dreambooth";
        else if (Question < 13) type = "LoRa";
        else if (Question < 17) type = "controlnet";
        else type = "dreambooth";
        BeforeImage.sprite = Resources.Load<Sprite>($"�D�w/q{Question}/before");
        AfterImage.sprite = Resources.Load<Sprite>($"�D�w/q{Question}/after");
        if (type == "dreambooth")
        {
            Sprite[] Hint = Resources.LoadAll<Sprite>("�D�w/dreamboothHint");
            // ���ö��ǡ]Fisher�VYates shuffle�^
            for (int i = 0; i < Hint.Length; i++)
            {
                int rand = Random.Range(i, Hint.Length);
                Sprite temp = Hint[i];
                Hint[i] = Hint[rand];
                Hint[rand] = temp;
            }

            // ���e4�i�Ϥ�
            for (int i = 0; i < 4; i++)
            {
                HintImage[i].sprite = Hint[i];
                Text btnText = buttons[i].GetComponentInChildren<Text>();
                btnText.text = Hint[i].name;
            }
        }
        else if (type == "LoRa")
        {
            Sprite[] Hint = Resources.LoadAll<Sprite>("�D�w/LoRaHint");
            // �M��W�� Answer �� Sprite
            Sprite answerSprite = null;
            List<Sprite> remaining = new List<Sprite>();

            foreach (Sprite sprite in Hint)
            {
                if (sprite.name == Answer)
                {
                    answerSprite = sprite;
                }
                else
                {
                    remaining.Add(sprite);
                }
            }
            // �H���q�ѤU���Ϥ�����X 3 �i
            List<Sprite> selected = new List<Sprite>();
            while (selected.Count < 3)
            {
                int index = Random.Range(0, remaining.Count);
                selected.Add(remaining[index]);
                remaining.RemoveAt(index); // �T�O������
            }

            // �[�J���T���׹Ϥ�
            selected.Add(answerSprite);

            // ���ö���
            for (int i = 0; i < selected.Count; i++)
            {
                int rand = Random.Range(i, selected.Count);
                var temp = selected[i];
                selected[i] = selected[rand];
                selected[rand] = temp;
            }

            // ������ HintImage[]
            for (int i = 0; i < 4; i++)
            {
                HintImage[i].sprite = selected[i];
                Text btnText = buttons[i].GetComponentInChildren<Text>();
                btnText.text = selected[i].name;
            }
        }
        else
        {
            Sprite[] Hint = Resources.LoadAll<Sprite>("�D�w/controlnetHint");
            for (int i = 0; i < Hint.Length; i++)
            {
                int rand = Random.Range(i, Hint.Length);
                Sprite temp = Hint[i];
                Hint[i] = Hint[rand];
                Hint[rand] = temp;
            }
            Debug.Log($"HintImage.Length = {Hint.Length}");

            // ���e4�i�Ϥ�
            for (int i = 0; i < 4; i++)
            {
                HintImage[i].sprite = Hint[i];
                Text btnText = buttons[i].GetComponentInChildren<Text>();
                btnText.text = Hint[i].name;
            }
        }
        return Answer;
    }
    public void ChangeOptionsForHardMode(string[] tempAns)//{ tempCP, ControlNetType, Addresult , Resolution.ToString() }
    {
        // �Ҧ��i��ﶵ
        Dictionary<string, string[]> AllOptions = new Dictionary<string, string[]>
        {
            ["ControlNet"] = new string[] { "Canny", "Depth", "Openpose", "Shuffle" },
            ["Checkpoint"] = new string[] { "Cetus-Mix", "Counterfeit", "CuteYukiMix", "DreamShaper", "ReV Animated" },
            ["Prompt"] = new string[]{"desert", "forest", "beach", "grassland", "lake", "blizzard", "sunset", "foggy", "thunderstorm",
            "god rays", "downtown", "cyberpunk", "oil painting", "watercolor", "japanese temple", "castle",
            "classroom", "bedroom", "magic forest", "lava ground", "red", "blue", "green", "yellow", "purple",
            "orange", "pink", "black", "white", "gray", "brown" },
            ["Resolution"] = new string[] { "384", "1024", "512", "768" }
        };

            // �N���T���׹�����U�����O
        Dictionary<string, string> correctAnswers = new Dictionary<string, string> {
            { "Checkpoint", tempAns[0] },
            { "ControlNet", tempAns[1] },
            { "Prompt", tempAns[2] },
            { "Resolution", tempAns[3] }
        };

        System.Random rand = new System.Random();

        // �H����X�䤤 3 �ӥ��T��������
        List<string> keys = new List<string>(correctAnswers.Keys);
        List<string> selectedKeys = keys.OrderBy(x => rand.Next()).Take(3).ToList();

        List<string> selected = new List<string>();

        // �[�J���T���ס]3�ӡ^
        foreach (var key in selectedKeys)
        {
            selected.Add(correctAnswers[key]);
        }

        // �[�J���~�ﶵ�]5�ӡ^
        while (selected.Count < 8)
        {
            // �H����@�����O
            string category = keys[rand.Next(keys.Count)];

            // ���L�w�F�W���]�C���̦h3�ӡ^
            int countInCategory = selected.Count(s => AllOptions[category].Contains(s));
            if (countInCategory >= 3) continue;

            // �q�����O���ư����T���׻P�w��L���ﶵ
            var possible = AllOptions[category].Where(opt => opt != correctAnswers[category] && !selected.Contains(opt)).ToList();
            if (possible.Count == 0) continue;

            string wrong = possible[rand.Next(possible.Count)];
            selected.Add(wrong);
        }

        // �H�����ÿﶵ
        selected = selected.OrderBy(x => rand.Next()).ToList();

        // ��ܨ���s�W
        for (int i = 0; i < 8; i++)
        {
            Text btnText = buttons[i].GetComponentInChildren<Text>();
            btnText.text = selected[i];
        }
    }

    public void ChangeHintImage(string type)
    {

        Sprite answerSprite = null;
        List<Sprite> remaining = new List<Sprite>();
        Sprite[] Hint = null;
        List<Sprite> selected = new List<Sprite>();
        switch (type)
        {
            case "LoRa":
                Hint = Resources.LoadAll<Sprite>("�D�w/LoRaHint");
                // �M��W�� Answer �� Sprite

                foreach (Sprite sprite in Hint)
                {
                    if (sprite.name == stableDiffusionRegionPrompt.gameController.answer)
                    {
                        answerSprite = sprite;
                    }
                    else
                    {
                        remaining.Add(sprite);
                    }
                }
                // �H���q�ѤU���Ϥ�����X 3 �i
                while (selected.Count < 3)
                {
                    int index = Random.Range(0, remaining.Count);
                    selected.Add(remaining[index]);
                    remaining.RemoveAt(index); // �T�O������
                }

                // �[�J���T���׹Ϥ�
                selected.Add(answerSprite);

                // ���ö���
                for (int i = 0; i < selected.Count; i++)
                {
                    int rand = Random.Range(i, selected.Count);
                    var temp = selected[i];
                    selected[i] = selected[rand];
                    selected[rand] = temp;
                }

                // ������ HintImage[]
                for (int i = 0; i < 4; i++)
                {
                    HintImage[i].sprite = selected[i];
                    Text btnText = buttons[i].GetComponentInChildren<Text>();
                    btnText.text = selected[i].name;
                }
                break;
            case "Checkpoint":
                Hint = Resources.LoadAll<Sprite>("�D�w/dreamboothHint");
                // �M��W�� Answer �� Sprite

                foreach (Sprite sprite in Hint)
                {
                    if (sprite.name == stableDiffusionRegionPrompt.gameController.answer)
                    {
                        answerSprite = sprite;
                    }
                    else
                    {
                        remaining.Add(sprite);
                    }
                }
                // �H���q�ѤU���Ϥ�����X 3 �i
                while (selected.Count < 3)
                {
                    int index = Random.Range(0, remaining.Count);
                    selected.Add(remaining[index]);
                    remaining.RemoveAt(index); // �T�O������
                }

                // �[�J���T���׹Ϥ�
                selected.Add(answerSprite);

                // ���ö���
                for (int i = 0; i < selected.Count; i++)
                {
                    int rand = Random.Range(i, selected.Count);
                    var temp = selected[i];
                    selected[i] = selected[rand];
                    selected[rand] = temp;
                }

                // ������ HintImage[]
                for (int i = 0; i < 4; i++)
                {
                    HintImage[i].sprite = selected[i];
                    Text btnText = buttons[i].GetComponentInChildren<Text>();
                    btnText.text = selected[i].name;
                }
                break;
            case "Prompt":
                Hint = Resources.LoadAll<Sprite>("�D�w/PromptHint");
                foreach (Sprite sprite in Hint)
                {
                    if (sprite.name == stableDiffusionRegionPrompt.gameController.answer)
                    {
                        answerSprite = sprite;
                    }
                    else
                    {
                        remaining.Add(sprite);
                    }
                }
                // �H���q�ѤU���Ϥ�����X 3 �i
                while (selected.Count < 3)
                {
                    int index = Random.Range(0, remaining.Count);
                    selected.Add(remaining[index]);
                    remaining.RemoveAt(index); // �T�O������
                }

                // �[�J���T���׹Ϥ�
                selected.Add(answerSprite);

                // ���ö���
                for (int i = 0; i < selected.Count; i++)
                {
                    int rand = Random.Range(i, selected.Count);
                    var temp = selected[i];
                    selected[i] = selected[rand];
                    selected[rand] = temp;
                }

                // ������ HintImage[]
                for (int i = 0; i < 4; i++)
                {
                    HintImage[i].sprite = selected[i];
                    Text btnText = buttons[i].GetComponentInChildren<Text>();
                    btnText.text = selected[i].name;
                }
                break;
            case "Resolution":
                Hint = Resources.LoadAll<Sprite>("�D�w/Resolution Hint");
                for (int i = 0; i < Hint.Length; i++)
                {
                    int rand = Random.Range(i, Hint.Length);
                    Sprite temp = Hint[i];
                    Hint[i] = Hint[rand];
                    Hint[rand] = temp;
                }

                // ���e4�i�Ϥ�
                for (int i = 0; i < 4; i++)
                {
                    HintImage[i].sprite = Hint[i];
                    Text btnText = buttons[i].GetComponentInChildren<Text>();
                    btnText.text = Hint[i].name;
                }
                break;
            case "Controlnet":
                Hint = Resources.LoadAll<Sprite>("�D�w/controlnetHint");
                for (int i = 0; i < Hint.Length; i++)
                {
                    int rand = Random.Range(i, Hint.Length);
                    Sprite temp = Hint[i];
                    Hint[i] = Hint[rand];
                    Hint[rand] = temp;
                }

                // ���e4�i�Ϥ�
                for (int i = 0; i < 4; i++)
                {
                    HintImage[i].sprite = Hint[i];
                    Text btnText = buttons[i].GetComponentInChildren<Text>();
                    btnText.text = Hint[i].name;
                }
                break;
            default:
                break;
        }
    }
    
    public string[] GenerateQuestions()
    {
        string type = "Resolution";
        int[] weights = { 1500,20, 30, 5, 30 };// { "LoRa", "Checkpoint", "Prompt", "Resolution","Controlnet" }

        int totalWeight = weights.Sum();
        int rand = Random.Range(0, totalWeight);
        int cumulative = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (rand < cumulative)
            {
                type = AllType[i];
                break;
            }
        }
        string[] AllCheckpoint = new string[] { "anime_cute.safetensors", "anime-real_hybrid.safetensors", "anime_soft.safetensors", "realistic_anything.safetensors" , "anime_bold.safetensors" };
        string randomCheckpoint = AllCheckpoint[UnityEngine.Random.Range(0, AllCheckpoint.Length)];
        switch (type)
        {
            case "LoRa":
                string[] LoRaType = new string[] { "�~�A", "���e", "��", "����", "����", "�N���Q", "����", "�����Ӥ�","Snoopy" };
                //string LoRa = LoRaType[UnityEngine.Random.Range(0, LoRaType.Length)];
                string LoRa = LoRaType[8];
                return new string[] { LoRa, randomCheckpoint, type };
                //StartCoroutine(stableDiffusionRegionPrompt.HandlePromptAndGenerateImage(LoRa, randomCheckpoint,type));
                //break;
            case "Checkpoint":
                //StartCoroutine(stableDiffusionRegionPrompt.HandlePromptAndGenerateImage("", randomCheckpoint, type));
                return new string[] { "", randomCheckpoint, type };
                //break;
            case "Prompt":
                //StartCoroutine(stableDiffusionRegionPrompt.HandlePromptAndGenerateImage("", randomCheckpoint, type));
                return new string[] { "", randomCheckpoint, type };
                //break;
            case "Resolution":
                //StartCoroutine(stableDiffusionRegionPrompt.HandlePromptAndGenerateImage("", randomCheckpoint, type));
                return new string[] { "", randomCheckpoint, type };
                //break;
            case "Controlnet":
                //StartCoroutine(stableDiffusionRegionPrompt.HandlePromptAndGenerateImage("Controlnet", randomCheckpoint, type));
                return new string[] { "Controlnet", randomCheckpoint, type };
                //break;
            default:
                return new string[] { "","","" };
                //break;
        }
    }
    public IEnumerator ChangeButtonColor(float waitSec)
    {
        yield return new WaitForSeconds(waitSec);
        GameScreen.SetActive(false);
        ResultScreen.SetActive(true);
        Result_AfterImage.sprite = AfterImage.sprite;
        Result_BeforeImage.sprite = BeforeImage.sprite;
        Result_QuestionName.text = QuestionName.text;
        for (int i = 0; i < buttons.Length; i++)
        {
            Text btnText = buttons[i].GetComponentInChildren<Text>();
            ColorBlock cb = buttons[i].colors;

            Color correctColor = new Color(0.5f, 1f, 0.5f);  // ���
            Color wrongColor = new Color(1f, 0.5f, 0.5f);     // ����

            Color targetColor = (stableDiffusionRegionPrompt.gameController.answer == btnText.text)
                                ? correctColor
                                : wrongColor;

            // �]�w�Ҧ��C�⪬�A���ؼ��C��
            cb.normalColor = targetColor;
            cb.highlightedColor = targetColor;
            cb.pressedColor = targetColor;
            cb.selectedColor = targetColor;
            cb.disabledColor = targetColor;

            buttons[i].colors = cb;
        }
        Explain.text = "�p�G���������A�I���Q���D���ﶵ�A�o�˧ڴN�|���A����������o�ӿﶵ�O�諸�ο�����";
    }
    public IEnumerator ChangeButtonColorForHardMode(float waitSec)
    {
        yield return new WaitForSeconds(waitSec);
        GameScreen.SetActive(false);
        ResultScreen.SetActive(true);
        Result_AfterImage.sprite = AfterImage.sprite;
        Result_BeforeImage.sprite = BeforeImage.sprite;
        Result_QuestionName.text = QuestionName.text;
        Debug.Log(string.Join(", ", stableDiffusionRegionPrompt.HardTempAnswer));
        for (int i = 0; i < buttons.Length; i++)
        {
            Text btnText = buttons[i].GetComponentInChildren<Text>();
            ColorBlock cb = buttons[i].colors;

            Color correctColor = new Color(0.5f, 1f, 0.5f);  // ���
            Color wrongColor = new Color(1f, 0.5f, 0.5f);     // ����

            Color targetColor = (stableDiffusionRegionPrompt.HardTempAnswer.Contains(btnText.text))
                                ? correctColor
                                : wrongColor;

            // �]�w�Ҧ��C�⪬�A���ؼ��C��
            cb.normalColor = targetColor;
            cb.highlightedColor = targetColor;
            cb.pressedColor = targetColor;
            cb.selectedColor = targetColor;
            cb.disabledColor = targetColor;

            buttons[i].colors = cb;
        }
    }
    public void ResetButtonColor()
    {
        PrepareQuestions = false;
        IsResultScreen = false;
        GameScreen.SetActive(true);
        ResultScreen.SetActive(false);
        for (int i = 0; i < buttons.Length; i++)
        {
            Text btnText = buttons[i].GetComponentInChildren<Text>();
            ColorBlock cb = buttons[i].colors;
            Color targetColor = new Color(1f, 1f, 1f);
            cb.normalColor = targetColor;
            cb.highlightedColor = targetColor;
            cb.pressedColor = targetColor;
            cb.selectedColor = targetColor;
            cb.disabledColor = targetColor;
            buttons[i].colors = cb;
        }
    }

    public void ChangeOneButtonColor(Text buttonText)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Text btnText = buttons[i].GetComponentInChildren<Text>();
            if (btnText.text == buttonText.text)
            {
                ColorBlock cb = buttons[i].colors;
                Color correctColor = new Color(0.5f, 1f, 0.5f);  // ���
                Color wrongColor = new Color(1f, 0.5f, 0.5f);     // ����
                Color BlackColor = new Color(0.5f, 0.5f, 0.5f);
                //Color targetColor = (stableDiffusionRegionPrompt.HardTempAnswer.Contains(btnText.text))
                //                    ? correctColor
                //                    : wrongColor;
                Color targetColor = BlackColor;
                // �]�w�Ҧ��C�⪬�A���ؼ��C��
                cb.normalColor = targetColor;
                cb.highlightedColor = targetColor;
                cb.pressedColor = targetColor;
                cb.selectedColor = targetColor;
                cb.disabledColor = targetColor;

                buttons[i].colors = cb;
            }
        }
    }
    public void ChangeEveryButtonColor()
    {
        Dictionary<string, string[]> AllOptions = new Dictionary<string, string[]>
        {
            ["ControlNet"] = new string[] { "Canny", "Depth", "Openpose", "Shuffle" },
            ["Checkpoint"] = new string[] { "Cetus-Mix", "Counterfeit", "CuteYukiMix", "DreamShaper", "ReV Animated" },
            ["Prompt"] = new string[]{"desert", "forest", "beach", "grassland", "lake", "blizzard", "sunset", "foggy", "thunderstorm",
            "god rays", "downtown", "cyberpunk", "oil painting", "watercolor", "japanese temple", "castle",
            "classroom", "bedroom", "magic forest", "lava ground", "red", "blue", "green", "yellow", "purple",
            "orange", "pink", "black", "white", "gray", "brown" },
            ["Resolution"] = new string[] { "384", "1024", "512", "768" }
        };
        Dictionary<string, Color> SetColor = new Dictionary<string, Color>
        {
            ["ControlNet"] = Classification[0].color,
            ["Checkpoint"] = Classification[1].color,
            ["Prompt"] = Classification[2].color,
            ["Resolution"] = Classification[3].color
        };
        string foundKey = null;
        for (int i = 0; i < buttons.Length; i++)
        {
            Text btnText = buttons[i].GetComponentInChildren<Text>();
            ColorBlock cb = buttons[i].colors;

            foreach (var pair in AllOptions)
            {
                if (pair.Value.Contains(btnText.text))
                {
                    foundKey = pair.Key;
                    break;
                }
            }
            Color targetColor = SetColor[foundKey];

            // �]�w�Ҧ��C�⪬�A���ؼ��C��
            cb.normalColor = targetColor;
            cb.highlightedColor = targetColor;
            cb.pressedColor = targetColor;
            cb.selectedColor = targetColor;
            cb.disabledColor = targetColor;

            buttons[i].colors = cb;
        }
    }
    bool PrepareQuestions = false;
    // Update is called once per frame
    void Update()
    {
        if(ResultScreen.activeSelf && stableDiffusionRegionPrompt.SkipButton.activeSelf && !PrepareQuestions)
        {
            stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(5);
            PrepareQuestions = true;
        }
    }
}
