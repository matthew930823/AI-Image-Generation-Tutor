using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public InputField roomInputField;  
    public Text roomCodeText; 
    public Text statusText; 

    private string generatedRoomCode;

    private string GenerateRoomCode()
    {
        return Random.Range(1000, 9999).ToString(); 
    }

    public void CreateRoom()
    {
        generatedRoomCode = GenerateRoomCode();
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(generatedRoomCode, roomOptions);
        roomCodeText.text = "房間號：" + generatedRoomCode; 
        statusText.text = "創建房間";
    }

    public void JoinRoom()
    {
        string roomName = roomInputField.text;
        if (string.IsNullOrEmpty(roomName)) return;

        PhotonNetwork.JoinRoom(roomName);
        statusText.text = "正在加入房間：" + roomName;
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "已加入房間：" + PhotonNetwork.CurrentRoom.Name;
        Debug.Log("玩家 " + PhotonNetwork.NickName + " 加入房間 " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel("SelectScene"); 
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = "創建房間失敗：" + message;
        CreateRoom(); 
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = "加入房間失敗：" + message;
    }
}