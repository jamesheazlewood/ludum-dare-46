using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDelete : MonoBehaviour
{
    public float liveTime = 2.0f;
    private float currentTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        if(currentTime >= liveTime) {
            Destroy(gameObject);
        }
    }
}
