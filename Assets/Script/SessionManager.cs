using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Multiplayer.Playmode;
using Unity.Netcode;

public class SessionManager : NetworkBehaviour
{
    public PublicPlayerData LocalPlayerData;
    public NetworkVariable<int> countDownTimer = new NetworkVariable<int>();
    public Dictionary<ulong, string> clientIdToPlayerID = new Dictionary<ulong, string>();
    public Dictionary<string, PublicPlayerData> publicPlayerDataCollection = new Dictionary<string, PublicPlayerData>();

    public static Action AllPlayersConnectedEvent;
    public static SessionManager Instance { get; private set; }

    private void HandleClientDisconnected(ulong pClientId)
    {

    }

    private void HandleClientConnected(ulong pClientId)
    {
        void HandleClient()
        {

        }

        void HandleServer()
        {
            PublicPlayerData[] playerDatas = GetAllPublicPlayerData();
            UpdateAllPlayerDataClientRpc(playerDatas);

            TryStartSession();
        }

#if SERVER
        HandleServer();

#elif CLIENT
        HandleClient();

#else

        var tags = CurrentPlayer.ReadOnlyTags();
        if (tags.Contains("CLIENT"))
        {
            HandleClient();
        }
        else if (tags.Contains("SERVER"))
        {
            HandleServer();
        }
#endif
    }

    private void TryStartSession()
    {
        if (GetConnectedClientCount() != GameManager.Instance.GetSessionSettings().maxConnections)
        {
            return;
        }

        AllPlayersConnectedEvent?.Invoke();
    }

    private void CleanData()
    {
        publicPlayerDataCollection.Clear();
    }

    private void Awake()
    {
        SessionManager.Instance = this;
    }

    public void Init()
    {
        NetworkManager.OnServerStarted += HandleServerStarted;
        NetworkManager.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        NetworkManager.OnServerStarted -= HandleServerStarted;
    }

    public void CreateNewSession()
    {
        CleanData();

        countDownTimer.Value = GameManager.Instance.GetSessionSettings().countDownTime;
    }

    public void EndSession()
    {
        CleanData();
    }

    public void HandleServerStarted()
    {
        SessionManager.Instance.CreateNewSession();
        countDownTimer.Value = GameManager.Instance.GetSessionSettings().countDownTime;
    }

    public PublicPlayerData GetPublicPlayerData(ulong pClientID)
    {
        if(!clientIdToPlayerID.ContainsKey(pClientID))
        {
            return PublicPlayerData.NullableData;
        }

        string playerId = clientIdToPlayerID[pClientID];
        return publicPlayerDataCollection[playerId];
    }

    public PublicPlayerData GetPublicPlayerData(string pPlayerID)
    {
        if (publicPlayerDataCollection == null || publicPlayerDataCollection.ContainsKey(pPlayerID))
        {
            return PublicPlayerData.NullableData;
        }

        return publicPlayerDataCollection[pPlayerID];
    }

    public PublicPlayerData[] GetAllPublicPlayerData()
    {
        if (publicPlayerDataCollection == null)
        {
            return Array.Empty<PublicPlayerData>();
        }

        return publicPlayerDataCollection.Values.ToArray();
    }

    public int GetConnectedClientCount()
    {
        return NetworkManager.ConnectedClients.Count;
    }

    public void AddClientToSession(PublicPlayerData pPublicData)
    {
        if (publicPlayerDataCollection == null)
        {
            publicPlayerDataCollection = new Dictionary<string, PublicPlayerData>();
        }

        string playerID = pPublicData.username.ToString();
        if (DuplicateConnectionCheck(playerID))
        {
            //player Already Connected
            return;
        }

        bool reconnection = CheckReconnection(playerID);
        if (reconnection)
        {
            ulong newClinetID = pPublicData.clientID;

            pPublicData = GetPublicPlayerData(playerID);
            pPublicData.isConnected = true;
            pPublicData.clientID = newClinetID;
        }

        clientIdToPlayerID[pPublicData.clientID] = pPublicData.username.ToString();
        publicPlayerDataCollection[pPublicData.username.ToString()] = pPublicData;
    }

    private bool DuplicateConnectionCheck(string pPlayerID)
    {
        return publicPlayerDataCollection.ContainsKey(pPlayerID) &&
            publicPlayerDataCollection[pPlayerID].isConnected == false;
    }

    private bool CheckReconnection(string pPlayerID)
    {
        return publicPlayerDataCollection.ContainsKey(pPlayerID) &&
            publicPlayerDataCollection[pPlayerID].isConnected;
    }

    public void RemoveClientFromSession(string playerID)
    {
        if(publicPlayerDataCollection != null || publicPlayerDataCollection.ContainsKey(playerID) == false)
        {
            return;
        }

        PublicPlayerData playerData = publicPlayerDataCollection[playerID];
        playerData.isConnected = false;

        UpdatePublicPlayerData(playerID, playerData);
    }

    public void UpdatePublicPlayerData(string pPlayerID, PublicPlayerData pPlayerData)
    {
        if(publicPlayerDataCollection == null)
        {
            return;
        }

        publicPlayerDataCollection[pPlayerID] = pPlayerData;

        UpdatePlayerDataClientRPC(pPlayerData);
    }

    public void UpdatePublicPlayerData(PublicPlayerData pPlayerData)
    {
        if (publicPlayerDataCollection == null)
        {
            return;
        }

        string playerID = pPlayerData.username.ToString();
        publicPlayerDataCollection[playerID] = pPlayerData;

        UpdatePlayerDataClientRPC(pPlayerData);
    }


    #region ----------> RPC <----------

    [ClientRpc]
    private void UpdateAllPlayerDataClientRpc(PublicPlayerData[] pPlayerData)
    {
        publicPlayerDataCollection.Clear();

        foreach (PublicPlayerData playerData in pPlayerData)
        {
            publicPlayerDataCollection[playerData.username.ToString()] = playerData;
        }
    }

    [ClientRpc]
    private void UpdatePlayerDataClientRPC(PublicPlayerData pPlayerData)
    {
        publicPlayerDataCollection[pPlayerData.username.ToString()] = pPlayerData; 
    }
    #endregion
}
