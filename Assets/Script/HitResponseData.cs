using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class HitResponseData : INetworkSerializable
{
    public float hitTime;
    public int damage;
    public ulong sourceID;
    public ulong hitId;
    public Vector3 projectileDirection;
    public Vector3 hitPosition;
    public Vector3 traceStart;
    public Quaternion projectileRotation;
    public float hitVelocity;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            FastBufferReader reader =serializer.GetFastBufferReader();
            reader.ReadValueSafe(out hitTime);
            reader.ReadValueSafe(out damage);
            reader.ReadValueSafe(out sourceID);
            reader.ReadValueSafe(out hitId);
            reader.ReadValueSafe(out projectileDirection);
            reader.ReadValueSafe(out hitPosition);
            reader.ReadValueSafe(out traceStart);
            reader.ReadValueSafe(out projectileRotation);
            reader.ReadValueSafe(out hitVelocity);
        }
        else
        {
            FastBufferWriter writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(hitTime);
            writer.WriteValueSafe(damage);
            writer.WriteValueSafe(sourceID);
            writer.WriteValueSafe(hitId);
            writer.WriteValueSafe(projectileDirection);
            writer.WriteValueSafe(hitPosition);
            writer.WriteValueSafe(traceStart);
            writer.WriteValueSafe(projectileRotation);
            writer.WriteValueSafe(hitVelocity);
        }
    }
}
