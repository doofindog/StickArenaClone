using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("health Bar")] 
    [SerializeField] private Image healthBar;

    public void OnEnable()
    {
        PlayerEvents.DamageTakenEvent += DecreaseBar;
    }

    public void OnDisable()
    {
        PlayerEvents.DamageTakenEvent -= DecreaseBar;
    }

    private void DecreaseBar(CharacterDataHandler playerData,float damageTaken)
    {
        float fillRation = playerData.health.Value / playerData.maxHealth.Value;
        healthBar.fillAmount = fillRation;
    }
}
