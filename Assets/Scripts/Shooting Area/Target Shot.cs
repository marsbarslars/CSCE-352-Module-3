using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class TargetShot : MonoBehaviour
{
    Score _score;
    public GameObject scoreGameObject;

    void Awake()
    {
        _score = scoreGameObject.transform.GetComponent<Score>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            Destroy(gameObject);
            _score.score += 1;
            _score.DisplayScore(_score.score);
        }
    }
}
