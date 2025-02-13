using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct PublicPlayerData : INetworkSerializable, System.IEquatable<PublicPlayerData>
{
    public FixedString32Bytes username;
    public TeamType teamType;

    public bool Equals(PublicPlayerData other)
    {
        return username == other.username;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            FastBufferReader reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out username);
        }
        else
        {
            FastBufferWriter writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(username);
        }
    }
}
