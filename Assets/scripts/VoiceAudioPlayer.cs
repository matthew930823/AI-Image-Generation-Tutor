using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.Video;

public class VoiceAudioPlayer : MonoBehaviour
{
    public AudioClip[] audioClip;
    private AudioSource audioSource;
    private Recorder recorder;
    public VideoPlayer videoPlayer;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        recorder = GetComponent<Recorder>();

        //if (audioClip != null)
        //{
        //    audioSource.clip = audioClip[0];
        //    audioSource.loop = false;

        //    // 確保 Recorder 傳輸音頻
        //    recorder.TransmitEnabled = true;

        //    // 開始播放音頻
        //    audioSource.Play();
        //    Invoke("StopLoop", 20f);
        //}
        //AudioPlay(0);
    }
    void StopLoop()
    {
        videoPlayer.isLooping = false;
    }
    
    public void AudioPlay(int index)
    {
        audioSource.clip = audioClip[index];//0遊戲敘述簡單 1答對 2答錯 3生成詳解時 4詳解生成冷卻 5題目生好時 6可以開始遊戲了喔 7遊戲敘述困難 8答錯第一次時 9答錯兩次時 10答對一題 11答對兩題 12答對三題 13按到重覆選項
                                            //考核模式: 14遊戲介紹 15開始生圖 16沒選模型 17沒選解析度 18沒選參考圖 19沒選預處理 20沒選主體提示詞 21沒選重點提示詞 22分數及格時 23分數不及格時
        audioSource.loop = false;           //匠人模式:0遊戲敘述 1進入選主題 2風格 3主體 4年齡 5姿勢 6參考圖 7表情 8色調 9背景 10解析度 11細節 12生圖 13朝代 14食物主體
                                            //猜題模式:0遊戲敘述 1題目生好時 2答對時 3答案重複時 4答錯時 5缺少選區域按提交時 6缺少填回答按提交時 7分數大於等於100時 8分數大於等於60時 9 分數小於60時
                                            // 確保 Recorder 傳輸音頻
        recorder.TransmitEnabled = true;

        // 開始播放音頻
        audioSource.Play();
    }
}
