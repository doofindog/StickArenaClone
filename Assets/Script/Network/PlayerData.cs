using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class PlayerData: INetworkSerializable
{
    public ulong clientID;
    public string userName;
    public bool isJoinSession;
    public bool isConnected;
    public TeamType teamType;
    public NetworkObject networkObject;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            FastBufferReader reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out clientID);
            reader.ReadValueSafe(out userName);
            reader.ReadValueSafe(out isJoinSession);
            reader.ReadValueSafe(out isConnected);
            reader.ReadValueSafe(out teamType);
        }
        else
        {            
            FastBufferWriter writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(clientID);
            writer.WriteValueSafe(userName);
            writer.WriteValueSafe(isJoinSession);
            writer.WriteValueSafe(isConnected);
            writer.WriteValueSafe(teamType);
        }
    }


    public void SetNetworkObject(NetworkObject obj)
    {
        networkObject = obj;
    }
}
