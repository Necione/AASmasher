using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using MelonLoader;

namespace AASmasher
{
    [Serializable]
    public class KeybindSettings
    {
        public KeyCode guiToggleKey = KeyCode.RightShift;
        public KeyCode actionKey = KeyCode.Semicolon;
        
        public KeybindSettings()
        {
            // Default values already set above
        }
    }
    
    public class SettingsManager
    {
        private KeybindSettings settings;
        private string settingsPath;
        
        // Events for when keybinds change
        public event Action<KeyCode> OnGuiToggleKeyChanged;
        public event Action<KeyCode> OnActionKeyChanged;
        
        public KeyCode GuiToggleKey => settings.guiToggleKey;
        public KeyCode ActionKey => settings.actionKey;
        
        // For the dropdown selection
        private readonly Dictionary<string, KeyCode> keyOptions = new Dictionary<string, KeyCode>
        {
            { "Right Shift", KeyCode.RightShift },
            { "Left Shift", KeyCode.LeftShift },
            { "F1", KeyCode.F1 },
            { "F2", KeyCode.F2 },
            { "F3", KeyCode.F3 },
            { "F4", KeyCode.F4 },
            { "F5", KeyCode.F5 },
            { "F6", KeyCode.F6 },
            { "Tab", KeyCode.Tab },
            { "Tilde (~)", KeyCode.BackQuote },
            { "Insert", KeyCode.Insert },
            { "Home", KeyCode.Home },
            { "End", KeyCode.End }
        };
        
        private readonly Dictionary<string, KeyCode> actionKeyOptions = new Dictionary<string, KeyCode>
        {
            { "Semicolon (;)", KeyCode.Semicolon },
            { "Quote (')", KeyCode.Quote },
            { "Left Bracket ([)", KeyCode.LeftBracket },
            { "Right Bracket (])", KeyCode.RightBracket },
            { "Backslash (\\)", KeyCode.Backslash },
            { "Comma (,)", KeyCode.Comma },
            { "Period (.)", KeyCode.Period },
            { "Slash (/)", KeyCode.Slash },
            { "F1", KeyCode.F1 },
            { "F2", KeyCode.F2 },
            { "F3", KeyCode.F3 },
            { "F4", KeyCode.F4 },
            { "F5", KeyCode.F5 },
            { "F6", KeyCode.F6 },
            { "Minus (-)", KeyCode.Minus },
            { "Equals (=)", KeyCode.Equals }
        };
        
        public string[] GuiToggleKeyOptions => new List<string>(keyOptions.Keys).ToArray();
        public string[] ActionKeyOptions => new List<string>(actionKeyOptions.Keys).ToArray();
        
        public SettingsManager()
        {
            settings = new KeybindSettings();
            
            // Set up the settings file path in the mod directory
            string modDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            settingsPath = Path.Combine(modDirectory, "AASmasher_Settings.json");
            
            LoadSettings();
        }
        
        public void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    var loadedSettings = JsonUtility.FromJson<KeybindSettings>(json);
                    if (loadedSettings != null)
                    {
                        settings = loadedSettings;
                        MelonLogger.Msg($"Loaded keybind settings: GUI Toggle = {settings.guiToggleKey}, Action = {settings.actionKey}");
                    }
                }
                else
                {
                    MelonLogger.Msg("No settings file found, using defaults");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to load settings: {ex.Message}");
                settings = new KeybindSettings(); // Use defaults
            }
        }
        
        public void SaveSettings()
        {
            try
            {
                string json = JsonUtility.ToJson(settings, true);
                File.WriteAllText(settingsPath, json);
                MelonLogger.Msg($"Saved keybind settings: GUI Toggle = {settings.guiToggleKey}, Action = {settings.actionKey}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to save settings: {ex.Message}");
            }
        }
        
        public void SetGuiToggleKey(KeyCode newKey)
        {
            if (settings.guiToggleKey != newKey)
            {
                settings.guiToggleKey = newKey;
                OnGuiToggleKeyChanged?.Invoke(newKey);
                SaveSettings();
            }
        }
        
        public void SetActionKey(KeyCode newKey)
        {
            if (settings.actionKey != newKey)
            {
                settings.actionKey = newKey;
                OnActionKeyChanged?.Invoke(newKey);
                SaveSettings();
            }
        }
        
        public string GetGuiToggleKeyDisplayName()
        {
            foreach (var kvp in keyOptions)
            {
                if (kvp.Value == settings.guiToggleKey)
                    return kvp.Key;
            }
            return settings.guiToggleKey.ToString();
        }
        
        public string GetActionKeyDisplayName()
        {
            foreach (var kvp in actionKeyOptions)
            {
                if (kvp.Value == settings.actionKey)
                    return kvp.Key;
            }
            return settings.actionKey.ToString();
        }
        
        public void SetGuiToggleKeyByName(string keyName)
        {
            if (keyOptions.TryGetValue(keyName, out KeyCode keyCode))
            {
                SetGuiToggleKey(keyCode);
            }
        }
        
        public void SetActionKeyByName(string keyName)
        {
            if (actionKeyOptions.TryGetValue(keyName, out KeyCode keyCode))
            {
                SetActionKey(keyCode);
            }
        }
        
        public int GetGuiToggleKeyIndex()
        {
            string displayName = GetGuiToggleKeyDisplayName();
            var options = GuiToggleKeyOptions;
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == displayName)
                    return i;
            }
            return 0; // Default to first option
        }
        
        public int GetActionKeyIndex()
        {
            string displayName = GetActionKeyDisplayName();
            var options = ActionKeyOptions;
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == displayName)
                    return i;
            }
            return 0; // Default to first option
        }
    }
}