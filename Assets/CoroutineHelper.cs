using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHelper : Singleton<CoroutineHelper>
{
    public void BeginCoroutine(IEnumerator method)
    {
        StartCoroutine(method);
    }
}
