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
                                            //�ҮּҦ�: 14�C������ 15�}�l�͹� 16�S��ҫ� 17�S��ѪR�� 18�S��Ѧҹ� 19�S��w�B�z 20�S��D�鴣�ܵ� 21�S�ﭫ�I���ܵ� 22���Ƥή�� 23���Ƥ��ή��
        audioSource.loop = false;           //�K�H�Ҧ�:0�C���ԭz 1�i�J��D�D 2���� 3�D�� 4�~�� 5���� 6�Ѧҹ� 7�� 8��� 9�I�� 10�ѪR�� 11�Ӹ` 12�͹� 13�¥N 14�����D��
                                            //�q�D�Ҧ�:0�C���ԭz 1�D�إͦn�� 2����� 3���׭��Ʈ� 4������ 5�ʤֿ�ϰ������� 6�ʤֶ�^��������� 7���Ƥj�󵥩�100�� 8���Ƥj�󵥩�60�� 9 ���Ƥp��60��
                                            // �T�O Recorder �ǿ魵�W
        recorder.TransmitEnabled = true;

        // �}�l�����W
        audioSource.Play();
    }
}
