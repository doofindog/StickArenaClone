using UnityEngine;

public class GiveCrownEffect : Effect
{
    public override void AddEffectToPlayer(ulong clientId)
    {
        ScoreManager.Instance.AddScore(clientId);
    }
}
