using UnityEngine;

public class Debugger : MonoBehaviour
{
    public static void Log(string context)
    {
#if DEBUGGER
        Debug.Log(context);
#endif
    }
}
