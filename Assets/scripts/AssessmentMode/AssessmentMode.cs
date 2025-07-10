using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssessmentMode : MonoBehaviour
{
    int Resolution = 384;
    string Checkpoint = "CuteYukiMix";
    string Controlnet = "Canny";
    string MainPrompt = "";
    string KeyPrompt = "";
    string[] Prompt = new string[5];

    public StableDiffusionRegionPrompt stableDiffusionRegionPrompt;
    public SelectImage selectImage;
    public Sprite noSelect;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetValue(out int resolution, out string mainPrompt, out string keyPrompt, out string[] prompt, out string checkpoint, out string controlnet, out string imageBase64)
    {
        resolution = Resolution;
        mainPrompt = MainPrompt;
        keyPrompt = KeyPrompt;
        prompt = Prompt;
        checkpoint = Checkpoint;
        controlnet = Controlnet;
        imageBase64 = ConvertSelectedImageToBase64();
    }
    public void ClearValue()
    {
        //Resolution = 384;
        //Checkpoint = "CuteYukiMix";
        //Controlnet = "Canny";
        MainPrompt = "";
        KeyPrompt = "";
        Prompt = new string[5];
        selectImage.selectedImageDisplay.sprite = noSelect;
    }
    public void SetValue(string info,string type)
    {
        switch (type)
        {
            case "MainPrompt":
                MainPrompt = info;
                break;
            case "KeyPrompt":
                KeyPrompt = info;
                break;
            case "Resolution":
                Resolution = int.Parse(info);
                break;
            case "Checkpoint":
                Checkpoint = info;
                break;
            case "Controlnet":
                Controlnet = info;
                break;
            default:
                Prompt[int.Parse(type)] = info;
                break;
        }
    }
    public string ConvertSelectedImageToBase64()
    {
        // 取得 Sprite 中的 Texture2D
        Sprite sprite = selectImage.selectedImageDisplay.sprite;
        
        if (sprite.name == "選擇Controlnet參考圖")
        {
            Debug.Log("沒有選擇Controlnet參考圖");
            return null;
        }

        // 將 Sprite.texture 複製成新的 Texture2D，否則有些 Sprite 可能不能直接讀取
        Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.RGBA32, false);
        texture.SetPixels(sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height
        ));
        texture.Apply();

        // 編碼為 PNG
        byte[] imageBytes = texture.EncodeToPNG();

        // 轉換為 Base64 字串
        string base64String = System.Convert.ToBase64String(imageBytes);

        // 可選：釋放暫存的 Texture
        Destroy(texture);

        return base64String;
    }
}
