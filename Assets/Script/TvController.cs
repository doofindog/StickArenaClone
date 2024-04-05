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
    private Action _onTurnOnComplete;
    private Action _onTurnOffComplete;


    public void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TurnOff(Action onTurnOffComplete = null)
    {
        this._onTurnOffComplete = onTurnOffComplete;
        animator.Play("TurnOff");
    }

    public void TurnOn(Action onTurnOnComplete = null)
    {
        this._onTurnOnComplete = onTurnOnComplete;
        animator.Play("TurnOn");
    }

    public void OnAnimComplete()
    {
        _onTurnOnComplete?.Invoke();
    }

    public void Update()
    {
        image.material.SetFloat("_height", height);
        image.material.SetFloat("_width", width);
    }
}
