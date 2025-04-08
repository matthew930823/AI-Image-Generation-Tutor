using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public InputField roomInputField;  // 玩家输入房间号
    public Text roomCodeText;  // 显示房间号
    public Text statusText;  // 显示状态信息

    private string generatedRoomCode;

    // 生成随机房间号
    private string GenerateRoomCode()
    {
        return Random.Range(1000, 9999).ToString(); // 生成 4 位数随机房间号
    }

    // 创建随机房间
    public void CreateRoom()
    {
        generatedRoomCode = GenerateRoomCode();
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(generatedRoomCode, roomOptions);
        roomCodeText.text = "房間號：" + generatedRoomCode; // 显示房间号
        statusText.text = "創建房間";
    }

    // 加入已有房间
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
        PhotonNetwork.LoadLevel("SelectScene"); // 进入游戏场景
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = "創建房間失敗：" + message;
        CreateRoom(); // 失败时重新生成房间号
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = "加入房間失敗：" + message;
    }
}