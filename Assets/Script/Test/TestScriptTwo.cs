using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;

public class TestScriptTwo : TestParent
{
    public void Awake()
    {
        string[] multiplayTag = CurrentPlayer.ReadOnlyTags();
        if(multiplayTag.Contains("CLIENT"))
        {
            Destroy(this);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void TestFunctionServerRPC()
    {
        Debug.Log("Called in Test Function Two");
    }
}
