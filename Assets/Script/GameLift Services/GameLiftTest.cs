using System;
using System.Collections.Generic;
using UnityEngine;
using Aws.GameLift.Server;
using Aws.GameLift;
using Aws.GameLift.Server.Model;

public class GameLiftTest : MonoBehaviour
{
    private int m_listenPort;
    private string m_webSocketUrl;
    private string m_processId;
    private string m_hostId;
    private string m_fleetId;
    private string m_authToken;
    private GenericOutcome m_terminationOutcome;
    private GenericOutcome m_initOutcome;

    private void Awake()
    {
        Initialise();
    }

    private void Initialise()
    {
        
        m_listenPort = CommandLineUtilt.GetArgument<int>("port");
        m_webSocketUrl = CommandLineUtilt.GetArgument<string>("webSocket");
        m_processId = CommandLineUtilt.GetArgument<string>("processId");
        m_hostId = CommandLineUtilt.GetArgument<string>("hostId");
        m_fleetId = CommandLineUtilt.GetArgument<string>("fleetId");
        m_authToken = CommandLineUtilt.GetArgument<string>("authToken");

        m_terminationOutcome = new GenericOutcome() { Success = false };
        m_initOutcome = new GenericOutcome() { Success = false };

        ServerParameters parameters = new ServerParameters()
        {
            WebSocketUrl = m_webSocketUrl,
            ProcessId = m_processId,
            HostId = m_hostId,
            FleetId = m_fleetId,
            AuthToken = m_authToken
        };

        m_initOutcome = GameLiftServerAPI.InitSDK(parameters);
        if (!m_initOutcome.Success)
        {
            Debug.Log("[AWS] AWS GameLift Initialisation Failed : "+ m_initOutcome.Error.ToString());
            return;
        }

        ProcessParameters processParameters = new ProcessParameters()
        {
            OnStartGameSession = OnStartGameSession,
            OnUpdateGameSession = OnUpdateGameSession,
            OnProcessTerminate = OnProcessTerminate,
            OnHealthCheck = OnHealthCheck,
            Port = m_listenPort
        };

        GenericOutcome processOutcome = GameLiftServerAPI.ProcessReady(processParameters);
        string processLog = processOutcome.Success ? "Process Outcome Success" : $"Process Outcome Failed : {processOutcome.Error.ToString()}";
        Debug.Log("[AWS] " + processLog);
    }

    private void OnStartGameSession(GameSession pGameSession)
    {
        Debugger.Log("[AWS] Game Session Initialising");
        GameLiftServerAPI.ActivateGameSession();
    }

    private void OnUpdateGameSession(UpdateGameSession pUpdateGameSession)
    {

    }

    private void OnProcessTerminate()
    {
        Debugger.Log("[AWS] Game Server Processing Terminating");
        m_terminationOutcome = GameLiftServerAPI.ProcessEnding();
    }

    private bool OnHealthCheck()
    {
        Debugger.Log("[AWS] Performing Health Check");
        return true;
    }

    private void OnApplicationQuit()
    {
        if(m_initOutcome == null || m_initOutcome.Success == false)
        {
            return;
        }

        OnProcessTerminate();
        GameLiftServerAPI.Destroy();
    }
}
