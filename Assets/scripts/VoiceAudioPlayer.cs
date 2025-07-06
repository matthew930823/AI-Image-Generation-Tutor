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
        //AudioPlay(0);
    }
    void StopLoop()
    {
        videoPlayer.isLooping = false;
    }
    
    public void AudioPlay(int index)
    {
        audioSource.clip = audioClip[index];//0�C���ԭz²�� 1���� 2���� 3�ͦ��ԸѮ� 4�Ըѥͦ��N�o 5�D�إͦn�� 6�i�H�}�l�C���F�� 7�C���ԭz�x�� 8�����Ĥ@���� 9�����⦸�� 10����@�D 11������D 12����T�D 13���쭫�пﶵ
        audioSource.loop = false; 

        // �T�O Recorder �ǿ魵�W
        recorder.TransmitEnabled = true;

        // �}�l�����W
        audioSource.Play();
    }
}
