using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Linq;

public class StableDiffusionRegionPrompt : MonoBehaviour
{
    //public RawImage imageUI;
    public Image imageUI;
    public Image imageUI2;
    public Text Narrative;
    public Texture2D img1 = null;
    public Texture2D img2 = null;
    public GeminiAPI geminiAPI;
    private string Sampler_name = "DPM++ 2M";
    private string Scheduler = "Karras";
    private string Prompt = "";
    string ControlnetImageBase64;
    string ControlNetType;
    string[] infoArray;//提取細節
    string[] LoRaType = new string[] { "漢服", "漫畫", "貓", "水墨", "盒玩", "吉普利", "眼睛", "食物照片" };
    Queue<string> MainBody = new Queue<string>();
    public MultiChoiceQuestion multiChoiceQuestion;
    string HanfuimageData;
    public GameController gameController;
    private string tempAnswer;

    public GameObject SkipButton;

    [System.Serializable]
    public class Region
    {
        public float x;
        public float y;
        public float w;
        public float h;
        public string prompt;
        public string negative_prompt; 
        public string blendMode;  // "Background" or "Foreground"
        public float feather;     // 只有 Foreground 用
    }

    [System.Serializable]
    public class MultiDiffusionWrapper
    {
        [JsonProperty("args")]
        public List<object> args;
    }

    [System.Serializable]
    public class TiledVAEWrapper
    {
        [JsonProperty("args")]
        public List<object> args;
    }

    [System.Serializable]
    public class AlwaysonScripts
    {
        [JsonProperty("Tiled Diffusion", NullValueHandling = NullValueHandling.Ignore)]
        public MultiDiffusionWrapper TiledDiffusion;

        [JsonProperty("Tiled VAE", NullValueHandling = NullValueHandling.Ignore)]
        public TiledVAEWrapper TiledVAE;

        [JsonProperty("ControlNet", NullValueHandling = NullValueHandling.Ignore)]
        public ControlNetWrapper ControlNet;

        [JsonProperty("ADetailer", NullValueHandling = NullValueHandling.Ignore)]
        public ADetailerWrapper ADetailer;
    }
    [System.Serializable]
    public class ControlNetWrapper
    {
        [JsonProperty("args")]
        public List<object> args;
    }
    [System.Serializable]
    public class ADetailerWrapper
    {
        [JsonProperty("args")]
        public List<object> args;
    }
    [System.Serializable]
    public class Txt2ImgRequest
    {
        public string prompt = "";
        public string negative_prompt = "";
        public string sampler_name = "";
        public string scheduler = "";
        public int steps = 20;
        public int width = 512;
        public int height = 512;
        //public int n_iter = 2;
        public int batch_size = 1;
        public bool enable_hr = false;
        public bool restore_faces = false;
        public bool tiling = false;
        public int seed = 1;

        [JsonProperty("alwayson_scripts", NullValueHandling = NullValueHandling.Ignore)]
        public AlwaysonScripts alwayson_scripts;
        internal Dictionary<string, object> override_settings;
    }

    [System.Serializable]
    public class Txt2ImgResponse
    {
        public List<string> images;
    }

