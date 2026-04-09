#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace GuildAcademy.EditorTools
{
    internal static class FieldEditModeTestRunner
    {
        private const string ResultFilePath = "Logs/FieldEditModeTests.xml";
        private const string AssemblyName = "Tests.EditMode.Field";

        [MenuItem("Tools/GuildAcademy/Tests/Run Field EditMode Tests")]
        public static void RunFromMenu()
        {
            RunInternal();
        }

        public static void RunFromCommandLine()
        {
            RunInternal();
        }

        private static void RunInternal()
        {
            var resultDirectory = Path.GetDirectoryName(ResultFilePath);
            if (!string.IsNullOrEmpty(resultDirectory))
                Directory.CreateDirectory(resultDirectory);

            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            testRunnerApi.RegisterCallbacks(new Callbacks());

            var filter = new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { AssemblyName }
            };

            var executionSettings = new ExecutionSettings(filter)
            {
                runSynchronously = true
            };

            Debug.Log("[GuildAcademy] Running Field EditMode tests.");
            testRunnerApi.Execute(executionSettings);
        }

        private sealed class Callbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
                Debug.Log($"[GuildAcademy] Test run started: {testsToRun.Name}");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                TestRunnerApi.SaveResultToFile(result, ResultFilePath);
                Debug.Log($"[GuildAcademy] Test run finished. Status={result.TestStatus}, Passed={result.PassCount}, Failed={result.FailCount}");
                EditorApplication.Exit(result.FailCount == 0 ? 0 : 1);
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.TestStatus == TestStatus.Failed)
                    Debug.LogError($"[GuildAcademy] Failed {result.FullName}: {result.Message}");
            }
        }
    }
}
#endif