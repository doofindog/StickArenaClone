using UnityEngine;

public abstract class Effect : MonoBehaviour
{
    public virtual void AddEffectToPlayer(ulong clientID)
    {
        ScoreManager.Instance.AddScore(clientID);
    }
}
