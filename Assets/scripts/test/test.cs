using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class test : MonoBehaviour
{
    public Image myImage;
    public GeminiAPI geminiAPI;
    // Start is called before the first frame update
    void Start()
    {
        CallGeminiWithImage();
    }
    void CallGeminiWithImage()
    {
        if (myImage != null && myImage.sprite != null && myImage.sprite.texture != null && geminiAPI != null)
        {
            Texture2D texture = myImage.sprite.texture;
            byte[] bytes = texture.EncodeToJPG();
            string base64Image = Convert.ToBase64String(bytes);
            string prompt = "接下來我會給你圖片，你要分別對圖片三點做評分，分數範圍0-10分，打分應該客觀、標準、不客套，以非常嚴格的標準評分1.美觀性:圖片的視覺吸引力，包含構圖、色彩、細節和氛圍的整體和諧感2.邏輯性:圖片中元素之間是否符合物理、空間與情境的正常關係，避免出現比例錯誤、不自然拼接或場景組合矛盾3.完整性:圖片中主體和背景結構是否正常、無破圖、扭曲、異常拼接等問題輸出格式:(美觀性分數) / (邏輯性分數) / (完整性分數"; // 你可以根據需要修改提示
            StartCoroutine(geminiAPI.SendPhotoRequest(prompt, base64Image, (response) =>
            {
                Debug.Log("Gemini 回應圖片描述: " + response);
            }));
        }
        else
        {
            Debug.LogError("必要的物件為空！");
        }
    }
}
