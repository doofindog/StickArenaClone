using System.Linq;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    private static string[] filters = new string[]
    {
            DebugType.Defualt.ToString(),
            DebugType.UI.ToString(),
            DebugType.Game.ToString(),
            DebugType.Session.ToString(),
            DebugType.Network.ToString(),
    };

    public static void Log(string context, DebugType pEnum = DebugType.Defualt)
    {
#if DEBUGGER

        if (filters.Contains(pEnum.ToString()))
        {
            Debug.Log($"[{pEnum.ToString()}] :" + context);
        }
#endif
    }

    public enum DebugType
    {
        UI,
        Game,
        Session,
        Network,
        Defualt
    }
}
