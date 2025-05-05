using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.UI;

public class StableDiffusionRegionPrompt : MonoBehaviour
{
    //public RawImage imageUI;
    public Image imageUI;
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
        [JsonProperty("Tiled Diffusion")]
        public MultiDiffusionWrapper TiledDiffusion;

        [JsonProperty("Tiled VAE")]
        public TiledVAEWrapper TiledVAE;
    }

    [System.Serializable]
    public class Txt2ImgRequest
    {
        public string prompt = "";
        public string negative_prompt = "";
        public int steps = 20;
        public int width = 512;
        public int height = 512;
        public bool enable_hr = false;
        public bool restore_faces = false;
        public bool tiling = false;

        [JsonProperty("alwayson_scripts")]
        public AlwaysonScripts alwayson_scripts;
    }

    [System.Serializable]
    public class Txt2ImgResponse
    {
        public List<string> images;
    }

    public List<Region> AllRegions;
    public void InputRegion(string blendMode, float X, float Y, float W, float H,string Prompt,string Neg_Prompt)
    {
        AllRegions.Add(new Region
        {
            x = X,
            y = Y,
            w = W,
            h = H,
            prompt = Prompt,
            negative_prompt = Neg_Prompt,
            blendMode = (blendMode == "Background")? "Background" : "Foreground",
            feather = 0.35f
        });
    }
    void Start()
    {
        // 初始化 List
        AllRegions = new List<Region>();

        //StartCoroutine(GenerateImageWithRegions());
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
    public IEnumerator GenerateImageWithRegions(System.Action<Texture2D> callback)
    {
        string url = "http://127.0.0.1:7860/sdapi/v1/txt2img";

        //var regions = new List<Region>
        //{
        //    new Region
        //    {
        //        x = 0,
        //        y = 0,
        //        w = 1,
        //        h = 1,
        //        prompt = "tiger",
        //        negative_prompt = "(human:3), person, people, man, (woman:3), child, children, boy, (girl:3), face, head, (human body:3), skin, hands, arms, (legs:3), eyes",
        //        blendMode="Background"
        //    },
        //    new Region
        //    {
        //        x = 0.1f,
        //        y = 0.6f,
        //        w = 0.3f,
        //        h = 0.3f,
        //        prompt = "a vending machine, old, red, slightly rusted, filled with colorful drinks, glowing interior light",
        //        negative_prompt = "(human:3), person, people, man, (woman:3), child, children, boy, (girl:3), face, head, (human body:3), skin, hands, arms, (legs:3), eyes",
        //        blendMode="Foreground",
        //        feather = 0.25f
        //    },
        //    new Region
        //    {
        //        x = 0.45f,
        //        y = 0.65f,
        //        w = 0.3f,
        //        h = 0.3f,
        //        prompt = "a suitcase, black, broken handle, wide open, clothes spilling out, resting on a wet tile floor",
        //        negative_prompt = "(human:3), person, people, man, (woman:3), child, children, boy, (girl:3), face, head, (human body:3), skin, hands, arms, (legs:3), eyes",
        //        blendMode="Foreground",
        //        feather = 0.25f
        //    },
        //    new Region
        //    {
        //        x = 0.75f,
        //        y = 0.55f,
        //        w = 0.3f,
        //        h = 0.3f,
        //        prompt = "a subway train door, metallic, closed, slightly dented, smeared with grime, reflecting blue neon light",
        //        negative_prompt = "(human:3), person, people, man, (woman:3), child, children, boy, (girl:3), face, head, (human body:3), skin, hands, arms, (legs:3), eyes",
        //        blendMode="Foreground",
        //        feather = 0.25f
        //    }
        //};


        var requestData = new Txt2ImgRequest
        {
            steps = 1,
            width = 512,
            height = 512,
            enable_hr = false,
            restore_faces = false,
            tiling = false,
            prompt = "masterpiece,  best quality, ultra high reslotion, highly detailed",
            negative_prompt = "(worst quality:2), (low quality:2), (normal quality:2), lowers, ((monochrome)), ((grayscale)), watermark",
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
                byte[] imageBytes = System.Convert.FromBase64String(response.images[0]);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                Debug.Log("✅ 圖片成功產生（Region Prompt Control）");

                callback?.Invoke(texture);
                //imageUI.texture = tex;
                //imageUI.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
}
