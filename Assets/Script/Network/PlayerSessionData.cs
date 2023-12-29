using Unity.Multiplayer.Samples.BossRoom;

public class PlayerSessionData: ISessionPlayerData
{
    public void Reinitialize()
    {
        
    }

    public bool IsConnected { get; set; }
    public ulong ClientID { get; set; }
}
