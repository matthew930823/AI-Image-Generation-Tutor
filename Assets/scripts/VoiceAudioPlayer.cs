using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.Video;

public class VoiceAudioPlayer : MonoBehaviour
{
    public AudioClip audioClip;
    private AudioSource audioSource;
    private Recorder recorder;
    public VideoPlayer videoPlayer;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        recorder = GetComponent<Recorder>();

        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.loop = false;

            // �T�O Recorder �ǿ魵�W
            recorder.TransmitEnabled = true;

            // �}�l�����W
            audioSource.Play();
            Invoke("StopLoop", 20f);
        }
    }
    void StopLoop()
    {
        videoPlayer.isLooping = false;
    }
}
