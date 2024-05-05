using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeEffect : MonoBehaviour
{
    [SerializeField] public Transform circleOne;
    [SerializeField] public Transform circleTwo;

    public void Update()
    {
        circleOne.Rotate(new Vector3(0,0,1), 1.0f);
        circleTwo.Rotate(new Vector3(0,0,1), 1.0f);
    }

    public void Charge(bool charging)
    {
        
    }
}
