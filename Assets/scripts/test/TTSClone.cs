using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TTSClone : MonoBehaviour
{
    private string serverUrl = "http://127.0.0.1:5000"; // 确保 FastAPI 服务器地址正确
    private string fixedReferenceAudio = "刻晴.wav"; // 服务器上的固定参考音频
    //private string reference_text = "你会爬树吗？我非常擅长这个，可以教你哦。还有，神社那边有棵树特别适合偷…啊不，特别适合小憩。下次你想睡午觉的时候，记得来找我，我们一起爬上去午睡吧。";
    void Start()
    {
        StartCoroutine(RequestTTS("哈囉你好，世界"));
        //string url = "C:/Users/matth/Desktop/專題測試/CosyVoice-main/CosyVoice-main/tmp/clone-20250317-194508.wav";
        //StartCoroutine(DownloadAndPlayAudio(url));
    }
    //TTS
    IEnumerator RequestTTS(string text)
    {
        string cloneUrl = $"{serverUrl}/tts";
        string jsonData = $"{{\"text\": \"{text}\", \"speaker_wav\": \"{fixedReferenceAudio}\"}}";

        using (UnityWebRequest cloneRequest = new UnityWebRequest(cloneUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            cloneRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            cloneRequest.downloadHandler = new DownloadHandlerBuffer();
            cloneRequest.SetRequestHeader("Content-Type", "application/json");

            yield return cloneRequest.SendWebRequest();

            if (cloneRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"請求 TTS 失敗: {cloneRequest.error}");
                yield break;
            }

            string json = cloneRequest.downloadHandler.text;
            string audioPath = JsonUtility.FromJson<AudioPathResponse>(json).audio_path;
            string fullPath = "file:///C:/Users/matth/Downloads/TTS/" + audioPath.Replace("\\", "/");

            Debug.Log($"下載路徑: {fullPath}");
            StartCoroutine(DownloadAndPlayAudio(fullPath));
        }
    }
    [System.Serializable]
    public class AudioPathResponse
    {
        public string audio_path;
    }
    //Cosyvoice
    //IEnumerator RequestTTS(string text)
    //{
    //    string cloneUrl = $"{serverUrl}/clone_eq";
    //    string jsonData = $"{{\"text\": \"{text}\", \"reference_audio\": \"{fixedReferenceAudio}\", \"reference_text\": \"{reference_text}\",\"speed\": 1.0}}";

    //    using (UnityWebRequest cloneRequest = new UnityWebRequest(cloneUrl, "POST"))
    //    {
    //        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
    //        cloneRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //        cloneRequest.downloadHandler = new DownloadHandlerBuffer();
    //        cloneRequest.SetRequestHeader("Content-Type", "application/json");

    //        yield return cloneRequest.SendWebRequest();

    //        if (cloneRequest.result != UnityWebRequest.Result.Success)
    //        {
    //            Debug.LogError($"請求 TTS 失敗: {cloneRequest.error}");
    //            yield break;
    //        }

    //        string audioDownloadUrl = cloneRequest.downloadHandler.text;  // 服务器返回音频URL
    //        Debug.Log($"TTS 生成成功: {audioDownloadUrl}");

    //        // 下载并播放音频
    //        StartCoroutine(DownloadAndPlayAudio(audioDownloadUrl));
    //    }
    //}

    IEnumerator DownloadAndPlayAudio(string audioUrl)
    {

        audioUrl = audioUrl.Replace("\\", "/").Trim('"'); 
        Debug.Log($"播放音頻路徑: {audioUrl}");
        using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.WAV))
        {
            yield return audioRequest.SendWebRequest();

            if (audioRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"音頻下載失敗: {audioRequest.error}");
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(audioRequest);
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.Play();
            Debug.Log("播放成功");
        }
    }
}
