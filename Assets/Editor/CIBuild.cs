using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CIBuild
{
    public static void BuildAndroid()
    {
        var sdk = Environment.GetEnvironmentVariable("ANDROID_HOME");
        if (!string.IsNullOrEmpty(sdk)) EditorPrefs.SetString("AndroidSdkRoot", sdk);
        var ndk = Environment.GetEnvironmentVariable("ANDROID_NDK_ROOT");
        if (!string.IsNullOrEmpty(ndk)) EditorPrefs.SetString("AndroidNdkRoot", ndk);

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetDevice = AndroidTargetDevice.ARM64;

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        Debug.Log("CIBuild: building scenes: " + string.Join(", ", scenes));

        var error = BuildPipeline.BuildPlayer(scenes, "Build/SvZ2-arm64.apk", BuildTarget.Android, BuildOptions.None);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError("CIBuild: build failed: " + error);
            EditorApplication.Exit(1);
        }
        Debug.Log("CIBuild: build succeeded");
    }
}
