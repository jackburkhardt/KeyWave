using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

    public static class BatchBuild
    {
        public static string GetFolderArg()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-outputPath")
                {
                    if (i + 1 < args.Length)
                    {
                        Debug.Log($"Building to output path {args[i + 1]}");
                        return args[i + 1];
                    }
                }
            }
            return null;
        }

        public static BuildPlayerOptions GetDefaultBuildOptions()
        {
            return new BuildPlayerOptions()
            {
                locationPathName = GetFolderArg(),
                target = BuildTarget.WebGL,
                scenes = EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => scene.path)
                    .ToArray()
            };
        }
        
        public static void DoBuildCompressed()
        {
            BuildPlayerOptions options = GetDefaultBuildOptions();
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Embedded;
            EditorUserBuildSettings.il2CppCodeGeneration = Il2CppCodeGeneration.OptimizeSize;
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.WebGL, Il2CppCompilerConfiguration.Release);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL , ManagedStrippingLevel.Low);
            var buildDate = DateTime.Today.ToString("yyyy.M.dd");
            buildDate += "-DEV";
            Debug.Log($"Beginning build number {buildDate}");
            PlayerSettings.bundleVersion = buildDate;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            
            BuildPipeline.BuildPlayer(options);
        }

   
        public static void DoBuild()
        {
            BuildPlayerOptions options = GetDefaultBuildOptions();
            
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Embedded;
            EditorUserBuildSettings.il2CppCodeGeneration = Il2CppCodeGeneration.OptimizeSize;
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.WebGL, Il2CppCompilerConfiguration.Release);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL , ManagedStrippingLevel.Low);
            var buildDate = DateTime.Today.ToString("yyyy.M.dd");
            buildDate += "-DEMO";
            Debug.Log($"Beginning build number {buildDate}");
            PlayerSettings.bundleVersion = buildDate;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            
            BuildPipeline.BuildPlayer(options);
        }

        public static void DoBuildRelease()
        {
            BuildPlayerOptions options = GetDefaultBuildOptions();
            
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithoutStacktrace;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
            EditorUserBuildSettings.il2CppCodeGeneration = Il2CppCodeGeneration.OptimizeSize;
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.WebGL, Il2CppCompilerConfiguration.Release);
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL , ManagedStrippingLevel.Low);
            var buildDate = DateTime.Today.ToString("yyyy.M.dd");
            buildDate += "-PUBLIC";
            Debug.Log($"Beginning build number {buildDate}");
            PlayerSettings.bundleVersion = buildDate;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            
            BuildPipeline.BuildPlayer(options);
        }
    }

