using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartGameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Animator timerAnim;

    private void Start()
    {
        StartCoroutine(StartCountDown());
    }

    private IEnumerator StartCountDown()
    {
        SessionSettings settings = GameSessionManager.Singleton.GetSessionSettings();
        int timer = settings.gameBeingCountDown;
        countText.text = timer.ToString();
        while (timer > 0)
        {
            yield return new WaitForSeconds(1);
            timerAnim.Play("Timer");
            timer--;
            countText.text = timer.ToString();
        }
        
        gameObject.SetActive(false);
    }
}
