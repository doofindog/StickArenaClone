using Unity.Netcode;

public class NetSessionData : INetworkSerializable
{
    public int blueScore;
    public int redScore;
    public int greenScore;
    public int yellowScore;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            FastBufferReader reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out blueScore);
            reader.ReadValueSafe(out redScore);
            reader.ReadValueSafe(out greenScore);
            reader.ReadValueSafe(out yellowScore);
        }
        else
        {
            FastBufferWriter writer = new FastBufferWriter();
            writer.WriteValueSafe(blueScore);
            writer.WriteValueSafe(redScore);
            writer.WriteValueSafe(greenScore);
            writer.WriteValueSafe(yellowScore);
        }
    }
}
