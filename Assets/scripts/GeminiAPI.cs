using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Text.RegularExpressions;
using SimpleJSON;
public class GeminiAPI : MonoBehaviour
{
    [Header("Google Gemini Settings")]
    public string apiKey = "AIzaSyBAU-OT51CaK3bVVW5mjLfinrzdxehK-0U";  // <-- �o�̶�J�A�� Google API Key
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    void Start()
    {
        //StartCoroutine(SendRequest("�ڭ̷Q���@�ӹC���A�C�����e�O�|���ƦW���a�M�@��AI�AAI�|�ھ��D�ؤ����q�����͹ϼҫ�����r�A�@�}�l������r�|���͹ϼҫ��ͥX�Ӫ��Ϥ������D�ءA�M�������a�ݹϲq�D�جO����A�p�G���a��������AI�N�|������r�i�H�ͪ����D�ءA�@������쪱�a�i�H�z�L�ͥX���Ϥ��q���D�ج���C�Ҧp�D�جO:�u�E�Y�v�A�Ĥ@���q���ܡG �u�X�n�B�ξA�B���P�v�� �Ϥ��i��ȧe�{�@�ط��ɩΦw�R���^��C�ĤG���q���ܡG �u�׫ǡB�]�ߡB����v�� �Ϥ��i��}�l�X�{�׫Ǥ����ηt�ܺίv���ҡC�ĤT���q���ܡG �u�Y�䪺�u�@�̡B�׫ǥ��ơv�� �Ϥ���i�����yø�X�@�ӪE�Y�Ψ�ζH�ƯS�x�C�̲״��ܡ]�p�G�����q���^�G �u�X�n��R�B�ίv��Q�X�o�N�O�E�Y�I�v �� �Ϥ��M���̲ܳת������סC�A���U�ӱN��@�@�W�X�D�̡A�u�n���ڨC�Ӷ��q������r�N�n�A�榡���i�D��:�u�A���D�ءv�A�Ĥ@���q:�u����r�v�A�ĤG���q:�u����r�v�A�ĤT���q:�u����r�v�A�ĥ|���q:�u����r�v�j�A���n���h�l���r�A�ж}�l�X�H���@���D�ءC"));

    }
    public IEnumerator SendRequest(string prompt, Action<string> onComplete)
    {
        // �ǳƭn�o�e�� JSON �ШD
        string jsonBody = "{\"contents\": [{\"parts\": [{\"text\": \"" + prompt + "\"}]}]}";

        using (UnityWebRequest request = new UnityWebRequest($"{apiUrl}?key={apiKey}", "POST"))
        {
            // �]�w Header
            request.SetRequestHeader("Content-Type", "application/json");

            // �]�w Body
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // �o�e�ШD
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                JSONNode response = JSON.Parse(jsonResponse);
                string reply = response["candidates"][0]["content"]["parts"][0]["text"];
                Debug.Log("Gemini �^��: " + reply);
                //List<string> extractedWords = ExtractTextInsideQuotes(reply);

                onComplete?.Invoke(reply);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }
    public IEnumerator SendPhotoRequest(string prompt, string base64Image, Action<string> onComplete)
    {
        string jsonBody;
            // �]�t�Ϥ����ШD
        jsonBody = "{\"contents\": [{\"parts\": [" +
                   "{\"inline_data\": {\"data\": \"" + base64Image + "\", \"mimeType\": \"image/png\"}}," + 
                   "{\"text\": \"" + prompt + "\"}" +
                   "]}]}";

        using (UnityWebRequest request = new UnityWebRequest($"{apiUrl}?key={apiKey}", "POST"))
        {
            // �]�w Header
            request.SetRequestHeader("Content-Type", "application/json");

            // �]�w Body
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // �o�e�ШD
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                JSONNode response = JSON.Parse(jsonResponse);
                string reply = response["candidates"][0]["content"]["parts"][0]["text"];
                Debug.Log("Gemini �^��: " + reply);
                onComplete?.Invoke(reply);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }
}
