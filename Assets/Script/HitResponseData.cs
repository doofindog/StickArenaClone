using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class HitResponseData : INetworkSerializable
{
    public int damage;
    public ulong sourceID;
    public ulong hitId;
    public Vector3 hitPosition;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            FastBufferReader reader =serializer.GetFastBufferReader();
            reader.ReadValueSafe(out damage);
            reader.ReadValueSafe(out sourceID);
            reader.ReadValueSafe(out hitId);
            reader.ReadValueSafe(out hitPosition);
        }
        else
        {
            FastBufferWriter writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(damage);
            writer.WriteValueSafe(sourceID);
            writer.WriteValueSafe(hitId);
            writer.WriteValueSafe(hitPosition);
        }
    }
}
