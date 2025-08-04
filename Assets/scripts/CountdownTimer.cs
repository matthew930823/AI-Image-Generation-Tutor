using UnityEngine;
using UnityEngine.UI;
using TMPro; // 如果你使用 TextMeshPro，請加這行
using System.Collections.Generic;

public class CountdownTimer : MonoBehaviour
{
    private float timeRemaining = 180f; // 3 分鐘 = 180 秒
    public TextMeshProUGUI countdownText; // 使用 Text 就改成 Text countdownText;

    private bool timerIsRunning = false;

    public GameObject GameScreen;
    public GameObject ResultScreen;
    public Image GameImage;
    public Image ResultImage;
    public TMP_Text Score;

    public GameController gameController;
    public SelectionBox selectionBox;

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay();
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                UpdateTimerDisplay();
                OnTimerEnd();
            }
        }
    }

    public void StartTimer()
    {
        timerIsRunning = true;
        timeRemaining = 180f;
        StartCoroutine(gameController.ChainCoroutines());
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void OnTimerEnd()
    {
        Debug.Log("時間到！");
        // 你可以在這裡觸發遊戲結束、顯示畫面等動作
        int score = selectionBox.B.Count * 10;
        if (score >= 100)
        {
            gameController.voiceAudioPlayer.AudioPlay(7);
        }
        else if(score >= 60)
        {
            gameController.voiceAudioPlayer.AudioPlay(8);
        }
        else
        {
            gameController.voiceAudioPlayer.AudioPlay(9);
        }
        Score.text = "Score : " + score;
        ResultImage.sprite = GameImage.sprite;
        //gameController.voiceAudioPlayer.AudioPlay(10);
        ResultScreen.SetActive(true);
        GameScreen.SetActive(false);
        selectionBox.B = new List<string>();
    }
}
