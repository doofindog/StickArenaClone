using UnityEngine;

public class Debugger : MonoBehaviour
{
    public static void Log(string context)
    {
#if DEBUGGER
        string[] filters = new string[]
        {
            "COMMAND_ARGUMENTS",
            "AWS"
        };

        bool print = true;
        //for (int i = 0; i < filters.Length; i++)
        //{
        //    print = context.Contains(filters[i]);
        //}

        if (print)
        {
            Debug.Log(context);
        }
#endif
    }
}
