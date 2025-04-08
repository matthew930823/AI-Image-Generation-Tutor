using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine;

public class DifficultyManager : MonoBehaviourPunCallbacks
{
    private string difficulty;
    public GameController gameController;
    void Start()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Difficulty", out object diff))
        {
            difficulty = diff.ToString();
        }
        else
        {
            difficulty = "Normal"; // �w�]������
        }

        Debug.Log($"��e������: {difficulty}");
        gameController.ApplyDifficultySettings(difficulty);
    }
}
