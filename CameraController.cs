using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using MelonLoader;

namespace AASmasher
{
    public class CameraController
    {
        private Component gameCamera;
        private object originalCameraState;
        private MethodInfo cameraUpdateMethod;
        private bool hasCapturedOriginalCamera = false;
        
        private bool cameraZoomEnabled = false;
        private bool cameraInitialized = false;
        private object originalDefaultZoom = null;
        private object originalMaxZoom = null;
        
        public bool CameraZoomEnabled
        {
            get => cameraZoomEnabled;
            set
            {
                if (cameraZoomEnabled != value)
                {
                    cameraZoomEnabled = value;
                    UpdateCameraZoom();
                }
            }
        }
        
        public void OnSceneLoaded(int buildIndex, string sceneName)
        {
            if (sceneName != "Game Scene") return;
            
            SetupCameraController();
        }
        
        private void SetupCameraController()
        {
            GameObject worldCamera = GameObject.Find("World Camera");
            
            if (worldCamera == null)
            {
                var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                var found = allGameObjects.FirstOrDefault(go => go != null && go.name == "World Camera");
                if (found != null)
                {
                    worldCamera = found;
                    MelonLogger.Msg($"Found World Camera via fallback search: {worldCamera.name}");
                }
            }
            
            if (worldCamera != null)
            {
                MelonLogger.Msg($"Found World Camera: {worldCamera.name}");
                
                Component cameraController = worldCamera.GetComponent("CameraController");
                if (cameraController == null)
                {
                    var allComponents = Resources.FindObjectsOfTypeAll<Component>();
                    var found = allComponents.FirstOrDefault(c => c != null && c.GetType().Name == "CameraController");
                    if (found != null)
                    {
                        cameraController = found;
                        MelonLogger.Msg("Found CameraController component via search");
                    }
                }
                
                if (cameraController != null)
                {
                    gameCamera = cameraController;
                    
                    InitializeCameraZoom(cameraController);
                    
                    cameraUpdateMethod = cameraController.GetType().GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    
                    MelonLogger.Msg("Camera controller setup completed");
                }
                else
                {
                    MelonLogger.Warning("CameraController component not found");
                }
            }
            else
            {
                MelonLogger.Warning("World Camera not found by name or fallback search");
            }
        }
        
        private void InitializeCameraZoom(Component cameraController)
        {
            try
            {
                var type = cameraController.GetType();
                var defaultZoomField = type.GetField("defaultZoom");
                var maxZoomField = type.GetField("maxZoom");
                
                if (defaultZoomField != null && maxZoomField != null)
                {
                    originalDefaultZoom = defaultZoomField.GetValue(cameraController);
                    originalMaxZoom = maxZoomField.GetValue(cameraController);
                    
                    cameraInitialized = true;
                    MelonLogger.Msg($"Camera zoom initialized. Original values - Default: {originalDefaultZoom}, Max: {originalMaxZoom}");
                    
                    UpdateCameraZoom();
                }
                else
                {
                    MelonLogger.Warning("Could not find zoom fields in CameraController");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error initializing camera zoom: {ex.Message}");
            }
        }
        
        private void UpdateCameraZoom()
        {
            if (!cameraInitialized || gameCamera == null)
                return;
                
            try
            {
                var type = gameCamera.GetType();
                var defaultZoomField = type.GetField("defaultZoom");
                var maxZoomField = type.GetField("maxZoom");
                
                if (defaultZoomField != null && maxZoomField != null)
                {
                    if (cameraZoomEnabled)
                    {
                        defaultZoomField.SetValue(gameCamera, 1000f);
                        maxZoomField.SetValue(gameCamera, 1000f);
                        MelonLogger.Msg("Enhanced camera zoom enabled (1000x)");
                    }
                    else
                    {
                        defaultZoomField.SetValue(gameCamera, originalDefaultZoom);
                        maxZoomField.SetValue(gameCamera, originalMaxZoom);
                        MelonLogger.Msg("Camera zoom restored to original values");
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error updating camera zoom: {ex.Message}");
            }
        }
        
        public void CaptureAndDisableCameraInput()
        {
            try
            {
                if (gameCamera != null && !hasCapturedOriginalCamera)
                {
                    var type = gameCamera.GetType();
                    
                    var inputEnabledField = type.GetField("inputEnabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var dragEnabledField = type.GetField("dragEnabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var mouseControlField = type.GetField("mouseControl", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    
                    if (inputEnabledField != null)
                    {
                        originalCameraState = inputEnabledField.GetValue(gameCamera);
                        inputEnabledField.SetValue(gameCamera, false);
                        hasCapturedOriginalCamera = true;
                    }
                    else if (dragEnabledField != null)
                    {
                        originalCameraState = dragEnabledField.GetValue(gameCamera);
                        dragEnabledField.SetValue(gameCamera, false);
                        hasCapturedOriginalCamera = true;
                    }
                    else if (mouseControlField != null)
                    {
                        originalCameraState = mouseControlField.GetValue(gameCamera);
                        mouseControlField.SetValue(gameCamera, false);
                        hasCapturedOriginalCamera = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error capturing camera state: {ex.Message}");
            }
        }
        
        public void RestoreCameraInput()
        {
            try
            {
                if (gameCamera != null && hasCapturedOriginalCamera && originalCameraState != null)
                {
                    var type = gameCamera.GetType();
                    
                    var inputEnabledField = type.GetField("inputEnabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var dragEnabledField = type.GetField("dragEnabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var mouseControlField = type.GetField("mouseControl", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    
                    if (inputEnabledField != null)
                    {
                        inputEnabledField.SetValue(gameCamera, originalCameraState);
                    }
                    else if (dragEnabledField != null)
                    {
                        dragEnabledField.SetValue(gameCamera, originalCameraState);
                    }
                    else if (mouseControlField != null)
                    {
                        mouseControlField.SetValue(gameCamera, originalCameraState);
                    }
                    
                    hasCapturedOriginalCamera = false;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error restoring camera state: {ex.Message}");
            }
        }
        
        public void Reset()
        {
            hasCapturedOriginalCamera = false;
            originalCameraState = null;
            cameraInitialized = false;
            originalDefaultZoom = null;
            originalMaxZoom = null;
        }
    }
}