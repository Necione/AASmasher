using UnityEngine;
using System;
using System.Linq;

namespace AASmasher
{
    public class GuiRenderer
    {
        private readonly GuiSystem guiSystem;
        private readonly InputHandler inputHandler;
        private readonly InvasionManager invasionManager;
        private readonly NightModeManager nightModeManager;
        private readonly ButtonAutomation buttonAutomation;
        private readonly CameraController cameraController;
        private readonly FatAliensManager fatAliensManager;
        private readonly SettingsManager settingsManager;
        
        private string yourXInput = "";
        private string yourYInput = "";
        private string minLevelInput = "";
        private string maxLevelInput = "";
        private string fatAliensLevelInput = "";
        
        public string YourXInput { get => yourXInput; set => yourXInput = value; }
        public string YourYInput { get => yourYInput; set => yourYInput = value; }
        public string MinLevelInput { get => minLevelInput; set => minLevelInput = value; }
        public string MaxLevelInput { get => maxLevelInput; set => maxLevelInput = value; }
        public string FatAliensLevelInput { get => fatAliensLevelInput; set => fatAliensLevelInput = value; }
        
        public GuiRenderer(GuiSystem guiSystem, InputHandler inputHandler, InvasionManager invasionManager, 
                          NightModeManager nightModeManager, ButtonAutomation buttonAutomation, CameraController cameraController, 
                          FatAliensManager fatAliensManager, SettingsManager settingsManager)
        {
            this.guiSystem = guiSystem;
            this.inputHandler = inputHandler;
            this.invasionManager = invasionManager;
            this.nightModeManager = nightModeManager;
            this.buttonAutomation = buttonAutomation;
            this.cameraController = cameraController;
            this.fatAliensManager = fatAliensManager;
            this.settingsManager = settingsManager;
        }
        
        public void DrawMenu(Action onCloseRequested)
        {
            guiSystem.InitializeStyles();
            
            GUI.enabled = true;
            if (GUI.Button(new Rect(0, 0, Screen.width, Screen.height), "", GUIStyle.none))
            {
                // Catch clicks outside close window
            }
            
            inputHandler.HandleGuiEvents(guiSystem.GuiRect);
            
            // Background
            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            // Auto scanning
            if (!invasionManager.InvasionLocationsScanned)
            {
                invasionManager.UpdateInvasionLocations();
                guiSystem.ShowSuccessMessage($"Found {invasionManager.InvasionLocations.Count} invasion locations");
            }
            
            // Drawing the main windows
            var newGuiRect = GUILayout.Window(1212, guiSystem.GuiRect, DrawWindowContents, "AASmasher", guiSystem.WindowStyle);
            guiSystem.UpdateGuiRect(newGuiRect);
            
            var newTogglesRect = GUILayout.Window(1213, guiSystem.TogglesRect, DrawTogglesWindow, "Toggles", guiSystem.WindowStyle);
            guiSystem.UpdateTogglesRect(newTogglesRect);
            
            var newSettingsRect = GUILayout.Window(1214, guiSystem.SettingsRect, DrawSettingsWindow, "Keybinds", guiSystem.WindowStyle);
            guiSystem.UpdateSettingsRect(newSettingsRect);
            
            DrawTitleBar(onCloseRequested);
            DrawKeyExplanation();
            DrawSuccessMessage();
            
            inputHandler.ResetInputAxesIfNeeded();
        }
        
        private void DrawTitleBar(Action onCloseRequested)
        {
            float titleBarPulse = Mathf.Sin(guiSystem.AnimationTime * 3) * 0.05f + 1f;
            GUI.color = new Color(titleBarPulse, titleBarPulse, titleBarPulse, 1f);
            GUI.Label(new Rect(guiSystem.GuiRect.x, guiSystem.GuiRect.y, guiSystem.GuiRect.width, 30), "AASmasher", guiSystem.TitleBarStyle);
            GUI.color = Color.white;
        }
        
        private void DrawKeyExplanation()
        {
            if (inputHandler.IsAnyTextFieldFocused)
            {
                GUI.color = new Color(1f, 1f, 0.7f, 0.9f); // Light yellow
                GUI.DrawTexture(new Rect(guiSystem.GuiRect.x, guiSystem.GuiRect.y - 25, guiSystem.GuiRect.width, 25), Texture2D.whiteTexture);
                
                GUI.color = Color.black;
                GUI.Label(new Rect(guiSystem.GuiRect.x, guiSystem.GuiRect.y - 25, guiSystem.GuiRect.width, 25), 
                    $"Press {settingsManager.GetGuiToggleKeyDisplayName().ToUpper()} to close the window", 
                    new GUIStyle(GUI.skin.label) { 
                        alignment = TextAnchor.MiddleCenter, 
                        fontStyle = FontStyle.Bold,
                        fontSize = 12
                    });
                GUI.color = Color.white;
            }
        }
        
