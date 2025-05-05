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
            string prompt = "���U�ӧڷ|���A�Ϥ��A�A�n���O��Ϥ��T�I�������A���ƽd��0-10���A�������ӫ��[�B�зǡB���ȮM�A�H�D�`�Y�檺�зǵ���1.���[��:�Ϥ�����ı�l�ޤO�A�]�t�c�ϡB��m�B�Ӹ`�M�^�򪺾���M�ӷP2.�޿��:�Ϥ������������O�_�ŦX���z�B�Ŷ��P���Ҫ����`���Y�A�קK�X�{��ҿ��~�B���۵M�����γ����զX�٬�3.�����:�Ϥ����D��M�I�����c�O�_���`�B�L�}�ϡB�ᦱ�B���`���������D��X�榡:(���[�ʤ���) / (�޿�ʤ���) / (����ʤ���"; // �A�i�H�ھڻݭn�קﴣ��
            StartCoroutine(geminiAPI.SendPhotoRequest(prompt, base64Image, (response) =>
            {
                Debug.Log("Gemini �^���Ϥ��y�z: " + response);
            }));
        }
        else
        {
            Debug.LogError("���n�����󬰪šI");
        }
    }
}
