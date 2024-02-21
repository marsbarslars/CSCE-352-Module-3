using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor.ShaderKeywordFilter;

public class Score : MonoBehaviour
{
    public int score;
    Timer timer;
    public GameObject timerGameObject;
    public TMP_Text scoreText;

    // Awake activates anything before the sene loads
    void Awake()
    {
        timer = timerGameObject.transform.GetComponent<Timer>();
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: if button pushed then reset timer
        //if () 
        //{
        //score = 0;
        //}

    }

    public void AddScore(int score)
    {
        score += 1;
        DisplayScore(score);
    }

    void DisplayScore(float scoreToDisplay)
    {
        scoreText.text = string.Format("{00}", scoreToDisplay);
    }
}
