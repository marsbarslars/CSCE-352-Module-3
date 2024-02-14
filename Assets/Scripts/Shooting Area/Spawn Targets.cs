using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTargets : MonoBehaviour

{
    public GameObject stationaryTarget;
    public GameObject movingTarget;
    public int chooseTarget;
    public int xPos;
    public int yPos;
    public int zPos;
    [SerializeField] Timer Timer;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TargetDrop());
    }

    IEnumerator TargetDrop()
    {
        while (Timer.timeremaining >= 0)
        {
            xPos = Random.Range(10, 0);
            yPos = Random.Range(6, 12);
            zPos = Random.Range(10, 20);
            //chooses which target to spawn stationaryTarget is 1 and movingTarget is 2
            chooseTarget = Random.Range(1, 2);
            if (chooseTarget == 1)
            {
                Instantiate(stationaryTarget, new Vector3(xPos, yPos, zPos), Quaternion.identity);
            }
            if (chooseTarget == 2)
            {
                Instantiate(movingTarget, new Vector3(xPos, yPos, zPos), Quaternion.identity);
            }

            yield return new WaitForSeconds(1f);
        }
    }

}
