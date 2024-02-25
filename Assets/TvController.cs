using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TvController : MonoBehaviour
{
    public Image image;
    public float height;
    public float width;
    public Animator animator;
    public AudioClip cassetAudio;


    public void Awake()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(PlaySplash());
    }

    private IEnumerator PlaySplash()
    {
        yield return new WaitForSeconds(1);
        AudioManager.Instance.PlayOneShot(cassetAudio);
        yield return new WaitForSeconds(2);
        animator.Play("TurnOn");
    }

    public void Update()
    {
        image.material.SetFloat("_height", height);
        image.material.SetFloat("_width", width);
    }


    public void OnGameTurnOn()
    {
        GameEvents.SendGameOnComplete();
    }
}
