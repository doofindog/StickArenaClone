using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SessionData : INetworkSerializable
{
    public string hostName;
    public string joinCode;

    public SessionData()
    {
    }
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            FastBufferReader reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out hostName);
            reader.ReadValueSafe(out joinCode);
        }
        else
        {
            FastBufferWriter writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(hostName);
            writer.WriteValueSafe(joinCode);
        }
    }
}
