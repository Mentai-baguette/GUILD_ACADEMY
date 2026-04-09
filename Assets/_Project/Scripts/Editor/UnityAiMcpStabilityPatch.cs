#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GuildAcademy.EditorTools
{
    /// <summary>
    /// Applies a conservative MCP stability patch for Unity AI Assistant:
    /// keeps bridge enabled, but disables background process validation.
    /// </summary>
    [InitializeOnLoad]
    internal static class UnityAiMcpStabilityPatch
    {
        const string AutoApplyKey = "GuildAcademy.UnityAiMcp.AutoApplyStabilityPatch";

        static UnityAiMcpStabilityPatch()
        {
            // Default to enabled once for this project to reduce known MCP thread errors.
            if (!EditorPrefs.HasKey(AutoApplyKey))
            {
                EditorPrefs.SetBool(AutoApplyKey, true);
            }

            if (EditorPrefs.GetBool(AutoApplyKey, true))
            {
                EditorApplication.delayCall += ApplyStabilityPatch;
            }
        }

        [MenuItem("Tools/GuildAcademy/AI Assistant/Apply MCP Stability Patch")]
        static void ApplyStabilityPatchMenu()
        {
            ApplyStabilityPatch();
        }

        [MenuItem("Tools/GuildAcademy/AI Assistant/Restore MCP Default Validation")]
        static void RestoreDefaultsMenu()
        {
            SetProcessValidationEnabled(true);
        }

        [MenuItem("Tools/GuildAcademy/AI Assistant/Toggle Auto Apply Stability Patch")]
        static void ToggleAutoApplyMenu()
        {
            bool next = !EditorPrefs.GetBool(AutoApplyKey, true);
            EditorPrefs.SetBool(AutoApplyKey, next);
            Debug.Log($"[GuildAcademy] MCP stability auto-apply: {(next ? "ON" : "OFF")}");
        }

        [MenuItem("Tools/GuildAcademy/AI Assistant/Toggle Auto Apply Stability Patch", true)]
        static bool ToggleAutoApplyMenuValidate()
        {
            Menu.SetChecked(
                "Tools/GuildAcademy/AI Assistant/Toggle Auto Apply Stability Patch",
                EditorPrefs.GetBool(AutoApplyKey, true));
            return true;
        }

        static void ApplyStabilityPatch()
        {
            SetProcessValidationEnabled(false);
        }

        static void SetProcessValidationEnabled(bool enabled)
        {
            try
            {
                var managerType = Type.GetType("Unity.AI.MCP.Editor.Settings.MCPSettingsManager, Unity.AI.MCP.Editor");
                if (managerType == null)
                {
                    Debug.LogWarning("[GuildAcademy] MCPSettingsManager was not found. Unity AI Assistant package may not be loaded yet.");
                    return;
                }

                var settingsProperty = managerType.GetProperty(
                    "Settings",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (settingsProperty == null)
                {
                    Debug.LogWarning("[GuildAcademy] MCP Settings property was not found.");
                    return;
                }

                object settings = settingsProperty.GetValue(null);
                if (settings == null)
                {
                    Debug.LogWarning("[GuildAcademy] MCP settings instance is null.");
                    return;
                }

                var settingsType = settings.GetType();
                var validationField = settingsType.GetField(
                    "processValidationEnabled",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (validationField == null)
                {
                    Debug.LogWarning("[GuildAcademy] processValidationEnabled field was not found.");
                    return;
                }

                bool current = (bool)validationField.GetValue(settings);
                if (current == enabled)
                {
                    Debug.Log($"[GuildAcademy] MCP process validation already {(enabled ? "enabled" : "disabled")}.");
                    return;
                }

                validationField.SetValue(settings, enabled);

                var saveMethod = managerType.GetMethod(
                    "SaveSettings",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                saveMethod?.Invoke(null, null);

                Debug.Log($"[GuildAcademy] MCP process validation set to: {enabled}." +
                          " This reduces background validation thread errors while keeping bridge enabled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GuildAcademy] Failed to patch MCP settings: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
#endif
