using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public void SelectDifficulty(string difficulty)
    {
        if (!PhotonNetwork.IsMasterClient) return; // �u���ХD�i�H�]�w������

        // �]�w�ж��ݩ�
        Hashtable roomProperties = new Hashtable();
        roomProperties["Difficulty"] = difficulty;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

        Debug.Log($"�ХD�]�w������: {difficulty}");
    }

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return; // �u���ХD�i�H�}�l�C��

        PhotonNetwork.LoadLevel("gameScenes"); // �ХD���������A��L���a�|�P�B
    }
}
