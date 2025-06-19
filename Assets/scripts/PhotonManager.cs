using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI loadingText; // 使用 TMP_Text
    private string baseText = "Loading";
    private int dotCount = 0;

    private void Start()
    {
        StartCoroutine(AnimateLoadingText());
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.LoadLevel("Lobby");
    }

    IEnumerator AnimateLoadingText()
    {
        while (true)
        {
            dotCount = (dotCount + 1) % 4; 
            loadingText.text = baseText + new string('.', dotCount);
            yield return new WaitForSeconds(0.5f);
        }
    }
}