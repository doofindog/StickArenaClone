using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GiveHealthEffect : Effect
{
   [SerializeField] private int healthToGive;
   public override void AddEffectToPlayer(ulong clientID)
   { 
      NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientID);
      CharacterDataHandler dataHandler = networkObject.GetComponent<CharacterDataHandler>();
      dataHandler.health.Value +=  healthToGive;
   }
}
