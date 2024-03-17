using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TvController : MonoBehaviour
{
    public Image image;
    public float height;
    public float width;
    public Animator animator;
    public Action onTurnOnComplete;


    public void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TurnOf()
    {
        animator.Play("TurnOff");
    }

    public void TurnOn(Action onTurnOnComplete)
    {
        this.onTurnOnComplete = onTurnOnComplete;
        animator.Play("TurnOn");
    }

    public void OnAnimComplete()
    {
        onTurnOnComplete?.Invoke();
    }

    public void Update()
    {
        image.material.SetFloat("_height", height);
        image.material.SetFloat("_width", width);
    }
}
