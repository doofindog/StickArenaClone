using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class DynamicTiles : NetworkBehaviour
{
    private Animator _anim;

    public void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void Reset()
    {
        _anim.Play("GroundClose");
    }
}
