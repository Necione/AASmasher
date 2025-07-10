using MelonLoader;
using AASmasher;
using System;
using UnityEngine;

[assembly: MelonInfo(typeof(AASmasherClass), "AASmasher", "1.1.2", "Neci")]
[assembly: MelonGame("Hunted Cow Studios Ltd", "New Earth")]

namespace AASmasher
{
    public class AASmasherClass : MelonMod
    {
        private bool showGUI = false;
        
        private SettingsManager settingsManager;
        private GuiSystem guiSystem;
        private InputHandler inputHandler;
        private InvasionManager invasionManager;
        private NightModeManager nightModeManager;
        private ButtonAutomation buttonAutomation;
        private CameraController cameraController;
        private FatAliensManager fatAliensManager;
        private GuiRenderer guiRenderer;
        
        public override void OnInitializeMelon()
        {
            MelonEvents.OnGUI.Subscribe(DrawMenu, 100);
            MelonLogger.Msg("AASmasher initialized");
            
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            settingsManager = new SettingsManager();
            guiSystem = new GuiSystem();
            inputHandler = new InputHandler(settingsManager);
            invasionManager = new InvasionManager();
            nightModeManager = new NightModeManager();
            buttonAutomation = new ButtonAutomation();
            cameraController = new CameraController();
            fatAliensManager = new FatAliensManager();
            guiRenderer = new GuiRenderer(guiSystem, inputHandler, invasionManager, nightModeManager, buttonAutomation, cameraController, fatAliensManager, settingsManager);
        }
        
        public override void OnUpdate()
        {
            guiSystem.UpdateAnimation(Time.deltaTime);
            inputHandler.UpdateInputState(Time.deltaTime);
            
            if (inputHandler.CheckToggleKeyPressed())
            {
                SetGuiActive(!showGUI);
                MelonLogger.Msg($"GUI toggled to: {showGUI}"); // Debug logging
            }
            
            if (showGUI)
            {
                inputHandler.HandleInputBlockingWhenGuiIsOpen(guiSystem.GuiRect);
            }

            else
            {
                if (inputHandler.CheckActionKeyPressed())
                {
                    HandleActionHotkey();
                }
            }
            
            if (fatAliensManager.FatAliensEnabled)
            {
                if (!invasionManager.InvasionLocationsScanned)
                {
                    invasionManager.UpdateInvasionLocations();
                }
                fatAliensManager.UpdateFatAliens(invasionManager.InvasionLocations);
            }
        }

        private void SetGuiActive(bool active)
        {
            showGUI = active;
            
            if (showGUI)
            {
                invasionManager.UpdateInvasionLocations();
                guiSystem.ShowSuccessMessage($"Found {invasionManager.InvasionLocations.Count} invasion locations");
                
                cameraController.CaptureAndDisableCameraInput();
            }
            else
            {
                invasionManager.Reset();
                guiSystem.SetDragging(false);
                
                inputHandler.ResetInputState();
                
                cameraController.RestoreCameraInput();
            }
        }
        
        private void HandleActionHotkey()
        {
            float.TryParse(guiRenderer.YourXInput, out float userX);
            float.TryParse(guiRenderer.YourYInput, out float userY);
            
            var invasion = invasionManager.GetNextInvasion(userX, userY, guiRenderer.MinLevelInput, guiRenderer.MaxLevelInput);
            if (invasion.HasValue)
            {
                buttonAutomation.SetInputsAndPressButtons((int)invasion.Value.x, (int)invasion.Value.y);
                
                MelonLogger.Msg($"Navigating to Invasion at X: {invasion.Value.x}, Y: {invasion.Value.y}, Level: {invasion.Value.level}");
                
                if (showGUI)
                {
                    guiSystem.ShowSuccessMessage($"Navigating to invasion at ({invasion.Value.x}, {invasion.Value.y}) - Level {invasion.Value.level}");
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Game Scene")
            {
                nightModeManager.OnSceneLoaded();
            }
            
            cameraController.OnSceneLoaded(buildIndex, sceneName);
            
            fatAliensManager.OnSceneLoaded();
        }
        
        private void DrawMenu()
        {
            if (!showGUI) return;
            
            guiRenderer.DrawMenu(() => SetGuiActive(false));
        }
    }
}
