using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class SpawnTargets : MonoBehaviour

{
    public GameObject stationaryTarget;
    public int xPos;
    public int yPos;
    public int zPos;
    Timer timer;
    public GameObject timerGameObject;
    public int t = 0;
    private List<GameObject> targetList = new List<GameObject>();

    void Awake()
    {
        timer = timerGameObject.transform.GetComponent<Timer>();
    }

    void FixedUpdate()
    {
        if (timer.timeIsRunning && (Time.timeSinceLevelLoad * 10) % 10 == 0)
        {
            xPos = Random.Range(-29, 29);
            yPos = Random.Range(6, 12);
            zPos = Random.Range(-29, 29);
            GameObject newTarget = Instantiate(stationaryTarget, new Vector3(xPos, yPos, zPos), Quaternion.identity);
            targetList.Add(newTarget);
        }

        if (timer.timeRemaining <= 0)
        {
            for (var i = 0; i < targetList.Count; i++)
            {
                Destroy(targetList[i]);
            }
        }
    }




}