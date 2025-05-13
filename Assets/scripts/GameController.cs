using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

    public Animator characteranimator;

    public StableDiffusionRegionPrompt stablediffusionregionprompt;

    public VoiceAudioPlayer voiceAudioPlayer;

    private String NowDifficulty;
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
        NowDifficulty = diff;
        // �ھ������׽վ�C���ѼơA�Ҧp�ĤH�ƶq�B��q��
        if (diff == "Easy") { /* �]�w²��Ҧ��Ѽ� */
            StartCoroutine(GetGeminiKeywords(1));
        }
        else if (diff == "Hard") { /* �]�w�x���Ҧ��Ѽ� */
            StartCoroutine(GetGeminiKeywords(2));
        }
        else
        {
            StartCoroutine(GetGeminiKeywords(3));
        }
    }

    private void OnImageGenerated(Texture2D generatedTexture)
    {
        if (generatedTexture != null)
        {
            byte[] imageBytes = generatedTexture.EncodeToJPG();
            //StartCoroutine(huggingFaceAPI.UpscaleImage(imageBytes, (result) =>
            //{
            //    imageBytes = result;
            //}));
            photonView.RPC("SyncImage", RpcTarget.All, imageBytes);
            // �}�l����ǰe�Ϥ�
            //StartCoroutine(DelayedSyncImage(imageBytes, 30f)); // 300 ����
        }
        else
        {
            Debug.LogError("�Ϥ��ͦ����ѡI");
        }
    }
    //private IEnumerator DelayedSyncImage(byte[] imageBytes, float delaySeconds)
    //{
    //    yield return new WaitForSecondsRealtime(delaySeconds);
    //    ApplyDifficultySettings(NowDifficulty);
    //    photonView.RPC("SyncImage", RpcTarget.All, imageBytes);
    //}
    [PunRPC]
    void SyncImage(byte[] imageBytes)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);
        Debug.Log("�Ϥ��[������");
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
            yield return StartCoroutine(geminiAPI.SendRequest("�ڭ̷Q���@�ӹC���A�C�����e�O�|���ƦW���a�M�@��AI�A�M�������a�ݹϲq�D�جO����A�p�G���a��������AI�N�|������r�i�H�ͪ����D�ءA�@������쪱�a�i�H�z�L�ͥX���Ϥ��q���D�ج���C�^���|�O����D���˦��A�|���|�ӿﶵ�A�@�ӥ��T�A�T�ӿ��~�A�ЧA�X�D�ءA�A�n�^�����˦����A�D��:�u���ܦr�v�A�ﶵA:�u���״��ܦr�v�A�ﶵB:�u���ܦr�v�A�ﶵC:�u���ܦr�v�A�ﶵD:�u���ܦr�v�A���T���׬�:�u���ܦr�v�A�ɶq�����T���~�A�D�إέ^��A�ﶵ�Τ���A���n���h�l���r�A���ڤ@�D�N�n�F�A���I�Ÿ����n�ܡA�@�w�n�ϥΡu�v�ئ��ܦr�C", (result) =>
            {
                keywords = ExtractTextInsideQuotes(result);
            }));
            Debug.Log("����쪺����r: " + string.Join(", ", keywords));
            GetOption();
            answer = keywords[1];
            StartCoroutine(huggingFaceAPI.GenerateImageFromText(keywords[0], OnImageGenerated));
        }
        else if(mode == 2)
        {
            yield return StartCoroutine(geminiAPI.SendRequest("�ڭ̷Q���@�ӹC���A�C�����e�O�|���ƦW���a�M�@��AI�A�M�������a�ݹϲq�D�جO����A�p�G���a��������AI�N�|������r�i�H�ͪ����D�ءA�@������쪱�a�i�H�z�L�ͥX���Ϥ��q���D�ج���C�^���|�O����D���˦��A�|���|�ӿﶵ�A�@�ӥ��T�A�T�ӿ��~�A�ЧA�X�D�ءA�C�������@�˪��D�ءA�D�ج����妨�y�έ^��h��F�A�A�n�^�����˦����A�D��:�u���ܦr�v�A�ﶵA:�u���״��ܦr�v�A�ﶵB:�u���ܦr�v�A�ﶵC:�u���ܦr�v�A�ﶵD:�u���ܦr�v�A���T���׬�:�u���ܦr�v�A�D�إέ^��y�z�A�ﶵ�Τ���A���n���h�l���r�A���ڤ@�D�N�n�F�A���I�Ÿ����n�ܡA�@�w�n�ϥΡu�v�ئ��ܦr�C", (result) =>
            {
                keywords = ExtractTextInsideQuotes(result);
            }));
            Debug.Log("����쪺����r: " + string.Join(", ", keywords));
            GetOption();
            answer = keywords[1];
            StartCoroutine(huggingFaceAPI.GenerateImageFromText(keywords[0], OnImageGenerated));
        }
        else
        {
            StartCoroutine(ChainCoroutines());
        }

        // ���� API �^��
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
        if (Buttontext.text == answer)
        {
            characteranimator.SetTrigger("correct");
        }
        else
        {
            //characteranimator.Play("Wrong", 0, 0f);
            characteranimator.SetTrigger("wrong");
        }
    }
    private string Uncheckedprompt;
    private string checkedprompt;
    IEnumerator ReadSettingFileAndSend()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "LLM�]�w.txt");

    #if UNITY_ANDROID && !UNITY_EDITOR
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Ū�ɥ���: " + www.error);
            yield break;
        }
        string fileContent = www.downloadHandler.text;
    #else
            string fileContent = System.IO.File.ReadAllText(path);
    #endif

        // �I�s Gemini API �öǤJ�ɮפ��e
        yield return StartCoroutine(geminiAPI.SendRequest(fileContent, (result) =>
        {
            Uncheckedprompt = result;
            //keywords = ExtractContentsSeparatedByDash(result);

            ////foreach (var content in keywords)
            ////{
            ////    Debug.Log(content);
            ////}
            ////Debug.Log(keywords.Count);
            //for (int i=0;i< keywords.Count / 7; i++)
            //{
            //    stablediffusionregionprompt.InputRegion(keywords[i * 7], float.Parse(keywords[i * 7 + 1]), float.Parse(keywords[i * 7 + 2]), float.Parse(keywords[i * 7 + 3]), float.Parse(keywords[i * 7 + 4]), keywords[i * 7 + 5], keywords[i * 7 + 6]);
            //    //Debug.Log(i);
            //}
        }));
    }
    IEnumerator ReadCheckFileAndSend()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "�ˬd�T��.txt");

