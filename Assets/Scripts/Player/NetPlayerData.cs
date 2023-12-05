using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct NetInputPayLoad : INetworkSerializable
{
    public int tick;
    public Vector3 direction;
    public float aimAngle;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            FastBufferReader reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out tick);
            reader.ReadValueSafe(out direction);
            reader.ReadValueSafe(out aimAngle);
        }
        else
        {
            FastBufferWriter writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(tick);
            writer.WriteValueSafe(direction);
            writer.WriteValueSafe(aimAngle);
        }
    }
}

[System.Serializable]
public struct NetStatePayLoad : INetworkSerializable
{
    public int tick;
    public Vector3 position;
    public float aimAngle;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            FastBufferReader reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out tick);
            reader.ReadValueSafe(out position);
            reader.ReadValueSafe(out aimAngle);
        }
        else
        {
            FastBufferWriter writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(tick);
            writer.WriteValueSafe(position);
            writer.WriteValueSafe(aimAngle);
        }
    }
}
