using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreControll : MonoBehaviour
{
    public RectTransform[] value;
    public TMP_Text ScoreText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetScore(int[] score)
    {
        float scale=0f;
        float[] maxscore = new float[] {20f,20f,15f,15f,20f,10f};
        for (int i= 0; i < 6; i++)
        {
            scale = score[i] / maxscore[i];
            value[i].localScale = new Vector3(scale, 1f, 1f);
        }
        ScoreText.text = score[6].ToString()+"%";
    }
}
