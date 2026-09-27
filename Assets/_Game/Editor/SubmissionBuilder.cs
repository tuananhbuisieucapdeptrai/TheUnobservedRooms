using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnobservedRooms.Data;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UnobservedRooms.Editor
{
    public static class SubmissionBuilder
    {
        [MenuItem("Tools/The Unobserved Rooms/Build Submission Packages")]
        public static void BuildAll()
        {
            ValidateRelease();
            var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
            var root=Path.GetFullPath(Path.Combine(Application.dataPath,"../SubmissionBuilds")); Directory.CreateDirectory(root);
            var webGl = Path.Combine(root,"WebGL");
            Build(scenes,BuildTarget.WebGL,webGl);
            Zip(webGl,Path.Combine(root,"TheUnobservedRooms-WebGL.zip"));
            Build(scenes,BuildTarget.StandaloneWindows64,Path.Combine(root,"Windows","TheUnobservedRooms.exe"));
            Zip(Path.Combine(root,"Windows"),Path.Combine(root,"TheUnobservedRooms-Windows.zip"));
            AssetDatabase.Refresh();
            if (!Application.isBatchMode) EditorUtility.RevealInFinder(root);
        }

        [MenuItem("Tools/The Unobserved Rooms/Validate Release")]
        public static void ValidateRelease()
        {
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToArray();
            if (scenes.Length == 0) throw new System.Exception("No enabled scenes are configured in Build Settings.");
            foreach (var scene in scenes)
                if (!File.Exists(scene.path)) throw new System.Exception($"Build scene is missing: {scene.path}");

            var fallback = Resources.Load<TextAsset>("RunFallbacks/valid_demo");
            if (fallback == null) throw new System.Exception("The embedded fallback run is missing.");
            var run = JsonConvert.DeserializeObject<RunDefinition>(fallback.text);
            var validation = new RunValidator().Validate(run);
            if (!validation.IsValid) throw new System.Exception($"Fallback run is invalid: {validation.Summary}");
            foreach (var warning in validation.Warnings) Debug.LogWarning($"Release validation: {warning}");
            Debug.Log($"RELEASE VALIDATION PASSED: {scenes.Length} scenes and run '{run.RunId}' ({run.Level.Rooms.Count} rooms).");
        }

        private static void Build(string[] scenes,BuildTarget target,string location)
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=scenes,target=target,locationPathName=location,options=BuildOptions.CleanBuildCache });
            if (report.summary.result != BuildResult.Succeeded) throw new System.Exception($"{target} build failed with {report.summary.totalErrors} errors.");
            Debug.Log($"Submission build ready: {location}");
        }

        private static void Zip(string source,string destination)
        {
            if (File.Exists(destination)) File.Delete(destination);
            System.IO.Compression.ZipFile.CreateFromDirectory(source,destination,System.IO.Compression.CompressionLevel.Optimal,false);
        }
    }
}
