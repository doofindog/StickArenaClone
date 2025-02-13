using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode]
public class TvController : MonoBehaviour
{
    [SerializeField] private GameObject m_uiObj;
    [SerializeField] private Image m_image;
    [SerializeField] private float m_height;
    [SerializeField] private float m_width;
    [SerializeField] private Animator m_animator;
    private Action onTurnOnComplete;
    private Action onTurnOffComplete;

    public void Awake()
    {
        if (m_uiObj != null)
        {
            m_uiObj.SetActive(true);
            m_animator = m_uiObj.GetComponent<Animator>();
        }
    }

    public void TurnOff(Action pOnTurnOffComplete = null)
    {
        this.onTurnOffComplete = pOnTurnOffComplete;
        m_animator.Play("TurnOff");
    }

    public void TurnOn(Action pOnTurnOnComplete = null, int delay = 0)
    {
        if(delay != 0 )
        {
            StartCoroutine(TurnOnDelayed(delay, pOnTurnOnComplete));
            return;
        }

        this.onTurnOnComplete = pOnTurnOnComplete;
        m_animator.Play("TurnOn");
    }

    private IEnumerator TurnOnDelayed(int delay, Action pOnCompleted = null)
    {
        yield return new WaitForSeconds(delay);

        TurnOn(pOnCompleted);
    }

    public void OnAnimComplete()
    {
        onTurnOnComplete?.Invoke();
        onTurnOffComplete = null;
    }

    public void OffAnimCompleted()
    {
        onTurnOffComplete?.Invoke();
        onTurnOffComplete = null;
    }

    public void Update()
    {
        m_image.material.SetFloat("_height", m_height);
        m_image.material.SetFloat("_width", m_width);
    }
}
