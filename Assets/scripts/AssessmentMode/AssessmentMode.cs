using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AssessmentMode : MonoBehaviour
{
    int Resolution = 0;
    string Checkpoint = "";
    string Controlnet = "";
    string MainPrompt = "";
    string KeyPrompt = "";
    string[] Prompt = new string[5];

    public StableDiffusionRegionPrompt stableDiffusionRegionPrompt;
    public SelectImage selectImage;
    public Sprite noSelect;
    public TMP_InputField[] inputFields;
    public TMP_Dropdown[] dropdowns;
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
        Resolution = 0;
        Checkpoint = "";
        Controlnet = "";
        MainPrompt = "";
        KeyPrompt = "";
        Prompt = new string[5];
        selectImage.selectedImageDisplay.sprite = noSelect;
        // �M�ũҦ���J���
        foreach (TMP_InputField input in inputFields)
        {
            input.text = "";
        }

        // �N�Ҧ��U�Կ��]���Ĥ@�ӿﶵ�]���� 0�^
        foreach (TMP_Dropdown dropdown in dropdowns)
        {
            if (dropdown.options.Count > 0)
            {
                dropdown.value = 0;
                dropdown.RefreshShownValue(); // ��s���
            }
        }
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
                if(info == "Resolution")
                    Resolution = 0;
                else
                    Resolution = int.Parse(info);
                break;
            case "Checkpoint":
                if (info == "Model")
                    Checkpoint = "";
                else
                    Checkpoint = info;
                break;
            case "Controlnet":
                if (info == "Controlnet")
                    Controlnet = "";
                else
                    Controlnet = info;
                break;
            default:
                Prompt[int.Parse(type)] = info;
                break;
        }
    }
    public string ConvertSelectedImageToBase64()
    {
        // ���o Sprite ���� Texture2D
        Sprite sprite = selectImage.selectedImageDisplay.sprite;
        
        if (sprite.name == "���Controlnet�Ѧҹ�")
        {
            Debug.Log("�S�����Controlnet�Ѧҹ�");
            return null;
        }

        // �N Sprite.texture �ƻs���s�� Texture2D�A�_�h���� Sprite �i�ण�ઽ��Ū��
        Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.RGBA32, false);
        texture.SetPixels(sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height
        ));
        texture.Apply();

        // �s�X�� PNG
        byte[] imageBytes = texture.EncodeToPNG();

        // �ഫ�� Base64 �r��
        string base64String = System.Convert.ToBase64String(imageBytes);

        // �i��G����Ȧs�� Texture
        Destroy(texture);

        return base64String;
    }
}
