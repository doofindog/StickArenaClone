using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CommandProcessMonitor : MonoBehaviour
{
    private Process m_parentProcess;
    private void Awake()
    {
        //int parentProcessID = GetParentProcessId();
    }

    int GetParentProcessId()
    {
        using (var currentProcess = Process.GetCurrentProcess())
        {
            return 0; // Requires Process.Parent() from System.Diagnostics
        }
    }
}


