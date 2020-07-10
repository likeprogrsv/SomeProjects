using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mixer : MonoBehaviour
{
    public float zAngle;
    public GameObject mixer;

    void Awake()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        mixer.transform.Rotate(0, 0, zAngle, Space.Self);
    }
}
