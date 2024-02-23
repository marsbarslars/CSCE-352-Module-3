using System.Collections;
using System.Collections.Generic;
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

    void Awake()
    {
        timer = timerGameObject.transform.GetComponent<Timer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TargetDrop());
    }

    IEnumerator TargetDrop()
    {
        while (timer.timeRemaining > 0)
        {
            xPos = Random.Range(10, 0);
            yPos = Random.Range(6, 12);
            zPos = Random.Range(10, 20);
            Instantiate(stationaryTarget, new Vector3(xPos, yPos, zPos), Quaternion.identity);


            yield return new WaitForSeconds(1f);
        }
    }

}