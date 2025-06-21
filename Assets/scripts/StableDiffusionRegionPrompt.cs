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

public class StableDiffusionRegionPrompt : MonoBehaviour
{
    //public RawImage imageUI;
    public Image imageUI;
    public GeminiAPI geminiAPI;
    private string Sampler_name = "Euler a";
    private string Scheduler = "Automatic";
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
        public int batch_size = 3;
        public bool enable_hr = false;
        public bool restore_faces = false;
        public bool tiling = false;

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
        StartCoroutine(GenerateImageForMultipleChoice(384,384, "girl,hanfu,ming style,<lora:hanfu40-beta-3:0.6>", "counterfeitV30_v30", "",
                        texture =>
                        {
                            // 將 Texture2D 轉為 Sprite 並灌入 UI Image
                            Sprite newSprite = Sprite.Create(
                                texture,
                                new Rect(0, 0, texture.width, texture.height),
                                new Vector2(0.5f, 0.5f)
                            );
                            imageUI.sprite = newSprite;
                        }));
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
    public IEnumerator GenerateImageForMultipleChoice(int Width,int Height,string prompt,string Model_checkpoint,string Lora_Name, System.Action<Texture2D> callback)
    {
        string url = "http://127.0.0.1:7860/sdapi/v1/txt2img";

        string LoraPrompt = "";
        switch (Lora_Name)
        {
            case "漢服":
                LoraPrompt = "<lora:hanfu40-beta-3:0.6>";
                break;
            case "漫畫":
                LoraPrompt = "<lora:animeoutlineV4_16:1.3>";
                break;
            case "貓":
                LoraPrompt = "<lora:cat_20230627113759:0.7>";
                break;
            case "水墨":
                LoraPrompt = "<lora:3Guofeng3_v34:0.85> <lora:shuV2:0.9>";
                break;
            case "盒玩": 
                LoraPrompt = "<lora:blindbox_v1_mix:1>";
                break;
            case "吉普利":
                LoraPrompt = "<lora:ghibli_style_offset:1>";
                break;
            case "眼睛":
                LoraPrompt = "<lora:Loraeyes_V1:0.8>";
                break;
            case "食物照片":
                LoraPrompt = "<lora:foodphoto:0.6>";
                break;
            default:
                break;
        }
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
            prompt = prompt+ LoraPrompt+ "<lora:add_detail:0.5>",
            negative_prompt = "(worst quality:2), (low quality:2), (normal quality:2), lowers, ((monochrome)), ((grayscale)), watermark",
            override_settings = new Dictionary<string, object>
            {
                { "sd_model_checkpoint", Model_checkpoint },
                { "CLIP_stop_at_last_layers", 2 }
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

    IEnumerator ReaFileAndSend(string TXTfile,string LoRa_name)
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, TXTfile);
        string AddLLM = "";
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
            default:
                AddLLM = "題目由你來決定";
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

        // 呼叫 Gemini API 並傳入檔案內容
        yield return StartCoroutine(geminiAPI.SendRequest(fileContent+ AddLLM, (result) =>
        {
            Debug.Log(result);
        }));
    }
}
