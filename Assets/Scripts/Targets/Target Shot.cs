using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetShot : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            Destroy(gameObject);
        }
    }
}
