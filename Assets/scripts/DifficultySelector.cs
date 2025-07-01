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
        if (!PhotonNetwork.IsMasterClient) return; // 只有房主可以設定難易度

        // 設定房間屬性
        Hashtable roomProperties = new Hashtable();
        roomProperties["Difficulty"] = difficulty;
        Diff = difficulty;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

        Debug.Log($"房主設定難易度: {difficulty}");
    }

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return; // 只有房主可以開始遊戲
        if(Diff == "Easy" )
            PhotonNetwork.LoadLevel("MultiChoicegameScenes"); // 房主切換場景，其他玩家會同步
        else if(Diff == "Hard")
            PhotonNetwork.LoadLevel("HardModegameScenes");
        else
            PhotonNetwork.LoadLevel("gameScenes"); // 房主切換場景，其他玩家會同步
    }
}
