using Amazon.Auth.AccessControlPolicy;
using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommandLineUtilt
{
    private static Dictionary<string, string> arguments = new Dictionary<string, string>();

    static CommandLineUtilt()
    {
        InitialiseArgs();
    }

    private static void InitialiseArgs()
    {
        string[] args = Environment.GetCommandLineArgs();

        for(int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("-"))
            {
                string key = args[i].TrimStart('-');
                bool condition = i + 1 < args.Length && !args[i + 1].StartsWith("-");
                arguments[key] = condition ? args[i + 1] : string.Empty;

                Debugger.Log($"[COMMAND_ARGUMENTS] {args[i]} added");
            }
        }
    }

    public static T GetArgument<T>(string key)
    {
        if(arguments.TryGetValue(key, out string value))
        {
            try
            {
                Debugger.Log($"[COMMAND_ARGUMENTS] Geting Arugment [{key}] : {value}");
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                Debugger.Log($"[COMMAND_ARGUMENTS] Error : {typeof(T)} ,{ex.Message}");
            }
        }

        return default;
    }
};
