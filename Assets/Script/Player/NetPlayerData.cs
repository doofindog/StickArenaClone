using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct NetInputPayLoad : INetworkSerializable
{
    public float time;
    public int tick;
    public int mousePosition;
    public Vector3 direction;
    public float aimAngle;
    public bool dodgePressed;
    public bool attackPressed;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            FastBufferReader reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out time);
            reader.ReadValueSafe(out tick);
            reader.ReadValueSafe(out mousePosition);
            reader.ReadValueSafe(out direction);
            reader.ReadValueSafe(out aimAngle);
            reader.ReadValueSafe(out dodgePressed);
            reader.ReadValueSafe(out attackPressed);
        }
        else
        {
            FastBufferWriter writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(time);
            writer.WriteValueSafe(tick);
            writer.WriteValueSafe(mousePosition);
            writer.WriteValueSafe(direction);
            writer.WriteValueSafe(aimAngle);
            writer.WriteValueSafe(dodgePressed);
            writer.WriteValueSafe(attackPressed);
        }
    }
}

[System.Serializable]
public struct NetStatePayLoad : INetworkSerializable
{
    public float time;
    public int tick;
    public Vector3 position;
    public float aimAngle;
    public bool dodge;
    public bool canDodge;
    public bool firedWeapon;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            FastBufferReader reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out time);
            reader.ReadValueSafe(out tick);
            reader.ReadValueSafe(out position);
            reader.ReadValueSafe(out aimAngle);
            reader.ReadValueSafe(out dodge);
            reader.ReadValueSafe(out canDodge);
            reader.ReadValueSafe(out firedWeapon);
        }
        else
        {
            FastBufferWriter writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(time);
            writer.WriteValueSafe(tick);
            writer.WriteValueSafe(position);
            writer.WriteValueSafe(aimAngle);
            writer.WriteValueSafe(dodge);
            writer.WriteValueSafe(canDodge);
            writer.WriteValueSafe(firedWeapon);
        }
    }
}
