using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class HitResponseData : INetworkSerializable
{
    [FormerlySerializedAs("serverTime")] public double hitTime;
    public int damage;
    public ulong sourceID;
    public ulong hitId;
    public Vector3 hitPosition;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            FastBufferReader reader =serializer.GetFastBufferReader();
            reader.ReadValueSafe(out hitTime);
            reader.ReadValueSafe(out damage);
            reader.ReadValueSafe(out sourceID);
            reader.ReadValueSafe(out hitId);
            reader.ReadValueSafe(out hitPosition);
        }
        else
        {
            FastBufferWriter writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(hitTime);
            writer.WriteValueSafe(damage);
            writer.WriteValueSafe(sourceID);
            writer.WriteValueSafe(hitId);
            writer.WriteValueSafe(hitPosition);
        }
    }
}
