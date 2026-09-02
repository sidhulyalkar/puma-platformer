using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Wildbound.Editor
{
    public static class WildboundBuild
    {
        public const string Scene = "Assets/Scenes/Wildbound.unity";
        [MenuItem("Wildbound/Play Wildbound")]
        public static void Play()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(Scene); EditorApplication.isPlaying = true;
        }
        [MenuItem("Wildbound/Build WebGL")]
        public static void WebGL()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new InvalidOperationException("Install Web Build Support for this editor in Unity Hub.");
            string output = Environment.GetEnvironmentVariable("WILDBOUND_BUILD_PATH");
            if (string.IsNullOrWhiteSpace(output)) output = "Builds/WebGL";
            PlayerSettings.companyName = "Sid Hulyalkar"; PlayerSettings.productName = "Puma: Wildbound";
            PlayerSettings.WebGL.template = "PROJECT:Wildbound";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.runInBackground = false;
            PlayerSettings.defaultWebScreenWidth = 1280; PlayerSettings.defaultWebScreenHeight = 720;
            Directory.CreateDirectory(output);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { Scene }, locationPathName = output, target = BuildTarget.WebGL, options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("WebGL build failed: " + report.summary.result);
            Debug.Log("Wildbound WebGL build: " + output + " (" + report.summary.totalSize + " bytes)");
        }
    }
}
