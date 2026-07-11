using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class CIBuild
{
    public static void BuildAndroid()
    {
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

        var keystoreName = Environment.GetEnvironmentVariable("ANDROID_KEYSTORE_NAME");
        var keystorePass = Environment.GetEnvironmentVariable("ANDROID_KEYSTORE_PASS");
        if (!string.IsNullOrEmpty(keystoreName) && !string.IsNullOrEmpty(keystorePass))
        {
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystoreName;
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasName = Environment.GetEnvironmentVariable("ANDROID_KEYALIAS_NAME");
            PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("ANDROID_KEYALIAS_PASS");
            Debug.Log("CIBuild: signing with custom keystore " + keystoreName);
        }

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        Debug.Log("CIBuild: building scenes: " + string.Join(", ", scenes));

        // Development build to match upstream releases: enables the in-game debug
        // console (4-finger tap), the only remaining source of hard currency.
        var report = BuildPipeline.BuildPlayer(scenes, "Build/SvZ2-arm64.apk", BuildTarget.Android, BuildOptions.Development);
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("CIBuild: build failed: " + report.summary.result);
            EditorApplication.Exit(1);
        }
        Debug.Log("CIBuild: build succeeded");
    }
}
