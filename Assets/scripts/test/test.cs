using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class test : MonoBehaviour
{
    public Image imageDisplay; // 拖拉 UI 元件進來

    public void OnUploadButtonClick()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("選擇圖片", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);

            // 將 Texture2D 轉為 Sprite
            Rect rect = new Rect(0, 0, tex.width, tex.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Sprite sprite = Sprite.Create(tex, rect, pivot);

            // 顯示圖片
            imageDisplay.sprite = sprite;
        }
#else
        Debug.LogWarning("此功能僅在 Editor 模式可用，建議使用第三方 Plugin 如 FileBrowser。");
#endif
    }
}
