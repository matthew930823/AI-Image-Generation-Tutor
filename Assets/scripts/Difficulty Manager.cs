using UnityEngine;
using Photon.Pun;

public class DifficultyManager : MonoBehaviourPunCallbacks
{
    public static DifficultyManager Instance { get; private set; }

    private string difficulty = "Normal"; // �w�]����

    private void Awake()
    {
        // �p�G�w�g���@�� DifficultyManager�A�N�R���s�ͦ���
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // ���������ɤ��P��
    }

    void Start()
    {
    }

    public string GetDifficulty()
    {
        return difficulty;
    }

    public void SetDifficulty(string newDiff)
    {
        difficulty = newDiff;
    }
}
