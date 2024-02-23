using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor.ShaderKeywordFilter;
using Valve.VR.InteractionSystem;

public class Score : MonoBehaviour
{
    public int score;
    Timer timer;
    public GameObject timerGameObject;
    public TMP_Text scoreText;
    HoverButton button;
    public GameObject MovingPart;
    public GameObject buttonPressed;

    // Awake activates anything before the sene loads
    void Awake()
    {
        timer = timerGameObject.transform.GetComponent<Timer>();
        button = MovingPart.transform.GetComponent<HoverButton>();
    }

    // Update is called once per frame
    void Update()
    {
        if (button.buttonDown)
        {
            score = 0;
        }
    }
    public void AddScore(int score)
    {
        score += 1;
        DisplayScore(score);
    }

    public void DisplayScore(int scoreToDisplay)
    {
        scoreText.text = "Score:" + scoreToDisplay;
    }
}
