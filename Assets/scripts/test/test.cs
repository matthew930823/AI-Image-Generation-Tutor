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
    public Image imageDisplay; // ��� UI ����i��

    public void OnUploadButtonClick()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("��ܹϤ�", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);

            // �N Texture2D �ର Sprite
            Rect rect = new Rect(0, 0, tex.width, tex.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Sprite sprite = Sprite.Create(tex, rect, pivot);

            // ��ܹϤ�
            imageDisplay.sprite = sprite;
        }
#else
        Debug.LogWarning("���\��Ȧb Editor �Ҧ��i�ΡA��ĳ�ϥβĤT�� Plugin �p FileBrowser�C");
#endif
    }
}
