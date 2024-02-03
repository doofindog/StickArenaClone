using Unity.Netcode;

public interface IDamageableEntity 
{
    public void TakeDamage(float damage, NetworkObject source);
}
