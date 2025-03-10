using System;
using Unity.Netcode;

public interface IDamageableEntity 
{
    public Action<HitResponseData> TakeDamageEvent { get; set; }
    public void TakeDamage(HitResponseData hitResponseData);
    public void TakeDamageServerRPC(HitResponseData hitResponseData);
}
