using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{

    public float timeRemaining = 1;
    public bool timeIsRunning = true;
    public TMP_Text timeText;

    // Start is called before the first frame update
    void Start()
    {
        // Starts the timer
        timeIsRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeIsRunning)
        {
            if (timeRemaining >= 0) 
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }

        }
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float minuets = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("{0:00} : {1:00}", minuets, seconds);
    }
}
