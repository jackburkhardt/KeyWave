using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Project.Editor.Scripts
{
    public class VersionTracker : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var buildDate = DateTime.Today.ToString("yyyy.M.dd");
            Debug.Log($"Beginning build number {buildDate}");
            PlayerSettings.bundleVersion = buildDate;
            PlayerSettings.SplashScreen.showUnityLogo = false;
        }
    }
}