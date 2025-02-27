using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct NetInputPayLoad : INetworkSerializable
{
    public bool isNull;
    public int payloadSequence;
    public float time;
    public int tick;
    public int mousePosition;
    public Vector3 direction;
    public float aimAngle;
    public bool shootPressed;
    public List<NetInputPayLoad> previousPayloads;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref isNull);
        serializer.SerializeValue(ref payloadSequence);
        serializer.SerializeValue(ref time);
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref mousePosition);
        serializer.SerializeValue(ref direction);
        serializer.SerializeValue(ref aimAngle);
        serializer.SerializeValue(ref shootPressed);

        // Next, serialize the count of previous payloads.
        int count = previousPayloads != null ? previousPayloads.Count : 0;
        serializer.SerializeValue(ref count);

        // Then, serialize each previous payload.
        if (serializer.IsWriter)
        {
            for (int i = 0; i < count; i++)
            {
                previousPayloads[i].NetworkSerialize(serializer);
            }
        }
        else
        {
            // On the reader side, reinitialize the list and read each payload.
            previousPayloads = new List<NetInputPayLoad>(count);
            for (int i = 0; i < count; i++)
            {
                NetInputPayLoad payload = new NetInputPayLoad();
                payload.NetworkSerialize(serializer);
                previousPayloads.Add(payload);
            }
        }
    }
}

[System.Serializable]
public struct NetStatePayLoad : INetworkSerializable
{
    public int inputSequence;
    public float time;
    public int tick;
    public Vector3 position;
    public Vector3 positionDelta;
    public float aimAngle;
    public bool shootPressed;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref inputSequence);
        serializer.SerializeValue(ref time);
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref positionDelta);
        serializer.SerializeValue(ref aimAngle);
        serializer.SerializeValue(ref shootPressed);
    }
}