        private void DrawSuccessMessage()
        {
            if (guiSystem.ShowingSuccessMessage)
            {
                float alpha = 1f;
                if (guiSystem.SuccessMessageTime > 2f)
                {
                    alpha = Mathf.Clamp01(3f - guiSystem.SuccessMessageTime);
                }
                
                GUI.color = new Color(0.2f, 0.8f, 0.2f, alpha * 0.9f);
                GUI.DrawTexture(new Rect(guiSystem.GuiRect.x, guiSystem.GuiRect.y + guiSystem.GuiRect.height + 5, guiSystem.GuiRect.width, 30), Texture2D.whiteTexture);
                
                // Success message text
                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.Label(new Rect(guiSystem.GuiRect.x + 10, guiSystem.GuiRect.y + guiSystem.GuiRect.height + 5, guiSystem.GuiRect.width - 20, 30), guiSystem.SuccessMessageText, 
                    new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
                GUI.color = Color.white;
            }
        }
        
        private void DrawWindowContents(int windowID)
        {
            // Handle window dragging
            HandleWindowDragging();
            
            GUILayout.BeginVertical();
            
            // Title and info
            GUILayout.Space(10);
            GUILayout.Label("Find and Navigate to Invasion Locations", guiSystem.HeaderStyle);
            GUILayout.Space(15);
            
            DrawPositionSection();
            DrawFilterSection();
            DrawActionButtons();
            DrawInvasionList();
            DrawStatusInfo();
            
            // Help text at the bottom
            GUILayout.Label("RightShift - Toggle GUI | Semicolon (;) - Cycle through invasions", 
                new GUIStyle(guiSystem.LabelStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 10, fontStyle = FontStyle.Italic });
            GUILayout.Label("Use 'Fat Aliens' in Toggles to make aliens at a specific level appear 2x larger", 
                new GUIStyle(guiSystem.LabelStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 9, fontStyle = FontStyle.Italic });
            
            GUILayout.EndVertical();
            
            // Make window draggable from the title bar area
            GUI.DragWindow(new Rect(0, 0, 10000, 30));
            
            HandleWindowEvents();
        }
        
        private void HandleWindowDragging()
        {
            if (Event.current != null && Event.current.type == EventType.MouseDrag)
            {
                guiSystem.SetDragging(true);
            }
            else if (Event.current != null && Event.current.type == EventType.MouseUp)
            {
                guiSystem.SetDragging(false);
            }
        }
        
        private void DrawPositionSection()
        {
            // Section: Your Position
            GUI.DrawTexture(GUILayoutUtility.GetRect(guiSystem.GuiRect.width - 20, 1), guiSystem.HeaderBackgroundTexture);
            GUILayout.Space(5);
            GUILayout.Label("Your Position", new GUIStyle(guiSystem.LabelStyle) { fontStyle = FontStyle.Bold });
            GUILayout.Space(5);
            
            // Current coordinates - with a more modern layout
            GUILayout.BeginHorizontal();
            
            // X coordinate
            GUILayout.Label("Your X:", guiSystem.LabelStyle, GUILayout.Width(70));
            GUI.SetNextControlName("XInput");
            yourXInput = GUILayout.TextField(yourXInput, guiSystem.InputFieldStyle, GUILayout.Width(70), GUILayout.Height(28));
            
            GUILayout.Space(15);
            
            // Y coordinate
            GUILayout.Label("Your Y:", guiSystem.LabelStyle, GUILayout.Width(70));
            GUI.SetNextControlName("YInput");
            yourYInput = GUILayout.TextField(yourYInput, guiSystem.InputFieldStyle, GUILayout.Width(70), GUILayout.Height(28));
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(15);
        }
        
        private void DrawFilterSection()
        {
            // Section: Filter Options
            GUI.DrawTexture(GUILayoutUtility.GetRect(guiSystem.GuiRect.width - 20, 1), guiSystem.HeaderBackgroundTexture);
            GUILayout.Space(5);
            GUILayout.Label("Filter Options", new GUIStyle(guiSystem.LabelStyle) { fontStyle = FontStyle.Bold });
            GUILayout.Space(5);
            
            // Level filter inputs - with a more modern layout
            GUILayout.BeginHorizontal();
            
            // Min level
            GUILayout.Label("Min Level:", guiSystem.LabelStyle, GUILayout.Width(70));
            GUI.SetNextControlName("MinLevelInput");
            minLevelInput = GUILayout.TextField(minLevelInput, guiSystem.InputFieldStyle, GUILayout.Width(70), GUILayout.Height(28));
            
            GUILayout.Space(15);
            
            // Max level
            GUILayout.Label("Max Level:", guiSystem.LabelStyle, GUILayout.Width(70));
            GUI.SetNextControlName("MaxLevelInput");
            maxLevelInput = GUILayout.TextField(maxLevelInput, guiSystem.InputFieldStyle, GUILayout.Width(70), GUILayout.Height(28));
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(15);
        }
        
        private void DrawActionButtons()
        {
            // Action buttons
            GUILayout.BeginHorizontal();
            
            // Refresh button
            if (GUILayout.Button("Refresh Invasions", guiSystem.ButtonStyle, GUILayout.Height(32)))
            {
                invasionManager.ForceRefresh();
                guiSystem.ShowSuccessMessage($"Found {invasionManager.InvasionLocations.Count} invasion locations");
            }
            
            GUILayout.Space(10);
            
            // Auto cycle button
            if (GUILayout.Button("Auto-Cycle (;)", guiSystem.ButtonStyle, GUILayout.Height(32)))
            {
                HandleSemicolonAction();
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.Space(15);
        }
        
        private void DrawInvasionList()
        {
            // Section: Invasion List
            GUI.DrawTexture(GUILayoutUtility.GetRect(guiSystem.GuiRect.width - 20, 1), guiSystem.HeaderBackgroundTexture);
            GUILayout.Space(5);
            
            // Get filtered invasions
            float.TryParse(yourXInput, out float userX);
            float.TryParse(yourYInput, out float userY);
            var filtered = invasionManager.GetFilteredInvasions(userX, userY, minLevelInput, maxLevelInput);
            
            GUILayout.Label($"Invasions Found: {filtered.Count}", new GUIStyle(guiSystem.LabelStyle) { fontStyle = FontStyle.Bold });
            GUILayout.Space(5);
            
            // Table header with highlight color
            GUI.color = Color.white;
            GUILayout.BeginHorizontal(GUILayout.Height(30));
            GUILayout.Label("Level", guiSystem.TableHeaderStyle, GUILayout.Width(90));
            GUILayout.Label("X", guiSystem.TableHeaderStyle, GUILayout.Width(90));
            GUILayout.Label("Y", guiSystem.TableHeaderStyle, GUILayout.Width(90));
            GUILayout.Label("Distance", guiSystem.TableHeaderStyle, GUILayout.Width(90));
            GUILayout.Label("Action", guiSystem.TableHeaderStyle, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            
            // Table content in scrollview
            var scrollPos = guiSystem.ScrollPosition;
            scrollPos = GUILayout.BeginScrollView(scrollPos, guiSystem.ScrollViewStyle, GUILayout.Height(320));
            guiSystem.ScrollPosition = scrollPos;
            
            if (filtered.Count == 0)
            {
                GUILayout.Space(20);
                GUILayout.Label("No invasions found matching the criteria", 
                    new GUIStyle(guiSystem.LabelStyle) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic }, 
                    GUILayout.ExpandWidth(true));
            }
            else
            {
                DrawInvasionTable(filtered, userX, userY);
            }

            GUILayout.EndScrollView();
        }
        
        private void DrawInvasionTable(System.Collections.Generic.List<(float x, float y, string level)> filtered, float userX, float userY)
        {
            // Display each invasion in a table row
            for (int i = 0; i < filtered.Count; i++)
            {
                var loc = filtered[i];
                bool isEven = i % 2 == 0;
                
                // Highlight row if it's the next one to be selected with semicolon
                bool isNextToSelect = (i == invasionManager.SelectedInvasionIndex);
                
                GUIStyle rowStyle = isEven ? guiSystem.TableRowStyle : guiSystem.TableRowAltStyle;
                
                // Highlight the next row that would be selected
                if (isNextToSelect)
                {
                    GUI.color = new Color(0.3f, 0.7f, 1f, Mathf.Sin(guiSystem.AnimationTime * 4) * 0.2f + 0.8f); // Pulsing highlight
                }
                else
                {
                    GUI.color = Color.white;
                }
                
                // Calculate distance for display
                float distance = (float)Math.Sqrt(Math.Pow(loc.x - userX, 2) + Math.Pow(loc.y - userY, 2));
                
                GUILayout.BeginHorizontal(rowStyle, GUILayout.Height(32));
                
                GUILayout.Label(loc.level, guiSystem.TableCellStyle, GUILayout.Width(90));
                GUILayout.Label(loc.x.ToString(), guiSystem.TableCellStyle, GUILayout.Width(90));
                GUILayout.Label(loc.y.ToString(), guiSystem.TableCellStyle, GUILayout.Width(90));
                GUILayout.Label(distance.ToString("F1"), guiSystem.TableCellStyle, GUILayout.Width(90));
                
                // Go button with modern style
                GUI.color = Color.white; // Reset color for button
                if (GUILayout.Button("Go", guiSystem.ActionButtonStyle, GUILayout.ExpandWidth(true), GUILayout.Height(24)))
                {
                    buttonAutomation.SetInputsAndPressButtons((int)loc.x, (int)loc.y);
                    guiSystem.ShowSuccessMessage($"Navigating to invasion at ({loc.x}, {loc.y}) - Level {loc.level}");
                }
                
                GUILayout.EndHorizontal();
                
                // Restore normal color
                GUI.color = Color.white;
            }
        }
        
        private void DrawStatusInfo()
        {
            GUILayout.Space(10);
            
            // Status info with modern style
            GUILayout.BeginVertical(new GUIStyle(GUI.skin.box) { normal = { background = guiSystem.TableHeaderTexture }, padding = new RectOffset(10, 10, 10, 10) });
            float.TryParse(yourXInput, out float userX);
            float.TryParse(yourYInput, out float userY);
            var filtered = invasionManager.GetFilteredInvasions(userX, userY, minLevelInput, maxLevelInput);
            GUILayout.Label($"Total invasions: {invasionManager.InvasionLocations.Count} ? Filtered: {filtered.Count}", 
                new GUIStyle(guiSystem.LabelStyle) { alignment = TextAnchor.MiddleCenter });
            GUILayout.EndVertical();
            
            GUILayout.Space(5);
            
            // Help text at the bottom with current keybinds
            GUILayout.Label($"{settingsManager.GetGuiToggleKeyDisplayName()} - Toggle GUI | {settingsManager.GetActionKeyDisplayName()} - Cycle through invasions", 
                new GUIStyle(guiSystem.LabelStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 10, fontStyle = FontStyle.Italic });
            GUILayout.Label("Use 'Fat Aliens' in Toggles to make aliens at a specific level appear 2x larger", 
                new GUIStyle(guiSystem.LabelStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 9, fontStyle = FontStyle.Italic });
            GUILayout.Label("Change keybinds in the Keybinds window", 
                new GUIStyle(guiSystem.LabelStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 9, fontStyle = FontStyle.Italic });
        }
        
        private void DrawTogglesWindow(int windowID)
        {
            GUILayout.BeginVertical(GUILayout.Width(180)); // Fixed width for consistent layout
            
            GUILayout.Space(5);
            
            // Night Mode Toggle with proper layout
            GUILayout.BeginHorizontal();
            GUILayout.Label("Night Mode", guiSystem.LabelStyle, GUILayout.Width(100)); // Fixed width label
            bool prevNightState = nightModeManager.NightModeEnabled;
            bool newNightState = GUILayout.Toggle(prevNightState, "", guiSystem.ToggleStyle, GUILayout.Width(20)); // Just the toggle, no label
            
            if (prevNightState != newNightState)
            {
                nightModeManager.NightModeEnabled = newNightState;
                guiSystem.ShowSuccessMessage($"Night Mode {(newNightState ? "Enabled" : "Disabled")}");
            }
            GUILayout.EndHorizontal();
            
            // Camera Zoom Toggle with proper layout
            GUILayout.BeginHorizontal();
            GUILayout.Label("Camera Zoom", guiSystem.LabelStyle, GUILayout.Width(100)); // Fixed width label
            bool prevZoomState = cameraController.CameraZoomEnabled;
            bool newZoomState = GUILayout.Toggle(prevZoomState, "", guiSystem.ToggleStyle, GUILayout.Width(20)); // Just the toggle, no label
            
            if (prevZoomState != newZoomState)
            {
                cameraController.CameraZoomEnabled = newZoomState;
                guiSystem.ShowSuccessMessage($"Camera Zoom {(newZoomState ? "Enhanced (1000x)" : "Normal")}");
            }
            GUILayout.EndHorizontal();
            
            // Fat Aliens Toggle with proper layout
            GUILayout.BeginHorizontal();
            GUILayout.Label("Fat Aliens", guiSystem.LabelStyle, GUILayout.Width(100)); // Fixed width label
            bool prevFatState = fatAliensManager.FatAliensEnabled;
            bool newFatState = GUILayout.Toggle(prevFatState, "", guiSystem.ToggleStyle, GUILayout.Width(20)); // Just the toggle, no label
            
            if (prevFatState != newFatState)
            {
                fatAliensManager.FatAliensEnabled = newFatState;
                guiSystem.ShowSuccessMessage($"Fat Aliens {(newFatState ? "Enabled" : "Disabled")}");
            }
            GUILayout.EndHorizontal();
            
            // Fat Aliens Level Input
            GUILayout.BeginHorizontal();
            GUILayout.Label("Level:", guiSystem.LabelStyle, GUILayout.Width(50));
            GUI.SetNextControlName("FatAliensLevelInput");
            string newLevelInput = GUILayout.TextField(fatAliensLevelInput, guiSystem.InputFieldStyle, GUILayout.Width(50), GUILayout.Height(20));
            
            if (newLevelInput != fatAliensLevelInput)
            {
                fatAliensLevelInput = newLevelInput;
                fatAliensManager.TargetLevel = fatAliensLevelInput;
            }
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            // Make window draggable
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
        
        private void HandleWindowEvents()
        {
            // Any interaction with the window is considered a GUI interaction and should be consumed
            if (Event.current != null && Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
            {
                // Specifically mark scroll events as consumed
                if (Event.current.type == EventType.ScrollWheel)
                {
                    // Reset game input and consume the event
                    Input.ResetInputAxes();
                    Event.current.Use();
                }
                else
                {
                    Event.current.Use();
                }
            }
        }
        
        private void HandleSemicolonAction()
        {
            float.TryParse(yourXInput, out float userX);
            float.TryParse(yourYInput, out float userY);
            
            var invasion = invasionManager.GetNextInvasion(userX, userY, minLevelInput, maxLevelInput);
            if (invasion.HasValue)
            {
                buttonAutomation.SetInputsAndPressButtons((int)invasion.Value.x, (int)invasion.Value.y);
                guiSystem.ShowSuccessMessage($"Navigating to invasion at ({invasion.Value.x}, {invasion.Value.y}) - Level {invasion.Value.level}");
            }
        }
        
        private void DrawSettingsWindow(int windowID)
        {
            GUILayout.BeginVertical(GUILayout.Width(180));
            
            GUILayout.Space(5);
            GUILayout.Label("Keybind Settings", new GUIStyle(guiSystem.LabelStyle) { fontStyle = FontStyle.Bold });
            GUILayout.Space(10);
            
            // GUI Toggle Key Setting
            GUILayout.BeginVertical();
            GUILayout.Label("GUI Toggle Key:", guiSystem.LabelStyle);
            int currentGuiToggleIndex = settingsManager.GetGuiToggleKeyIndex();
            int newGuiToggleIndex = currentGuiToggleIndex;
            
            if (GUILayout.Button(settingsManager.GetGuiToggleKeyDisplayName(), guiSystem.ButtonStyle, GUILayout.Height(25)))
            {
                // Cycle to next option
                var options = settingsManager.GuiToggleKeyOptions;
                newGuiToggleIndex = (currentGuiToggleIndex + 1) % options.Length;
            }
            
            if (newGuiToggleIndex != currentGuiToggleIndex)
            {
                var options = settingsManager.GuiToggleKeyOptions;
                settingsManager.SetGuiToggleKeyByName(options[newGuiToggleIndex]);
                guiSystem.ShowSuccessMessage($"GUI Toggle key changed to {options[newGuiToggleIndex]}");
            }
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Action Key Setting
            GUILayout.BeginVertical();
            GUILayout.Label("Action Key:", guiSystem.LabelStyle);
            int currentActionIndex = settingsManager.GetActionKeyIndex();
            int newActionIndex = currentActionIndex;
            
            if (GUILayout.Button(settingsManager.GetActionKeyDisplayName(), guiSystem.ButtonStyle, GUILayout.Height(25)))
            {
                // Cycle to next option
                var options = settingsManager.ActionKeyOptions;
                newActionIndex = (currentActionIndex + 1) % options.Length;
            }
            
            if (newActionIndex != currentActionIndex)
            {
                var options = settingsManager.ActionKeyOptions;
                settingsManager.SetActionKeyByName(options[newActionIndex]);
                guiSystem.ShowSuccessMessage($"Action key changed to {options[newActionIndex]}");
            }
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Info text
            GUILayout.Label("Click buttons to cycle through options", 
                new GUIStyle(guiSystem.LabelStyle) { 
                    fontSize = 9, 
                    fontStyle = FontStyle.Italic,
                    alignment = TextAnchor.MiddleCenter 
                });
            
            GUILayout.EndVertical();
            
            // Make window draggable
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}