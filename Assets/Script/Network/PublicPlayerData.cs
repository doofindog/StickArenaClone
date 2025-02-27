using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct PublicPlayerData : INetworkSerializable, System.IEquatable<PublicPlayerData>
{
    public bool isNull;
    public bool isConnected;
    public ulong clientID;
    public NetworkObjectReference networkObject;
    public FixedString32Bytes username;
    public TeamType teamType;

    public static PublicPlayerData NullableData
    {
        get
        {
            return new PublicPlayerData()
            {
                isNull = true
            };
        }
    }

    public bool Equals(PublicPlayerData other)
    {
        return username == other.username;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref isNull);
        serializer.SerializeValue(ref isConnected);
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref username);
        serializer.SerializeValue(ref teamType);
    }
}
