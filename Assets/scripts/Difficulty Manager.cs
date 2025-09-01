using UnityEngine;
using Photon.Pun;

public class DifficultyManager : MonoBehaviourPunCallbacks
{
    public static DifficultyManager Instance { get; private set; }

    private string difficulty = "Normal"; // 預設難度

    private void Awake()
    {
        // 如果已經有一個 DifficultyManager，就刪掉新生成的
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 切換場景時不銷毀
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
