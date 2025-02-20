using System.Collections.Generic;
using Unity.Netcode;

public struct NetStateContainer : INetworkSerializable
{
    public List<NetStatePayLoad> netStateCollection;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Next, serialize the count of previous payloads.
        int count = netStateCollection != null ? netStateCollection.Count : 0;
        serializer.SerializeValue(ref count);

        // Then, serialize each previous payload.
        if (serializer.IsWriter)
        {
            for (int i = 0; i < count; i++)
            {
                netStateCollection[i].NetworkSerialize(serializer);
            }
        }
        else
        {
            // On the reader side, reinitialize the list and read each payload.
            netStateCollection = new List<NetStatePayLoad>(count);
            for (int i = 0; i < count; i++)
            {
                NetStatePayLoad payload = new NetStatePayLoad();
                payload.NetworkSerialize(serializer);
                netStateCollection.Add(payload);
            }
        }
    }
}
