using System;
using UnityEditor;


    public class BatchBuild
    {
        public static void DoBuildCompressed()
        {
            BuildPlayerOptions defaultOptions = new BuildPlayerOptions();
            
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Embedded;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL , ManagedStrippingLevel.Low);
            var buildDate = DateTime.Today.ToString("yyyy.M.dd");
            buildDate += "-DEV";
            Console.WriteLine($"Beginning build number {buildDate}");
            PlayerSettings.bundleVersion = buildDate;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            
            BuildPlayerOptions options = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(defaultOptions);
            BuildPipeline.BuildPlayer(options);
        }

        public static void DoBuild()
        {
            Console.WriteLine("WTFGO");
            BuildPlayerOptions defaultOptions = new BuildPlayerOptions();
            
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Embedded;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL , ManagedStrippingLevel.Low);
            var buildDate = DateTime.Today.ToString("yyyy.M.dd");
            buildDate += "-DEV";
            Console.WriteLine($"Beginning build number {buildDate}");
            PlayerSettings.bundleVersion = buildDate;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            
            BuildPlayerOptions options = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(defaultOptions);
            BuildPipeline.BuildPlayer(options);
        }

        public static void DoBuildRelease()
        {
            BuildPlayerOptions defaultOptions = new BuildPlayerOptions();
            
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithoutStacktrace;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.External;
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL , ManagedStrippingLevel.Medium);
            var buildDate = DateTime.Today.ToString("yyyy.M.dd");
            buildDate += "-PUBLIC";
            Console.WriteLine($"Beginning build number {buildDate}");
            PlayerSettings.bundleVersion = buildDate;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            
            BuildPlayerOptions options = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(defaultOptions);
            BuildPipeline.BuildPlayer(options);
        }
    }