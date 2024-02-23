using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class Timer : MonoBehaviour
{

    public float timeRemaining = 180;
    public bool timeIsRunning;
    public TMP_Text timeText;
    HoverButton button;
    public GameObject MovingPart;
    public GameObject buttonPressed;

    void Awake()
    {
        button = MovingPart.transform.GetComponent<HoverButton>();
    }

    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
    
        if (button.buttonDown)
        {
            timeIsRunning = true;
        }
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