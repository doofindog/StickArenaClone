using Unity.Netcode;

public interface IDamageableEntity 
{
    public void TakeDamage(HitResponseData hitResponseData);
}
