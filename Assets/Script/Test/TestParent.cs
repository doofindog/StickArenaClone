using Unity.Netcode;
using UnityEngine;


public class TestParent : NetworkBehaviour, ITestInterface
{
    [ServerRpc(RequireOwnership = false)]
    public virtual void TestFunctionServerRPC()
    {
        Debug.Log("Called Parent");
    }
}
