using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTarget : MonoBehaviour
{

    public float speed = 5.0f;
    private int direction = 1; //positive to start

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        float startingLocation = GameObject.Find("Moving Target").transform.position.x;
        float endingLocation = startingLocation + 10;
        float height = GameObject.Find("Moving Target").transform.position.y;
        float zNew = transform.position.z +
                     direction * speed * Time.deltaTime;
        if (zNew >= endingLocation)
        {
            zNew = endingLocation;
            direction *= -1;
        }
        else if (zNew <= startingLocation)
        {
            zNew = startingLocation;
            direction *= -1;
        }
        transform.position = new Vector3(startingLocation, height, zNew);
    }
}
