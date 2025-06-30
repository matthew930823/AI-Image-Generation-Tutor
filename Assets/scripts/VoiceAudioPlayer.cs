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

        //    // �T�O Recorder �ǿ魵�W
        //    recorder.TransmitEnabled = true;

        //    // �}�l�����W
        //    audioSource.Play();
        //    Invoke("StopLoop", 20f);
        //}
        AudioPlay(0);
    }
    void StopLoop()
    {
        videoPlayer.isLooping = false;
    }
    
    public void AudioPlay(int index)
    {
        audioSource.clip = audioClip[index];//1���� 2���� 3�ͦ��ԸѮ� 4�Ըѥͦ��N�o 5�D�إͦn�� 6�i�H�}�l�C���F��
        audioSource.loop = false;

        // �T�O Recorder �ǿ魵�W
        recorder.TransmitEnabled = true;

        // �}�l�����W
        audioSource.Play();
    }
}
