using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviourPunCallbacks
{
    private string Diff;
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
        Diff = difficulty;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

        Debug.Log($"�ХD�]�w������: {difficulty}");
    }

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return; // �u���ХD�i�H�}�l�C��
        if(Diff == "Easy" )
            PhotonNetwork.LoadLevel("MultiChoicegameScenes"); // �ХD���������A��L���a�|�P�B
        else if(Diff == "Hard")
            PhotonNetwork.LoadLevel("HardModegameScenes");
        else
            PhotonNetwork.LoadLevel("gameScenes"); // �ХD���������A��L���a�|�P�B
    }
}
