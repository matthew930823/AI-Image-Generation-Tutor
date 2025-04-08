using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using SimpleJSON;
using System.Text.RegularExpressions;
public class HuggingFaceAPI : MonoBehaviour
{
    private string apiUrl = "https://api-inference.huggingface.co/models/stabilityai/stable-diffusion-3.5-large-turbo";
    private string apiKey = "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"; // �b�o�̴������z�� Hugging Face API Token
    private string SuperResolutionapiUrl = "https://api.segmind.com/v1/esrgan";
    private string SuperResolutionapiKey = "SG_deb5def02d6cc7a4";
    public GameObject generateText;
    public GameObject errorText;
    /// <summary>
    /// �ͦ��Ϥ��]��r��Ϥ��^
    /// </summary>
    /// <param name="prompt">���a��J����r�y�z</param>
    /// <param name="callback">�^�ը�ơA��^�ͦ��� Texture2D</param>
    public IEnumerator GenerateImageFromText(string prompt, System.Action<Texture2D> callback)
    {
        // �ˬd��J�O�_�X�k
        if (string.IsNullOrWhiteSpace(prompt))
        {
            Debug.LogError("��J�y�z�L�ġA�д��Ѥ@�Ӧ��Ī���r�y�z�I");
            yield break;
        }
        //generateText.SetActive(true);
        //errorText.SetActive(false);
        // ���T�c�� JSON �ƾ�
        string jsonData = "{\"inputs\":\"" + prompt.Replace("\"", "\\\"") + "\"}";

        // �o�e POST �ШD
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            // �]�m�ШD�Y
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            // �]�m�ШD��
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // �o�e�ШD
            yield return request.SendWebRequest();

            // �B�z�^��
            if (request.result == UnityWebRequest.Result.Success)
            {
                //generateText.SetActive(false);
                byte[] imageData = request.downloadHandler.data;
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);
                callback?.Invoke(texture);
            }
            else
            {
                //errorText.SetActive(true);
                Debug.LogError($"Hugging Face API �ШD����: {request.error}");
                Debug.LogError($"�Բӿ��~�T��: {request.downloadHandler.text}");
                callback?.Invoke(null);
            }
        }
    }
    public IEnumerator UpscaleImage(byte[] imageData, System.Action<byte[]> onComplete)
    {
        // �إ߽ШD
        UnityWebRequest request = new UnityWebRequest(SuperResolutionapiUrl, "POST");
        request.SetRequestHeader("x-api-key",  SuperResolutionapiKey);
        request.SetRequestHeader("Content-Type", "application/json");

        // �c�y JSON �ШD��
        string jsonPayload = "{\"image\":\"" + System.Convert.ToBase64String(imageData) + "\"}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // �o�e�ШD
        yield return request.SendWebRequest();

        // �ˬd�O�_�o�Ϳ��~
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            onComplete?.Invoke(null);
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            // �ѪR��^���Ϥ��ƾ�
            byte[] imageBytes = request.downloadHandler.data;
            //Texture2D texture = new Texture2D(2, 2);
            //texture.LoadImage(imageBytes);
            onComplete?.Invoke(imageBytes);
        }
    }

}
