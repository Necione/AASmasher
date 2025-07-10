using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using MelonLoader;

namespace AASmasher
{
    public class NightModeManager
    {
        private bool nightModeEnabled = false; // Default to disabled
        private GameObject worldPlane;
        private List<GameObject> worldChunks = new List<GameObject>();
        private bool sceneInitialized = false;
        private bool isInitializing = false;
        
        public bool NightModeEnabled 
        { 
            get => nightModeEnabled; 
            set 
            { 
                if (nightModeEnabled != value)
                {
                    nightModeEnabled = value;
                    UpdateNightMode();
                }
            } 
        }
        
        public void InitializeNightMode()
        {
            try
            {
                // Prevent recursive initialization
                if (isInitializing || sceneInitialized) 
                    return;

                isInitializing = true;

                // Only proceed if we're in Game Scene
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Game Scene")
                {
                    isInitializing = false;
                    return;
                }

                worldChunks.Clear();

                // Find World Plane by name instead of instance ID since IDs change each game load
                worldPlane = GameObject.Find("World Plane");
                
                // If GameObject.Find doesn't work, try searching through all objects
                if (worldPlane == null)
                {
                    var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (var go in allGameObjects)
                    {
                        if (go != null && go.name == "World Plane" && go.scene.name == "Game Scene")
                        {
                            worldPlane = go;
                            break;
                        }
                    }
                }

                if (worldPlane != null)
                {
                    MelonLogger.Msg($"Found World Plane: {worldPlane.name}");
                    
                    // Get all children that contain "World Chunk"
                    Queue<Transform> toSearch = new Queue<Transform>();
                    toSearch.Enqueue(worldPlane.transform);

                    while (toSearch.Count > 0)
                    {
                        Transform current = toSearch.Dequeue();

                        // Only process if the current transform is valid
                        if (current != null)
                        {
                            if (current.gameObject.name.Contains("World Chunk"))
                            {
                                worldChunks.Add(current.gameObject);
                            }

                            // Add all valid children to search queue
                            for (int i = 0; i < current.childCount; i++)
                            {
                                Transform child = current.GetChild(i);
                                if (child != null)
                                {
                                    toSearch.Enqueue(child);
                                }
                            }
                        }
                    }

                    // Apply initial state to found chunks
                    if (worldChunks.Count > 0)
                    {
                        foreach (var chunk in worldChunks.ToList())
                        {
                            if (chunk != null)
                            {
                                chunk.SetActive(!nightModeEnabled);
                            }
                        }

                        sceneInitialized = true;
                        MelonLogger.Msg($"Night Mode initialized with {worldChunks.Count} chunks found");
                    }
                    else
                    {
                        MelonLogger.Warning("No World Chunks found in World Plane");
                    }
                }
                else
                {
                    MelonLogger.Warning("World Plane object not found in Game Scene");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error initializing Night Mode: {ex.Message}");
            }
            finally
            {
                isInitializing = false;
            }
        }

        public void UpdateNightMode()
        {
            if (!sceneInitialized && !isInitializing)
            {
                InitializeNightMode();
                return;
            }

            if (sceneInitialized && worldChunks != null && worldChunks.Count > 0)
            {
                foreach (var chunk in worldChunks.ToList())
                {
                    if (chunk != null)
                    {
                        chunk.SetActive(!nightModeEnabled);
                    }
                }
            }
        }
        
        public void OnSceneLoaded()
        {
            // Reset initialization state for new scene
            sceneInitialized = false;
            worldChunks.Clear();
            InitializeNightMode();
        }
        
        public void Reset()
        {
            sceneInitialized = false;
            worldChunks.Clear();
            isInitializing = false;
        }
    }
}