#if UNITY_ANDROID && !UNITY_EDITOR
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Ū�ɥ���: " + www.error);
            yield break;
        }
        string fileContent = www.downloadHandler.text;
#else
        string fileContent = System.IO.File.ReadAllText(path);
#endif
        checkedprompt = Uncheckedprompt + fileContent;

        // �I�s Gemini API �öǤJ�ɮפ��e
        yield return StartCoroutine(geminiAPI.SendRequest(checkedprompt, (result) =>
        {
            Debug.Log(result);
            answer = result;
            keywords = ExtractContentsSeparatedByDash(result);

            ////foreach (var content in keywords)
            ////{
            ////    Debug.Log(content);
            ////}
            ////Debug.Log(keywords.Count);
            for (int i = 0; i < keywords.Count / 7; i++)
            {
                stablediffusionregionprompt.InputRegion(keywords[i * 7], float.Parse(keywords[i * 7 + 1]), float.Parse(keywords[i * 7 + 2]), float.Parse(keywords[i * 7 + 3]), float.Parse(keywords[i * 7 + 4]), keywords[i * 7 + 5], keywords[i * 7 + 6]);
                //Debug.Log(i);
            }
        }));
    }
    List<string> ExtractContentsSeparatedByDash(string text)
    {
        List<string> allItems = new List<string>();
        MatchCollection matchCollection = Regex.Matches(text, @"\{(.*?)\}");

        foreach (Match match in matchCollection)
        {
            string insideBraces = match.Groups[1].Value;
            // �� '-' ����
            string[] parts = insideBraces.Split('=');

            foreach (var part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    allItems.Add(part.Trim());
                }
            }
        }

        return allItems;
    }
    static List<string> ExtractTextInsideQuotes(string input)
    {
        List<string> results = new List<string>();
        MatchCollection matches = Regex.Matches(input, "[�u\"](.*?)[�v\"]");

        foreach (Match match in matches)
        {
            results.Add(match.Groups[1].Value);
        }

        return results;
    }
    private IEnumerator ChainCoroutines()
    {
        yield return StartCoroutine(ReadSettingFileAndSend());
        yield return StartCoroutine(ReadCheckFileAndSend());
        yield return StartCoroutine(stablediffusionregionprompt.GenerateImageWithRegions(OnImageGenerated));
    }

    public void SubmitButton(Text MyAnswer)
    {
        StartCoroutine(CheckAnswer(MyAnswer.text));
    }
    private IEnumerator CheckAnswer(String MyAnswer)
    {
        String CheckAnswerPrompt = "�ثe�D�ت�����r���o��"+answer+ "�H�U���ڪ��^���A�ЧA�^���ڳo�Ӧ^�����S������������r�̭�������A�p�G���ʴӪ��h�^���������ۦP���ءA�Ҧp:����=���B�����������C�ڪ��^����: "+MyAnswer+ "�ЧA�^�Ч�(���T) or (���~) �A�û�����]�C";
        yield return StartCoroutine(geminiAPI.SendRequest(CheckAnswerPrompt, (result) => {
            if (result.Contains("���T"))
            {
                characteranimator.SetTrigger("correct");
                voiceAudioPlayer.AudioPlay(1);
            }
            else
            {
                characteranimator.SetTrigger("wrong");
                voiceAudioPlayer.AudioPlay(2);
            }
        }
        ));

    }
}