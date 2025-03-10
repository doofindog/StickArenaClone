using Unity.Netcode;
using UnityEngine;

public class TestScriptDuplicate : MonoBehaviour
{

    public void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientStarted;
    }

    public void OnClientStarted(ulong clientID)
    {
        if(NetworkManager.Singleton.IsServer)
        {
            return;
        }

        ITestInterface testParent = GetComponent<ITestInterface>();
        if (testParent != null)
        {
            Debug.Log("Trying to send RPC + " + testParent.GetType().ToString());
            testParent.TestFunctionServerRPC();
        }
    }
}
