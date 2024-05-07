using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeEffect : MonoBehaviour
{
    [SerializeField] public Transform circleOne;
    [SerializeField] public Transform circleTwo;
    [SerializeField] public SpriteRenderer circleOneSprite;
    [SerializeField] public SpriteRenderer circleTwoSprite;

    public void Start()
    {
        circleOneSprite = circleOne.GetComponent<SpriteRenderer>();
        circleTwoSprite = circleTwo.GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        circleOne.Rotate(new Vector3(0,0,1), 1.0f);
        circleTwo.Rotate(new Vector3(0,0,1), 1.0f);
    }

    public void Charge(float time, float maxRange)
    {
        float scale = 1 / maxRange;
        float aplha = time * scale;

        circleOneSprite.color = new Color(0.71f, 1f, 0.99f, aplha);
        circleTwoSprite.color = new Color(0.19f, 0.78f, 0.86f, aplha);
    }
}
