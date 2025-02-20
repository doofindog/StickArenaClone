using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using Unity.Netcode;
using Unity.Multiplayer.Playmode;

public class EditorUtilt : MonoBehaviour
{
    [MenuItem("Game/Setup Server")]
    public static void SetupServer()
    {
        AddDefineSymbol("SERVER");
        RemoveDefineSymbol("CLIENT");
    }

    [MenuItem("Game/Setup Client")]
    public static void SetupClient()
    {
        AddDefineSymbol("CLIENT");
        RemoveDefineSymbol("SERVER");
    }

    [MenuItem("Game/Recompile")]
    public static void Recompile()
    {
        CompilationPipeline.RequestScriptCompilation();
    }


    [MenuItem("Game/Compile Client Build")]
    public static void CompileClientBuild()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Main Scene.unity" };
        buildPlayerOptions.locationPathName = "builds/Client Build";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows;
        buildPlayerOptions.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    [MenuItem("Game/Compile Server Build")]
    public static void CompileServerBuild()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Main Scene.unity" };
        buildPlayerOptions.locationPathName = "builds/Server Build";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows;
        buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;
        buildPlayerOptions.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    public static void AddDefineSymbol(string pSymbol)
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        if (!defines.Contains(pSymbol))
        {
            defines += $";{pSymbol}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            Debug.Log($"Added Define Symbol: {pSymbol}");
        }
        else
        {
            Debug.Log($"Define Symbol '{pSymbol}' already exists.");
        }
    }

    public static void RemoveDefineSymbol(string pSymbol)
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        if (defines.Contains(pSymbol))
        {
            defines = defines.Replace(pSymbol, "").Replace(";;", ";").Trim(';');
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            Debug.Log($"Removed Define Symbol: {pSymbol}");
        }
        else
        {
            Debug.Log($"Define Symbol '{pSymbol}' not found.");
        }
    }


    public static string[] GetMultiplayTags()
    {
        return CurrentPlayer.ReadOnlyTags();
    }
}