    public List<Region> AllRegions;
    public void InputRegion(string BlendMode, float X, float Y, float W, float H,string Prompt,string Neg_Prompt)
    {
        AllRegions.Add(new Region
        {
            x = X,
            y = Y,
            w = W,
            h = H,
            prompt = Prompt,
            negative_prompt = Neg_Prompt,
            blendMode = (BlendMode == "Background")? "Background" : "Foreground",
            feather = (BlendMode == "Foreground") ? 0.15f : 0.35f
        });
    }
    void Start()
    {
        // 初始化 List
        AllRegions = new List<Region>();
        allScores = new int[4];
        //string LoRa = LoRaType[UnityEngine.Random.Range(0, LoRaType.Length)];
        //StartCoroutine(HandlePromptAndGenerateImage(LoRa, "anime_cute.safetensors","LoRa"));
    }
    string Image2base64(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename);
        byte[] imageBytes = File.ReadAllBytes(path);
        Texture2D controlImage = new Texture2D(2, 2);
        controlImage.LoadImage(imageBytes);
        string base64Image = Convert.ToBase64String(controlImage.EncodeToPNG());
        return base64Image;
    }
    public IEnumerator HandlePromptAndGenerateImage(string LoRa,string checkpoint,string type)
    {
        int seed = UnityEngine.Random.Range(0, 50);
        // 等待 ReadFileAndSendPrompt 完成
        yield return StartCoroutine(ReadFileAndSendPrompt(LoRa));

        yield return StartCoroutine(ReadFileAndSendImformation());
        // 然後執行 GenerateImageForMultipleChoice
        if(type!= "Controlnet")
        {
            yield return StartCoroutine(GenerateImageForMultipleChoice(768, 768, Prompt, checkpoint, LoRa, ControlNetType, "", ControlnetImageBase64, seed,
                   texture =>
                   {
                       img1 = texture;
                   }));
        }
       
        switch (type) {
            case "LoRa":

                switch (LoRa)
                {
                    case "漢服":
                        tempAnswer = "Hanfu";
                        break;
                    case "漫畫":
                        tempAnswer = "Lineart";
                        break;
                    case "貓":
                        tempAnswer = "Cutecat";
                        break;
                    case "水墨":
                        tempAnswer = "MoXin";
                        break;
                    case "盒玩":
                        tempAnswer = "Blindbox";
                        break;
                    case "吉普利":
                        tempAnswer = "Ghibli";
                        break;
                    case "眼睛":
                        tempAnswer = "Eye";
                        break;
                    case "食物照片":
                        tempAnswer = "Foodphoto";
                        break;
                    default:
                        break;
                }
                LoRa = "";
                yield return StartCoroutine(GenerateImageForMultipleChoice(768, 768, Prompt, checkpoint, LoRa, ControlNetType, "", ControlnetImageBase64, seed,
                        texture =>
                        {
                            img2 = texture;
                        }));
                break;
            case "Checkpoint":
                switch (checkpoint)
                {
                    case "anime_cute.safetensors":
                        tempAnswer = "CuteYukiMix";
                        break;
                    case "anime-real_hybrid.safetensors":
                        tempAnswer = "ReV Animated";
                        break;
                    case "anime_soft.safetensors":
                        tempAnswer = "Cetus-Mix";
                        break;
                    case "realistic_anything.safetensors":
                        tempAnswer = "DreamShaper";
                        break;
                    case "anime_bold.safetensors":
                        tempAnswer = "Counterfeit";
                        break;
                    default:
                        break;
                }
                yield return StartCoroutine(GenerateImageForMultipleChoice(768, 768, Prompt, "v1-5-pruned-emaonly.safetensors [6ce0161689]", LoRa, ControlNetType, "", ControlnetImageBase64, seed,
                        texture =>
                        {
                            img2 = texture;
                        }));
                break;
            case "Prompt":
                string[] add = new string[] { "desert", "forest", "beach", "grassland", "lake", "blizzard", "sunset", "foggy", "thunderstorm", "god rays", "downtown", "cyberpunk", "oil painting","watercolor", "japanese temple" , "castle", "classroom", "bedroom", "magic forest", "lava ground", "red", "blue", "green", "yellow", "purple", "orange", "pink", "black", "white", "gray", "brown" };
                string Addresult = add[UnityEngine.Random.Range(0, add.Length)];
                Debug.Log("新增提示詞為:" + Addresult);
                tempAnswer = Addresult;
                yield return StartCoroutine(GenerateImageForMultipleChoice(768, 768, "(" + Addresult + ":2)," + Prompt, checkpoint, LoRa, ControlNetType, "", ControlnetImageBase64, seed,
                   texture =>
                   {
                       img2 = texture;
                   }));
                break;
            case "Resolution":
                int[] resolution = new int[] { 128 ,384,1024,512};
                int randResolution=resolution[UnityEngine.Random.Range(0, resolution.Length)];
                Debug.Log("randResolution:" + randResolution);
                tempAnswer = randResolution.ToString();
                yield return StartCoroutine(GenerateImageForMultipleChoice(randResolution, randResolution, Prompt, checkpoint, LoRa, ControlNetType,"", ControlnetImageBase64, seed,
                        texture =>
                        {
                            img2 = texture;
                        }));
                break;
            case "Controlnet":
                string[] ControlnetModule = new string[] { "depth_anything_v2", "canny", "openpose_hand", "shuffle" };
                string randControlnet = ControlnetModule[UnityEngine.Random.Range(0, ControlnetModule.Length)];
                Debug.Log("randControlnet:" + randControlnet);
                ControlnetImageBase64 = GetRandomControlImageBase64("ConTrolNet參考圖/other");
                byte[] imageBytes = Convert.FromBase64String(ControlnetImageBase64);
                //Texture2D tex = new Texture2D(2, 2);
                img1 = new Texture2D(2, 2);
                img1.LoadImage(imageBytes);
                switch (randControlnet)
                {
                    case "depth_anything_v2":
                        tempAnswer = "depth";
                        break;
                    case "canny":
                        tempAnswer = "canny";
                        break;
                    case "openpose_hand":
                        tempAnswer = "openpose";
                        break;
                    case "shuffle":
                        tempAnswer = "shuffle";
                        break;
                    default:
                        break;
                }
                yield return StartCoroutine(GenerateImageForMultipleChoice(768, 768, Prompt, checkpoint, LoRa, ControlNetType, randControlnet, ControlnetImageBase64, seed,
                        texture =>
                        {
                            img2 = texture;
                        }));
                break;
            default:
                break;
        }
        
    }
    

    public bool skipWait = false;
    public void OnSkipButtonPressed()
    {
        skipWait = true;
    }
    public GameObject IntroScreen;
    public GameObject GameStartScreen;
    public GameObject GameStartButton;
    public void OnClickStartGame()
    {
        IntroScreen.SetActive(false);
        IntroScreen.SetActive(true);
    }
    public IEnumerator StartAutoImageUpdate()
    {
        bool first = true;
        while (true)
        {
            float startTime = Time.realtimeSinceStartup;
            Debug.Log("⏳ 開始生成圖片...");
            string[] result;
            if (first)
            {
                result = multiChoiceQuestion.GenerateQuestions();
                yield return StartCoroutine(HandlePromptAndGenerateImage(result[0], result[1], result[2]));
                yield return StartCoroutine(geminiAPI.SendPhotoRequest("題目會說明主體和他在做什麼，且需要在20個英文字裡說明完，且不能有標點符號，例子:[A young woman stands on a city street]，接下來我會給一張圖片，你要給我符合這個圖片的題目，請你依照{說明}回傳給我，說明要包在大括號內。", Convert.ToBase64String((((result[2] == "Prompt") || (result[2] == "Resolution")) ? img1 : img2).EncodeToPNG()), (result) =>
                {
                    Match match = Regex.Match(result, @"\{([^}]*)\}");
                    result = match.Groups[1].Value;
                    //Narrative.text = result;
                    multiChoiceQuestion.QuestionName.text = result;
                }));
                GameStartButton.SetActive(false);
                if (result[2] == "Checkpoint"|| result[2] == "LoRa")
                {
                    multiChoiceQuestion.AfterImage.sprite = Sprite.Create(img1, new Rect(0, 0, img1.width, img1.height), new Vector2(0.5f, 0.5f));
                    multiChoiceQuestion.BeforeImage.sprite = Sprite.Create(img2, new Rect(0, 0, img2.width, img2.height), new Vector2(0.5f, 0.5f));
                    //imageUI2.sprite = Sprite.Create(img1, new Rect(0, 0, img1.width, img1.height), new Vector2(0.5f, 0.5f));
                    //imageUI.sprite = Sprite.Create(img2, new Rect(0, 0, img2.width, img2.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    multiChoiceQuestion.BeforeImage.sprite = Sprite.Create(img1, new Rect(0, 0, img1.width, img1.height), new Vector2(0.5f, 0.5f));
                    multiChoiceQuestion.AfterImage.sprite = Sprite.Create(img2, new Rect(0, 0, img2.width, img2.height), new Vector2(0.5f, 0.5f));
                    //imageUI.sprite = Sprite.Create(img1, new Rect(0, 0, img1.width, img1.height), new Vector2(0.5f, 0.5f));
                    //imageUI2.sprite = Sprite.Create(img2, new Rect(0, 0, img2.width, img2.height), new Vector2(0.5f, 0.5f));
                }
                gameController.answer = tempAnswer;
                multiChoiceQuestion.ChangeHintImage(result[2]);
                MainBody.Enqueue(Prompt.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                if (MainBody.Count > 5)
                {
                    MainBody.Dequeue();
                }
                first = false;
                Debug.Log("已經生成完第一組圖 ，開始生成第二組圖");
            }
            result = multiChoiceQuestion.GenerateQuestions();
            yield return StartCoroutine(HandlePromptAndGenerateImage(result[0], result[1], result[2]));
            string temp = "";
            yield return StartCoroutine(geminiAPI.SendPhotoRequest("題目會說明主體和他在做什麼，且需要在20個英文字裡說明完，且不能有標點符號，例子:[A young woman stands on a city street]，接下來我會給一張圖片，你要給我符合這個圖片的題目，請你依照{說明}回傳給我，說明要包在大括號內。", Convert.ToBase64String((((result[2] == "Prompt") || (result[2] == "Resolution")) ? img1 : img2).EncodeToPNG()), (result) =>
            {
                Match match = Regex.Match(result, @"\{([^}]*)\}");
                result = match.Groups[1].Value;
                //Narrative.text = result;
                temp = result;
            }));
            SkipButton.SetActive(true);
            skipWait = false;
            MainBody.Enqueue(Prompt.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0]);
            if (MainBody.Count > 5)
            {
                MainBody.Dequeue();
            }
            yield return new WaitUntil(() => skipWait);
            SkipButton.SetActive(false);
            if (!first)
            {
                multiChoiceQuestion.QuestionName.text = temp;
                multiChoiceQuestion.ResetButtonColor();
                if (result[2] == "Checkpoint" || result[2] == "LoRa")
                {
                    multiChoiceQuestion.AfterImage.sprite = Sprite.Create(img1, new Rect(0, 0, img1.width, img1.height), new Vector2(0.5f, 0.5f));
                    multiChoiceQuestion.BeforeImage.sprite = Sprite.Create(img2, new Rect(0, 0, img2.width, img2.height), new Vector2(0.5f, 0.5f));
                    //imageUI2.sprite = Sprite.Create(img1, new Rect(0, 0, img1.width, img1.height), new Vector2(0.5f, 0.5f));
                    //imageUI.sprite = Sprite.Create(img2, new Rect(0, 0, img2.width, img2.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    multiChoiceQuestion.BeforeImage.sprite = Sprite.Create(img1, new Rect(0, 0, img1.width, img1.height), new Vector2(0.5f, 0.5f));
                    multiChoiceQuestion.AfterImage.sprite = Sprite.Create(img2, new Rect(0, 0, img2.width, img2.height), new Vector2(0.5f, 0.5f));
                    //imageUI.sprite = Sprite.Create(img1, new Rect(0, 0, img1.width, img1.height), new Vector2(0.5f, 0.5f));
                    //imageUI2.sprite = Sprite.Create(img2, new Rect(0, 0, img2.width, img2.height), new Vector2(0.5f, 0.5f));
                }
                gameController.answer = tempAnswer;
                multiChoiceQuestion.ChangeHintImage(result[2]);
            }
        }
    }
    List<object> BuildFullArgs(List<Region> regions)
    {
        var args = new List<object>
        {
            true,                            // enable
            "Mixture of Diffusers",         // method
            true,                            // overwrite image size
            true,                            // keep input size
            1024,                            // image width
            1024,                            // image height
            96,                              // latent tile width
            96,                              // latent tile height
            48,                              // latent tile overlap
            4,                               // latent tile batch size
            "None",                              // ⛔ upscaler（空字串關閉）
            2,                               // ⛔ scale factor（設成 1）
            false,                           // ⛔ enable noise inversion
            10,                               // ⛔ inversion steps
            1,                               // ⛔ retouch
            1,                               // ⛔ renoise strength
            64,                               // ⛔ renoise kernel size
            false,                           // ⛔ move ControlNet tensor to CPU
            true,                           // ⛔ enable control
            false,                           // ⛔ draw full canvas background
            false                            // ⛔ causalize layers
        };

        int regionCount = Mathf.Min(8, regions.Count);
        for (int i = 0; i < regionCount; i++)
        {
            Region r = regions[i];
            args.Add(true);                 // enabled
            args.Add(r.x);
            args.Add(r.y);
            args.Add(r.w);
            args.Add(r.h);
            args.Add(r.prompt);
            args.Add(r.negative_prompt);
            string blendMode = r.blendMode ?? "Background"; // 預設為 Background
            args.Add(blendMode); 
            if (blendMode == "Foreground")
            {
                args.Add(r.feather);       // ✅ feather 只加在 Foreground
            }
            else
            {
                args.Add(0.2f);
            }
            //args.Add(0);                   // mask blur
            args.Add(-1);                  // seed
            
        }

        // 補足至 8 個 Regions
        for (int i = regionCount; i < 8; i++)
        {
            args.Add(false); args.Add(0); args.Add(0); args.Add(0); args.Add(0);
            args.Add(""); args.Add(""); args.Add("Background"); args.Add(0); args.Add(-1);
        }

        return args;
    }
    List<object> BuildTiledVAEArgs()
    {
        return new List<object>
        {
            true,   // Enable Tiled VAE
            true,   // Move VAE to GPU
            960,    // Encoder Tile Size
            64,     // Decoder Tile Size
            true,   // Fast Encoder
            false,  // Fast Encoder Color Fix
            true    // Fast Decoder
        };
    }
    public IEnumerator GenerateImageForMultipleChoice(int Width,int Height,string prompt,string Model_checkpoint,string Lora_Name, string controlNetType,string controlnetModule, string base64Image,int seed, System.Action<Texture2D> callback)
    {
        string url = "http://127.0.0.1:7860/sdapi/v1/txt2img";

        string LoraPrompt = "";
        //Canny/Depth/Openpose/Shuffle
        string imageData = "data:image/png;base64," + base64Image;

        string modelString = "";
        string moduleString = (controlnetModule == "")?"none": controlnetModule;
        string[] CheckpointType = {};
        switch (controlnetModule)
        {
            case "canny":
                modelString = "control_v11p_sd15_canny [d14c016b]";
                break;
            case "depth_anything_v2":
                modelString = "control_v11f1p_sd15_depth [cfd03158]";
                break;
            case "openpose_hand":
                modelString = "control_v11p_sd15_openpose [cab727d4]";
                break;
            case "shuffle":
                modelString = "control_v11e_sd15_shuffle [526bfdae]";
                break;
            default:
                modelString = "none";
                break;
        }
        if (modelString == "none") {
            switch (controlNetType)
            {
                case "Canny":
                    modelString = "control_v11p_sd15_canny [d14c016b]";
                    break;
                case "Depth":
                    modelString = "control_v11f1p_sd15_depth [cfd03158]";
                    break;
                case "Openpose":
                    modelString = "control_v11p_sd15_openpose [cab727d4]";
                    break;
                case "Shuffle":
                    modelString = "control_v11e_sd15_shuffle [526bfdae]";
                    break;
                default:
                    modelString = "";
                    break;
            }
        }
        
        
        int rand;
        switch (Lora_Name)
        {
            case "漢服":
                LoraPrompt = ",<lora:hanfu40-beta-3:0.6>";
                modelString = "control_v11f1p_sd15_depth [cfd03158]";
                if (infoArray[2] == "girl")
                {
                    imageData = "data:image/png;base64," + Image2base64("ConTrolNet參考圖/female_depth/girl/no hand/1.png");
                }
                else
                {
                    rand = UnityEngine.Random.Range(1, 4); // 1~3
                    string path = $"ConTrolNet參考圖/female_depth/woman/no hand/{rand}.png";
                    imageData = "data:image/png;base64," + Image2base64(path);
                }

                HanfuimageData = imageData;
                break;
            case "漫畫":
                LoraPrompt = ",lineart, ((monochrome)),<lora:animeoutlineV4_16:1.3>";
                break;
            case "貓":
                LoraPrompt = ",<lora:cat_20230627113759:0.7>";
                break;
            case "水墨":
                LoraPrompt = ",<lora:3Guofeng3_v34:0.85> <lora:shuV2:0.9>";
                break;
            case "盒玩":
                modelString = "";
                LoraPrompt = ",<lora:blindbox_v1_mix:1>";
                CheckpointType = new string[] { "anime-real_hybrid", "anime_soft" };
                Model_checkpoint = CheckpointType[UnityEngine.Random.Range(0, CheckpointType.Length)];
                break;
            case "吉普利":
                LoraPrompt = ",<lora:ghibli_style_offset:1>";
                CheckpointType = new string[] { "anime_cute", "anime-real_hybrid", "anime_soft" };
                Model_checkpoint = CheckpointType[UnityEngine.Random.Range(0, CheckpointType.Length)];
                break;
            case "眼睛":
                LoraPrompt = ",<lora:Loraeyes_V1:0.8>";
                break;
            case "食物照片":
                LoraPrompt = ",<lora:foodphoto:0.6>";
                CheckpointType = new string[] { "anime_cute", "realistic_anything", "anime_soft" };
                Model_checkpoint = CheckpointType[UnityEngine.Random.Range(0, CheckpointType.Length)];
                break;
            case "漢服對比圖":
                modelString = "control_v11f1p_sd15_depth [cfd03158]";
                imageData = HanfuimageData;
                break;
            default:
                break;
        }
        yield return StartCoroutine(ChangeCheckpoint(Model_checkpoint));
        
        Debug.Log("是否使用controlNet:"+controlNetType);
        var controlnetArgs = new List<object>
        {
            new Dictionary<string, object>
            {
                { "enabled", (modelString == ""||infoArray[0] == "no")?false:true },
                { "model", modelString },
                { "module", moduleString },
                { "weight", 1.0f },
                { "guidance_start", 0.0f },
                { "guidance_end", 1.0f },
                { "control_mode", "Balanced" },
                { "pixel_perfect", true },
                { "resize_mode", "Resize and Fill" },
                { "image", new Dictionary<string, object>
                    {
                        { "image", imageData },
                        //{ "mask", null }
                    }
                }
            }
        };
        var adetailerArgs = new List<object>
        {
            (infoArray[0] == "no")?false:true,   // enabled
            false,  // disable second pass
            new Dictionary<string, object>
            {
                { "ad_model", "face_yolov8n_v2.pt" },
                { "ad_prompt", "detail face," + ((infoArray[5] == "none") ? "":infoArray[5])+((infoArray[4] == "old") ? ",wrinkle":"")}
            }/*,
            new Dictionary<string, object>
            {
                { "ad_model", "hand_yolov8n.pt" }
            }*/
        };
        Debug.Log("ADetaile:" + infoArray[5]);
        var requestData = new Txt2ImgRequest
        {
            steps = 20,
            width = Width,
            height = Height,
            sampler_name = Sampler_name,
            scheduler = Scheduler,
            enable_hr = false,
            restore_faces = false,
            tiling = false,
            seed = seed,
            prompt = prompt+ LoraPrompt+ ", BREAK, (masterpiece:1.2),  best quality, highres, highly detailed, perfect lighting , <lora:add_detail:0.5> " + ((infoArray[4] == "old") ? ",(AS-Elderly:1.5)" : ""),
            negative_prompt = (Lora_Name == "漫畫") ? "easynegative, (badhandv4:1.2), NSFW, watermark jpeg artifacts signature watermark username blurry" : "easynegative, (badhandv4:1.2), NSFW, (worst quality:2), (low quality:2), (normal quality:2), lowres, normal quality, ((monochrome)), ((grayscale)), skin spots, acnes, skin blemishes, age spot, (ugly:1.331), (duplicate:1.331), watermark jpeg artifacts signature watermark username blurry, Stable_Yogis_SD1.5_Negatives-neg",
            override_settings = new Dictionary<string, object>
            {
                { "sd_model_checkpoint", Model_checkpoint },
                { "CLIP_stop_at_last_layers", 2 }
            }, 
            alwayson_scripts = new AlwaysonScripts
            {
                ControlNet = new ControlNetWrapper{args = controlnetArgs},
                ADetailer = new ADetailerWrapper { args = adetailerArgs }
            }

        };


        string jsonData = JsonConvert.SerializeObject(requestData);
        Debug.Log(jsonData);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Request failed: " + request.error);
            Debug.LogError(request.downloadHandler.text); // 看錯誤訊息詳細原因
        }
        else
        {
            Txt2ImgResponse response = JsonConvert.DeserializeObject<Txt2ImgResponse>(request.downloadHandler.text);
            if (response.images != null)
            {
                byte[] imageBytes = System.Convert.FromBase64String(response.images[0]);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                Debug.Log("✅ 圖片成功產生（Region Prompt Control）");

                callback?.Invoke(texture);
            }
        }
    }
    public IEnumerator GenerateImageWithRegions(System.Action<Texture2D> callback)
    {
        string url = "http://127.0.0.1:7860/sdapi/v1/txt2img";


        var requestData = new Txt2ImgRequest
        {
            steps = 1,
            width = 512,
            height = 512,
            sampler_name = Sampler_name,
            scheduler = Scheduler,
            enable_hr = false,
            restore_faces = false,
            tiling = false,
            prompt = "masterpiece,  best quality, ultra high reslotion, highly detailed",
            negative_prompt = "(worst quality:2), (low quality:2), (normal quality:2), lowers, ((monochrome)), ((grayscale)), watermark",
            override_settings = new Dictionary<string, object>
            {
                { "sd_model_checkpoint", "counterfeitV30_v30.safetensors" },
                { "CLIP_stop_at_last_layers", 2 }
            },
            alwayson_scripts = new AlwaysonScripts
            {
                TiledDiffusion = new MultiDiffusionWrapper
                {
                    args = BuildFullArgs(AllRegions)
                },
                TiledVAE = new TiledVAEWrapper
                {
                    args = BuildTiledVAEArgs()
                }
            }
        };


        string jsonData = JsonConvert.SerializeObject(requestData);
        Debug.Log(jsonData);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Request failed: " + request.error);
            Debug.LogError(request.downloadHandler.text); // 看錯誤訊息詳細原因
        }
        else
        {
            Txt2ImgResponse response = JsonConvert.DeserializeObject<Txt2ImgResponse>(request.downloadHandler.text);
            if (response.images != null && response.images.Count > 0)
            {

                Debug.Log("總共生成了"+response.images.Count+"張");
                yield return StartCoroutine(ReadScoreFileAndSend(response.images[0], 0));
                yield return StartCoroutine(ReadScoreFileAndSend(response.images[1], 1));
                yield return StartCoroutine(ReadScoreFileAndSend(response.images[2], 2));
                int maxIndex = 0;
                int maxValue = allScores[0];
                for (int i = 1; i < 3; i++)
                {
                    if (allScores[i] > maxValue)
                    {
                        maxValue = allScores[i];
                        maxIndex = i;
                    }
                }
                Debug.Log("圖片" + (maxIndex + 1) + "分數最高，為" + maxValue + "分，所以將上傳第" + (maxIndex + 1) + "張圖");
                byte[] imageBytes = System.Convert.FromBase64String(response.images[maxIndex]);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                Debug.Log("✅ 圖片成功產生（Region Prompt Control）");

                callback?.Invoke(texture);
            }
        }
    }
    private int[] allScores;
    IEnumerator ReadScoreFileAndSend(string base64Image,int num)
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "評分階段.txt");

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
        yield return StartCoroutine(geminiAPI.SendPhotoRequest(fileContent, base64Image, (result) =>
        {
            // 將 Base64 字串轉為 byte 陣列
            byte[] imageBytesForDownload = Convert.FromBase64String(base64Image);

            // 建立 Texture2D 並載入圖片
            Texture2D textureForDownload = new Texture2D(2, 2);
            textureForDownload.LoadImage(imageBytesForDownload);

            // 將圖片轉為 PNG 格式的 byte[]
            byte[] pngData = textureForDownload.EncodeToPNG();
            // 選擇儲存路徑（例如：Application.persistentDataPath）
            string path = Path.Combine(Application.persistentDataPath, $"DownloadedImage{num}.png");

            // 寫入檔案
            System.IO.File.WriteAllBytes(path, pngData);

            Debug.Log("圖片已儲存至: " + path);
            int score = ContentsSeparated(result);
            Debug.Log(score);
            allScores[num] = score;
        }));
    }
    int ContentsSeparated(string text)
    {
        List<string> allItems = new List<string>();
        MatchCollection matchCollection = Regex.Matches(text, @"\((.*?)\)");
        int score = 0;
        foreach (Match match in matchCollection)
        {
            string insideBraces = match.Groups[1].Value;
            // 用 '-' 分割
            string[] parts = insideBraces.Split('/');

            foreach (var part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    string trimmed = part.Trim();
                    allItems.Add(trimmed);

                    // 嘗試轉換成 int 並加總
                    if (int.TryParse(trimmed, out int number))
                    {
                        score += number;
                    }
                }
            }
        }

        return score;
    }

    IEnumerator ReadFileAndSendPrompt(string LoRa_name)
    {
        string path;
        if(LoRa_name == "貓")
            path = System.IO.Path.Combine(Application.streamingAssetsPath, "選擇題提示詞For貓.txt");
        else if (LoRa_name == "食物")
            path = System.IO.Path.Combine(Application.streamingAssetsPath, "選擇題提示詞For食物.txt");
        else
            path = System.IO.Path.Combine(Application.streamingAssetsPath, "選擇題提示詞.txt");
        string AddLLM = "";

        string MainBodyLLM = string.Join(",", MainBody);

        Debug.Log("目前有的主體:" + MainBodyLLM);
        switch (LoRa_name)
        {
            case "漢服":
                AddLLM = "主角是一個女生，提示詞需要包含hanfu, (朝代) style，其他由你自由發揮";
                break;
            case "漫畫":
                AddLLM = "題目由你來決定";
                break;
            case "貓":
                AddLLM = "主角是一個貓，提示詞需要包含cat，其他由你自由發揮";
                break;
            case "水墨":
                AddLLM = "主體前需要加potrait of，提示詞需要包含traditional chinese ink painting, 大師名, shukezouma，其他由你自由發揮 ";
                break;
            case "盒玩":
                AddLLM = "主角是一個女生或男生，提示詞需要包含full body, chibi，提示詞只能描述主體，不用描述環境";
                break;
            case "吉普利":
                AddLLM = "題目由你來決定";
                break;
            case "眼睛":
                AddLLM = "主角是一個眼睛，提示詞需要包含loraeyes，其他由你自由發揮";
                break;
            case "食物照片":
                AddLLM = "主角是一個食物(禁止壽司)，提示詞需要包含foodphoto，可以使用攝影細節，其他由你自由發揮";
                break;
            case "Controlnet":
                AddLLM = "主角是一個人，其他由你自由發揮";
                break;
            default:
                AddLLM = "題目由你來決定" + "，不能出現的主體有:" + MainBodyLLM;
                break;
        }
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

        //Debug.Log(fileContent+ AddLLM);
        // 呼叫 Gemini API 並傳入檔案內容
        yield return StartCoroutine(geminiAPI.SendRequest(fileContent+ AddLLM, (result) =>
        {
            //string prompt = result.Split(new string[] { "PROMPT={" }, StringSplitOptions.None)[1].TrimEnd('}');
            Match match = Regex.Match(result, @"\{([^}]*)\}");
            Prompt = match.Groups[1].Value;
            //Debug.Log("取出的提示詞為:"+prompt.Groups[1].Value);
        }));
    }
    IEnumerator ReadFileAndSendImformation()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "提取資訊.txt");
        
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

        //Debug.Log(fileContent+ AddLLM);
        // 呼叫 Gemini API 並傳入檔案內容
        yield return StartCoroutine(geminiAPI.SendRequest(fileContent+Prompt , (result) =>
        {
            Match match = Regex.Match(result, @"\{([^}]*)\}");
            if (match.Success)
            {
                string raw = match.Groups[1].Value; // 取得大括號內的內容: yes,woman,girl,stand,no,smiling
                infoArray = raw.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                // ✅ 範例：列出陣列內容
                foreach (string item in infoArray)
                {
                    Debug.Log("陣列元素: " + item.Trim());
                }
                infoArray[2] = infoArray[2].Trim().ToLower();
                infoArray[3] = infoArray[3].Trim().ToLower();
                switch (infoArray[1].Trim().ToLower())
                {
                    case "man":
                        ControlNetType = "Openpose";
                        if (infoArray[3] == "stand" || infoArray[3] == "sit" || infoArray[3] == "run" || infoArray[3] == "jump")
                            ControlnetImageBase64 = GetRandomControlImageBase64("ConTrolNet參考圖/openpose/"+ infoArray[3]);
                        else
                            ControlnetImageBase64 = GetRandomControlImageBase64("ConTrolNet參考圖/openpose/stand");
                        Debug.Log("ControlnetImageBase64:"+ControlnetImageBase64);
                        break;
                    case "woman":
                        ControlNetType = "Depth";
                        if (infoArray[2]=="woman")
                            if (infoArray[3] == "stand" || infoArray[3] == "sit" || infoArray[3] == "run"  || infoArray[3] == "jump")
                                ControlnetImageBase64 = GetRandomControlImageBase64("ConTrolNet參考圖/female_depth/woman/" + infoArray[3]);
                            else
                                ControlnetImageBase64 = GetRandomControlImageBase64("ConTrolNet參考圖/female_depth/woman/stand");
                        else
                            if (infoArray[3] == "stand" || infoArray[3] == "sit" || infoArray[3] == "run" || infoArray[3] == "jump")
                                ControlnetImageBase64 = GetRandomControlImageBase64("ConTrolNet參考圖/female_depth/girl/" + infoArray[3]);
                            else
                                ControlnetImageBase64 = GetRandomControlImageBase64("ConTrolNet參考圖/female_depth/girl/stand");
                        Debug.Log("ControlnetImageBase64:" + ControlnetImageBase64);
                        break;
                    default:
                        ControlNetType = "";
                        ControlnetImageBase64 = "";
                        Debug.Log("ControlnetImageBase64:" + ControlnetImageBase64);
                        break;
                }
            }
            else
            {
                Debug.LogWarning("⚠️ 無法從回傳結果中找到 { ... } 格式內容！");
            }
        }));
    }
    public IEnumerator ChangeCheckpoint(string modelCheckpoint)
    {
        string url = "http://127.0.0.1:7860/sdapi/v1/options";

        // 建立 payload，符合 API 格式
        Dictionary<string, string> payload = new Dictionary<string, string>()
        {
            { "sd_model_checkpoint", modelCheckpoint }
        };

        string jsonData = JsonConvert.SerializeObject(payload);
        Debug.Log("要傳送的資料：" + jsonData);

        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Checkpoint 切換失敗: " + request.error);
            Debug.LogError(request.downloadHandler.text);
        }
        else
        {
            Debug.Log("✅ Checkpoint 已成功切換為: " + modelCheckpoint);
            Debug.Log("API 回應：" + request.downloadHandler.text);
        }
    }
    public string GetRandomControlImageBase64(string ConTrolNetPath)
    {
        string baseFolder = Path.Combine(Application.streamingAssetsPath, ConTrolNetPath);

        

        // 隨機圖片
        string[] imageFiles = Directory.GetFiles(baseFolder, "*.*")
            .Where(f => f.EndsWith(".png") || f.EndsWith(".jpg") || f.EndsWith(".jpeg"))
            .ToArray();

        if (imageFiles.Length == 0)
        {
            Debug.LogWarning("❌ 該資料夾中沒有圖片：" + baseFolder);
            return null;
        }

        string randomImagePath = imageFiles[UnityEngine.Random.Range(0, imageFiles.Length)];

        // 轉換為 StreamingAssets 相對路徑給你的方法
        string relativePath = randomImagePath.Replace(Application.streamingAssetsPath + Path.DirectorySeparatorChar, "");

        return Image2base64(relativePath);
    }


    public Dictionary<string, string[]> ResultLLM = new Dictionary<string, string[]>
    {
        // ---------------------模型---------------------
        ["ReV Animated"] = new string[]
        {
            "ReV Animated這個模型生成的圖片風格會介於動畫和現實之間，現在我會給你一張由ReV Animated生成的圖片，請解釋為什麼這張圖片是由ReV Animated這個模型生成的，解釋的長度限制在50個字以內，解釋用[因此可以判斷這張圖是用ReV Animated生成的]來做結尾",
            "ReV Animated這個模型生成的圖片風格會介於動畫和現實之間，現在我會給你一張不是由ReV Animated生成的圖片，請解釋為什麼這張圖片不是由ReV Animated這個模型生成的，解釋的長度限制在50個字以內，解釋用[因此可以判斷這張圖不是用ReV Animated生成的]來做結尾"
        },

        ["CuteYukiMix"] = new string[]
        {
            "CuteYukiMix這個模型生成的圖片是動畫風格，且生成的圖片會很可愛，現在我會給你一張由CuteYukiMix生成的圖片，請解釋為什麼這張圖片是由CuteYukiMix這個模型生成的，解釋的長度限制在50個字以內，解釋用[因此可以判斷這張圖是用CuteYukiMix生成的]來做結尾",
            "CuteYukiMix這個模型生成的圖片風格會很可愛，現在我會給你一張不是由CuteYukiMix生成的圖片，請解釋為什麼這張圖片不是由CuteYukiMix這個模型生成的，解釋的長度限制在50個字以內，解釋用[因此可以判斷這張圖不是用CuteYukiMix生成的]來做結尾"
        },

        ["Counterfeit"] = new string[]
        {
            "Counterfeit這個模型生成的圖片是動畫風格，生成的圖片會線條明顯，色彩鮮豔且對比度高，現在我會給你一張由Counterfeit生成的圖片，請解釋為什麼這張圖片是由Counterfeit這個模型生成的，解釋的長度限制在50個字以內，解釋用[因此可以判斷這張圖是用Counterfeit生成的]來做結尾",
            "Counterfeit這個模型生成的圖片是動畫風格，生成的圖片會線條明顯，色彩鮮豔，現在我會給你一張不是由Counterfeit生成的圖片，請解釋為什麼這張圖片不是由Counterfeit這個模型生成的，解釋的長度限制在50個字以內，解釋用[因此可以判斷這張圖不是用Counterfeit生成的]來做結尾"
        },

        ["Cetus-Mix"] = new string[]
        {
            "Cetus-Mix這個模型生成的圖片是動畫風格，生成的圖片會較柔和、細緻，現在我會給你一張由Cetus-Mix生成的圖片，請解釋為什麼這張圖片是由Cetus-Mix這個模型生成的，解釋的長度限制在50個字以內，解釋用[因此可以判斷這張圖是用Cetus-Mix生成的]來做結尾",
            "Cetus-Mix這個模型生成的圖片是動畫風格，生成的圖片會較柔和、細緻，現在我會給你一張不是由Cetus-Mix生成的圖片，請解釋為什麼這張圖片不是由Cetus-Mix這個模型生成的，解釋的長度限制在50個字以內，解釋用[因此可以判斷這張圖不是用Cetus-Mix生成的]來做結尾"
        },

        ["DreamShaper"] = new string[]
        {
            "DreamShaper這個模型生成的圖片是現實風格，現在我會給你一張由DreamShaper生成的圖片，請解釋為什麼這張圖片是由DreamShaper這個模型生成的，解釋的長度限制在50個字以內，解釋用[因此可以判斷這張圖是用DreamShaper生成的]來做結尾",
            "DreamShaper這個模型生成的圖片是現實風格，現在我會給你一張不是由DreamShaper生成的圖片，請解釋為什麼這張圖片不是由DreamShaper這個模型生成的，解釋的長度限制在50個字以內，解釋用[因此可以判斷這張圖不是用DreamShaper生成的]來做結尾"
        },

        // ---------------------LoRa---------------------
        ["Hanfu"] = new string[]
        {
            "Hanfu LoRa在做的事是讓模型生成的漢服更加精細且更符合歷史中的漢服，現在我會給你兩張圖片，分別是沒用Hanfu LoRa微調和有用Hanfu LoRa微調後的結果，請解釋如何判斷出這張經過Hanfu LoRa的微調，解釋的時候用原圖和經過微調後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這是經過Hanfu LoRa微調後的結果]來做結尾",
            "Hanfu LoRa在做的事是讓模型生成的漢服更加精細且更符合歷史中的漢服，現在我會給你一張沒透過Hanfu LoRa微調的圖片，請解釋如何判斷出這張沒經過Hanfu LoRa的微調，解釋的長度限制在50個字以內，解釋用[因此可以判斷這並不是經過Hanfu LoRa微調後的結果]來做結尾"
        },

        ["Cutecat"] = new string[]
        {
            "Cutecat LoRa在做的事是讓模型生成的貓更正常且更可愛，現在我會給你兩張圖片，分別是沒用Cutecat LoRa微調和有用Cutecat LoRa微調後的結果，請解釋如何判斷出這張經過Cutecat LoRa的微調，解釋的時候用原圖和經過微調後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這是經過Cutecat LoRa微調後的結果]來做結尾",
            "Cutecat LoRa在做的事是讓模型生成的貓更正常且更可愛，現在我會給你一張沒透過Cutecat LoRa微調的圖片，請解釋如何判斷出這張沒經過Cutecat LoRa的微調，解釋的長度限制在50個字以內，解釋用[因此可以判斷這並不是經過Cutecat LoRa微調後的結果]來做結尾"
        },

        ["Blindbox"] = new string[]
        {
            "Blindbox LoRa在做的事是讓模型生成的人物變成Q版盲盒娃娃，現在我會給你兩張圖片，分別是沒用Blindbox LoRa微調和有用Blindbox LoRa微調後的結果，請解釋如何判斷出這張經過Blindbox LoRa的微調，解釋的時候用原圖和經過微調後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這是經過Blindbox LoRa微調後的結果]來做結尾",
            "Blindbox LoRa在做的事是讓模型生成的人物變成Q版盲盒娃娃，現在我會給你一張沒透過Blindbox LoRa微調的圖片，請解釋如何判斷出這張沒經過Blindbox LoRa的微調，解釋的長度限制在50個字以內，解釋用[因此可以判斷這並不是經過Blindbox LoRa微調後的結果]來做結尾"
        },

        ["Eye"] = new string[]
        {
            "Eye LoRa在做的事是讓模型生成的圖片變成重點描繪眼睛，讓整張圖片都是眼睛，現在我會給你兩張圖片，分別是沒用Eye LoRa微調和有用Eye LoRa微調後的結果，請解釋如何判斷出這張經過Eye LoRa的微調，解釋的時候用原圖和經過微調後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這是經過Eye LoRa微調後的結果]來做結尾",
            "Eye LoRa在做的事是讓模型生成的圖片變成重點描繪眼睛，讓整張圖片都是眼睛，現在我會給你一張沒透過Eye LoRa微調的圖片，請解釋如何判斷出這張沒經過Eye LoRa的微調，解釋的長度限制在50個字以內，解釋用[因此可以判斷這並不是經過Eye LoRa微調後的結果]來做結尾"
        },

        ["MoXin"] = new string[]
        {
            "MoXin LoRa在做的事是讓模型生成的圖片變成中國水墨畫風格，現在我會給你兩張圖片，分別是沒用MoXin LoRa微調和有用MoXin LoRa微調後的結果，請解釋如何判斷出這張經過MoXin LoRa的微調，解釋的時候用原圖和經過微調後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這是經過MoXin LoRa微調後的結果]來做結尾",
            "MoXin LoRa在做的事是讓模型生成的圖片變成中國水墨畫風格，現在我會給你一張沒透過MoXin LoRa微調的圖片，請解釋如何判斷出這張沒經過MoXin LoRa的微調，解釋的長度限制在50個字以內，解釋用[因此可以判斷這並不是經過MoXin LoRa微調後的結果]來做結尾"
        },

        ["Ghibli"] = new string[]
        {
            "Ghibli LoRa在做的事是讓模型生成的圖片變成吉卜力動畫風格，現在我會給你兩張圖片，分別是沒用Ghibli LoRa微調和有用Ghibli LoRa微調後的結果，請解釋如何判斷出這張經過Ghibli LoRa的微調，解釋的時候用原圖和經過微調後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這是經過Ghibli LoRa微調後的結果]來做結尾",
            "Ghibli LoRa在做的事是讓模型生成的圖片變成吉卜力動畫風格，現在我會給你一張沒透過Ghibli LoRa微調的圖片，請解釋如何判斷出這張沒經過Ghibli LoRa的微調，解釋的長度限制在50個字以內，解釋用[因此可以判斷這並不是經過Ghibli LoRa微調後的結果]來做結尾"
        },

        ["Foodphoto"] = new string[]
        {
            "Foodphoto LoRa在做的事是讓模型生成的圖片的食物變得更精緻、細節更多、看起來更美味，現在我會給你兩張圖片，分別是沒用Foodphoto LoRa微調和有用Foodphoto LoRa微調後的結果，請解釋如何判斷出這張經過Foodphoto LoRa的微調，解釋的時候用原圖和經過微調後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這是經過Foodphoto LoRa微調後的結果]來做結尾",
            "Foodphoto LoRa在做的事是讓模型生成的圖片的食物變得更精緻、細節更多、看起來更美味，現在我會給你一張沒透過Foodphoto LoRa微調的圖片，請解釋如何判斷出這張沒經過Foodphoto LoRa的微調，解釋的長度限制在50個字以內，解釋用[因此可以判斷這並不是經過Foodphoto LoRa微調後的結果]來做結尾"
        },

        ["Lineart"] = new string[]
        {
            "Lineart LoRa在做的事是讓模型生成的圖片變成偏向黑白的漫畫風格且線條感會明顯，現在我會給你兩張圖片，分別是沒用Lineart LoRa微調和有用Lineart LoRa微調後的結果，請解釋如何判斷出這張經過Lineart LoRa的微調，解釋的時候用原圖和經過微調後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這是經過Lineart LoRa微調後的結果]來做結尾",
            "Lineart LoRa在做的事是讓模型生成的圖片變成偏向黑白的漫畫風格且線條感會明顯，現在我會給你一張沒透過Lineart LoRa微調的圖片，請解釋如何判斷出這張沒經過Lineart LoRa的微調，解釋的長度限制在50個字以內，解釋用[因此可以判斷這並不是經過Lineart LoRa微調後的結果]來做結尾"
        },

        // ---------------------ControlNet---------------------
        ["depth"] = new string[]
        {
            "我現在會告訴你4種controlnet控制模式的特色，並給你1張用來進行Depth controlnet控制的參考圖和1張經過Depth controlnet控制後的圖片，請解釋如何判斷出這張圖片經過Depth controlnet的控制，解釋的時候用參考圖和經過控制後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這是經過Depth controlnet控制後的結果]來做結尾",
            "我現在會告訴你4種controlnet控制模式的特色，並給你1張用來進行(填入正確答案) controlnet控制的參考圖和1張經過(填入正確答案) controlnet控制後的圖片，請解釋如何判斷出這張圖片不是經過Depth controlnet的控制而是經過(填入正確答案) controlnet的控制，解釋的時候用參考圖和經過控制後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這並不是經過Depth controlnet控制後的結果]來做結尾"
        },

        ["openpose"] = new string[]
        {
            "我現在會告訴你4種controlnet控制模式的特色，並給你1張用來進行Openpose controlnet控制的參考圖和1張經過Openpose controlnet控制後的圖片，請解釋如何判斷出這張圖片經過Openpose controlnet的控制，解釋的時候用參考圖和經過控制後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這是經過Openpose controlnet控制後的結果]來做結尾",
            "我現在會告訴你4種controlnet控制模式的特色，並給你1張用來進行(填入正確答案) controlnet控制的參考圖和1張經過(填入正確答案) controlnet控制後的圖片，請解釋如何判斷出這張圖片不是經過Openpose controlnet的控制而是經過(填入正確答案) controlnet的控制，解釋的時候用參考圖和經過控制後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這並不是經過Openpose controlnet控制後的結果]來做結尾"
        },

        ["canny"] = new string[]
        {
            "我現在會告訴你4種controlnet控制模式的特色，並給你1張用來進行Canny controlnet控制的參考圖和1張經過Canny controlnet控制後的圖片，請解釋如何判斷出這張圖片經過Canny controlnet的控制，解釋的時候用參考圖和經過控制後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這是經過Canny controlnet控制後的結果]來做結尾",
            "我現在會告訴你4種controlnet控制模式的特色，並給你1張用來進行(填入正確答案) controlnet控制的參考圖和1張經過(填入正確答案) controlnet控制後的圖片，請解釋如何判斷出這張圖片不是經過Canny controlnet的控制而是經過(填入正確答案) controlnet的控制，解釋的時候用參考圖和經過控制後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這並不是經過Canny controlnet控制後的結果]來做結尾"
        },

        ["shuffle"] = new string[]
        {
            "我現在會告訴你4種controlnet控制模式的特色，並給你1張用來進行Shuffle controlnet控制的參考圖和1張經過Shuffle controlnet控制後的圖片，請解釋如何判斷出這張圖片經過Shuffle controlnet的控制，解釋的時候用參考圖和經過控制後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這是經過Shuffle controlnet控制後的結果]來做結尾",
            "我現在會告訴你4種controlnet控制模式的特色，並給你1張用來進行(填入正確答案) controlnet控制的參考圖和1張經過(填入正確答案) controlnet控制後的圖片，請解釋如何判斷出這張圖片不是經過Shuffle controlnet的控制而是經過(填入正確答案) controlnet的控制，解釋的時候用參考圖和經過控制後的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這並不是經過Shuffle controlnet控制後的結果]來做結尾"
        },

        // ---------------------Resolution---------------------
        ["128"] = new string[]
        {
            "128x128解析度生成的圖片會模糊且缺少細節，現在我會給你一張128x128解析度的圖片，請解釋為什麼這張圖片的解析度為128x128，解釋的長度限制在50個字以內，解釋用[因此可以判斷圖片的解析度為128]來做結尾",
            "128x128解析度生成的圖片會模糊且缺少細節，現在我會給你一張不是128x128解析度的圖片，請解釋為什麼這張圖片的解析度不是128x128，解釋的長度限制在50個字以內，解釋用[因此可以判斷圖片的解析度不是128]來做結尾"
        },

        ["384"] = new string[]
        {
            "384x384解析度生成的圖片會缺少細節，現在我會給你一張384x384解析度的圖片，請解釋為什麼這張圖片的解析度為384x384，解釋的長度限制在50個字以內，解釋用[因此可以判斷圖片的解析度為384]來做結尾",
            "384x384解析度生成的圖片會缺少細節，現在我會給你一張不是384x384解析度的圖片，請解釋為什麼這張圖片的解析度不是384x384，解釋的長度限制在50個字以內，解釋用[因此可以判斷圖片的解析度不是384]來做結尾"
        },

        ["512"] = new string[]
        {
            "512x512解析度生成的圖片通常不會有問題，但有時會缺少圖片細節，現在我會給你一張512x512解析度的圖片，請解釋為什麼這張圖片的解析度為512x512，解釋的長度限制在50個字以內，解釋用[因此可以判斷圖片的解析度為512]來做結尾",
            "512x512解析度生成的圖片通常不會有問題，但有時會缺少圖片細節，現在我會給你一張不是512x512解析度的圖片，請解釋為什麼這張圖片的解析度不是512x512，解釋的長度限制在50個字以內，解釋用[因此可以判斷圖片的解析度不是512]來做結尾"
        },

        ["1024"] = new string[]
        {
            "1024x1024解析度生成的圖片有時會多出許多不需要的內容，現在我會給你一張1024x1024解析度的圖片，請解釋為什麼這張圖片的解析度為1024x1024，解釋的長度限制在50個字以內，解釋用[因此可以判斷圖片的解析度為1024]來做結尾",
            "1024x1024解析度生成的圖片有時會多出許多不需要的內容，現在我會給你一張不是1024x1024解析度的圖片，請解釋為什麼這張圖片的解析度不是1024x1024，解釋的長度限制在50個字以內，解釋用[因此可以判斷圖片的解析度不是1024]來做結尾"
        },

        // ---------------------提示詞---------------------
        ["Prompt"] = new string[]
        {
            "加入提示詞會影響圖片生成的結果，現在我會給你兩張圖片，分別是沒加(正確答案提示詞)和有加(正確答案提示詞)的圖片，請解釋如何判斷出這張圖片多加了(正確答案提示詞)提示詞，解釋的時候用原圖和加了提示詞的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這是加入了(正確答案提示詞)後的結果]來做結尾",
            "加入提示詞會影響圖片生成的結果，現在我會給你兩張圖片，分別是沒加(正確答案提示詞)和有加(正確答案提示詞)的圖片，請解釋如何判斷出這張圖片並不是加了(選擇詳解提示詞)提示詞，解釋的時候用原圖和加了提示詞的圖片來進行代稱(不允許用其他代稱，例第一張圖片、第二章圖片這種說法不被允許)，解釋的長度限制在50個字以內，解釋用[因此可以判斷這並是加入了(選擇詳解提示詞)後的結果]來做結尾"
        }
    };
}
