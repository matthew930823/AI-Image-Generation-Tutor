using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
public class GameController : MonoBehaviourPun
{
    public HuggingFaceAPI huggingFaceAPI; // HuggingFaceAPI ���ޥ�
    public InputField descriptionInput;  // ���a�y�z����J��
    public Image resultImage;            // �Ω���ܥͦ��Ϥ��� UI Image
    public Text roomNumber;

    public GeminiAPI geminiAPI;
    private List<string> keywords = null;

    public List<Button> OptionButton;

    private string answer;
    public Image uiImage;
    public Sprite trueAns;
    public Sprite falseAns;
    void Start()
    {
        roomNumber.text = "Room : " + PhotonNetwork.CurrentRoom.Name;
    }
    public void ApplyDifficultySettings(string diff)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            
            return;
        }
        Debug.Log(diff);
        // �ھ������׽վ�C���ѼơA�Ҧp�ĤH�ƶq�B��q��
        if (diff == "Easy") { /* �]�w²��Ҧ��Ѽ� */
            StartCoroutine(GetGeminiKeywords(1));
        }
        else if (diff == "Hard") { /* �]�w�x���Ҧ��Ѽ� */
            StartCoroutine(GetGeminiKeywords(2));
        }
    }

    private void OnImageGenerated(Texture2D generatedTexture)
    {
        if (generatedTexture != null)
        {
            byte[] imageBytes = generatedTexture.EncodeToJPG();
            StartCoroutine(huggingFaceAPI.UpscaleImage(imageBytes, (result) =>
            {
                imageBytes = result;
            }));
            photonView.RPC("SyncImage", RpcTarget.All, imageBytes);
        }
        else
        {
            Debug.LogError("�Ϥ��ͦ����ѡI");
        }
    }

    [PunRPC]
    void SyncImage(byte[] imageBytes)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);
        Debug.Log("����");
        resultImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private IEnumerator GetGeminiKeywords(int mode)
    {

        // �I�s API�A��������
        //yield return StartCoroutine(geminiAPI.SendRequest("�ڭ̷Q���@�ӹC���A�C�����e�O�|���ƦW���a�M�@��AI�AAI�|�ھ��D�ؤ����q�����͹ϼҫ�����r�A�@�}�l������r�|���͹ϼҫ��ͥX�Ӫ��Ϥ������D�ءA�M�������a�ݹϲq�D�جO����A�p�G���a��������AI�N�|������r�i�H�ͪ����D�ءA�@������쪱�a�i�H�z�L�ͥX���Ϥ��q���D�ج���C�Ҧp�D�جO:�u�E�Y�v�A�Ĥ@���q���ܡG �u�X�n�B�ξA�B���P�v�� �Ϥ��i��ȧe�{�@�ط��ɩΦw�R���^��C�ĤG���q���ܡG �u�׫ǡB�]�ߡB����v�� �Ϥ��i��}�l�X�{�׫Ǥ����ηt�ܺίv���ҡC�ĤT���q���ܡG �u�Y�䪺�u�@�̡B�׫ǥ��ơv�� �Ϥ���i�����yø�X�@�ӪE�Y�Ψ�ζH�ƯS�x�C�̲״��ܡ]�p�G�����q���^�G �u�X�n��R�B�ίv��Q�X�o�N�O�E�Y�I�v �� �Ϥ��M���̲ܳת������סC�A���U�ӱN��@�@�W�X�D�̡A�u�n���ڨC�Ӷ��q������r�N�n�A�榡���i�D��:�u�A���D�ءv�A�Ĥ@���q:�u����r�B����r�B����r...�v�A�ĤG���q:�u����r�B����r�B����r...�v�A�ĤT���q:�u����r�B����r�B����r...�v�A�ĥ|���q:�u����r�B����r�B����r...�v�j�A���n���h�l���r�A�ж}�l�X�H���@���D�ءA����r�έ^���ܡA�����I�Ÿ����ܡC", (result) =>
        //{
        //    keywords = result;
        //}));

        if(mode == 1)
        {
            yield return StartCoroutine(geminiAPI.SendRequest("�ڭ̷Q���@�ӹC���A�C�����e�O�|���ƦW���a�M�@��AI�A�M�������a�ݹϲq�D�جO����A�p�G���a��������AI�N�|������r�i�H�ͪ����D�ءA�@������쪱�a�i�H�z�L�ͥX���Ϥ��q���D�ج���C�^���|�O����D���˦��A�|���|�ӿﶵ�A�@�ӥ��T�A�T�ӿ��~�A�ЧA�X�D�ءA�A�n�^�����˦����A�D��:�u���ܦr�v�A�ﶵA:�u���ܦr�v�A�ﶵB:�u���ܦr�v�A�ﶵC:�u���ܦr�v�A�ﶵD:�u���ܦr�v�A���T���׬�:�u���ܦr�v�A�ɶq�����T���~�A�D�إέ^��A�ﶵ�Τ���A���n���h�l���r�A���ڤ@�D�N�n�F�A���I�Ÿ����n�ܡA�@�w�n�ϥΡu�v�ئ��ܦr�C", (result) =>
            {
                keywords = result;
            }));
        }
        else
        {
            yield return StartCoroutine(geminiAPI.SendRequest("�ڭ̷Q���@�ӹC���A�C�����e�O�|���ƦW���a�M�@��AI�A�M�������a�ݹϲq�D�جO����A�p�G���a��������AI�N�|������r�i�H�ͪ����D�ءA�@������쪱�a�i�H�z�L�ͥX���Ϥ��q���D�ج���C�^���|�O����D���˦��A�|���|�ӿﶵ�A�@�ӥ��T�A�T�ӿ��~�A�ЧA�X�D�ءA�C�������@�˪��D�ءA�D�ج����妨�y�έ^��h��F�A�A�n�^�����˦����A�D��:�u���ܦr�v�A�ﶵA:�u���ܦr�v�A�ﶵB:�u���ܦr�v�A�ﶵC:�u���ܦr�v�A�ﶵD:�u���ܦr�v�A���T���׬�:�u���ܦr�v�A�D�إέ^��y�z�A�ﶵ�Τ���A���n���h�l���r�A���ڤ@�D�N�n�F�A���I�Ÿ����n�ܡA�@�w�n�ϥΡu�v�ئ��ܦr�C", (result) =>
            {
                keywords = result;
            }));
        }
        
        // ���� API �^��
        Debug.Log("����쪺����r: " + string.Join(", ", keywords));
        GetOption();
        answer = keywords[1];
        StartCoroutine(huggingFaceAPI.GenerateImageFromText(keywords[0], OnImageGenerated));
        //StartCoroutine(ChangeEvery10Seconds());
    }
    //IEnumerator ChangeEvery10Seconds()
    //{
    //    for (int i = 1; i <= 4; i++)  // ���� 4 ��
    //    {
    //        Debug.Log(keywords[i]);
    //        string prompt = keywords[i];
    //        StartCoroutine(huggingFaceAPI.GenerateImageFromText(prompt, OnImageGenerated));
    //        yield return new WaitForSeconds(10f);  // ���� 10 ��
    //    }
    //}
    void GetOption()
    {
        for (int i = 1; i <= 4; i++)  // ���� 4 ��
        {
            string prompt = keywords[i];
            Text buttonText = OptionButton[i-1].GetComponentInChildren<Text>();
            buttonText.text = prompt;
        }
    }
    public void CheckAns(Text Buttontext)
    {
        //if(Buttontext.text == answer)
        //{
        //    uiImage.sprite = trueAns;
        //}
        //else
        //{
        //    uiImage.sprite = falseAns;
        //}
    }
}