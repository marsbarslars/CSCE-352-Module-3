using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{

    public float timeRemaining = 3;
    public bool timeIsRunning;
    public TMP_Text timeText;

    // Start is called before the first frame update
    void Start()
    {
        // Starts the timer
        timeIsRunning = true; //TODO: Delete this line
    }

    // Update is called once per frame
    void Update()
    {
        // TODO:  if button pushed then start timer and target game
        //if ()
        //{
        //timeIsRunning = true;
        //}
        if (timeIsRunning)
        {
            if (timeRemaining >= 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                timeIsRunning = false;
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