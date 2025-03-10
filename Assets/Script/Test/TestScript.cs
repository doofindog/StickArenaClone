using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;

public class TestScript : TestParent
{
    public void Awake()
    {
        string[] multiplayTag = CurrentPlayer.ReadOnlyTags();
        if (multiplayTag.Contains("SERVER"))
        {
            Destroy(this);
        }
    }
}
