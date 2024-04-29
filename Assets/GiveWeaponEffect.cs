using Unity.Netcode;
using UnityEngine;

public class GiveWeaponEffect : Effect
{
    public GameObject weaponPrefab;

    public override void AddEffectToPlayer(ulong clientID)
    {
        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientID);
        WeaponComponent weaponComponent = netObj.GetComponent<WeaponComponent>();
        NetworkObject weaponNetObj = SpawnManager.Instance.SpawnObject(weaponPrefab, SpawnManager.SpawnType.NETWORK).GetComponent<NetworkObject>();
        weaponNetObj.Spawn();
        weaponComponent.EquipWeapon(weaponNetObj.GetComponent<Weapon>());
    }
}
