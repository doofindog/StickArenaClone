using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class PreGameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Animator timerAnim;

    private void Start()
    {
        GameManager.Instance.startGameTimer.OnValueChanged += (value, newValue) =>
        {
            timerAnim.Play("Timer");
            countText.text = newValue.ToString(CultureInfo.InvariantCulture);
            if (newValue <= 0)
            {
                gameObject.SetActive(false);
            }
        };
    }
}
