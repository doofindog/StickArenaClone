using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct ClientData : IEquatable<ClientData>
{
    public ulong clientID;
    public Guid playerID;
    public PublicPlayerData publicPlayerData;
    public bool isNull;

    public bool Equals(ClientData other)
    {
        return clientID == other.clientID;
    }

    public static readonly ClientData NullableData = new ClientData()
    {
        clientID = 0,
        isNull = true
    };
}
