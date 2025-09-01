using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class DifficultySelector : MonoBehaviourPunCallbacks
{
    private string Diff;
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public void SelectDifficulty(string difficulty)
    {
        //if (!PhotonNetwork.IsMasterClient) return; // �u���ХD�i�H�]�w������

        // �]�w�ж��ݩ�
        Hashtable roomProperties = new Hashtable();
        roomProperties["Difficulty"] = difficulty;
        Diff = difficulty;
        //PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        DifficultyManager.Instance.SetDifficulty(Diff);
        Debug.Log($"�ХD�]�w������: {difficulty}");
        StartGame();
    }

    public void StartGame()
    {
        //if (!PhotonNetwork.IsMasterClient) return; // �u���ХD�i�H�}�l�C��
        if(Diff == "Easy" )
            SceneManager.LoadScene("MultiChoicegameScenes"); // �ХD���������A��L���a�|�P�B
        else if(Diff == "Hard")
            SceneManager.LoadScene("HardModegameScenes");
        else if (Diff == "Assessment")
            SceneManager.LoadScene("AssessmentModegameScenes");
        else if (Diff == "Agent")
            SceneManager.LoadScene("AIAgentMode");
        else
            SceneManager.LoadScene("gameScenes"); // �ХD���������A��L���a�|�P�B
    }
}
