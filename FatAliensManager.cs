using UnityEngine;
using System.Collections.Generic;
using MelonLoader;

namespace AASmasher
{
    public class FatAliensManager
    {
        private bool fatAliensEnabled = false;
        private string targetLevel = "";
        private Dictionary<(float x, float y), GameObject> invasionGameObjects = new Dictionary<(float x, float y), GameObject>();
        private Dictionary<(float x, float y), (Vector3 alienActivityOriginalScale, Vector3 levelBracketOriginalScale)> originalScales = new Dictionary<(float x, float y), (Vector3, Vector3)>();
        private float lastScanTime = 0f;
        private const float SCAN_INTERVAL = 2f;
        
        public bool FatAliensEnabled 
        { 
            get => fatAliensEnabled; 
            set 
            {
                if (fatAliensEnabled != value)
                {
                    fatAliensEnabled = value;
                    if (!fatAliensEnabled)
                    {
                        ResetAllScales();
                    }
                    else
                    {
                        ApplyFatAliens();
                    }
                }
            } 
        }
        
        public string TargetLevel 
        { 
            get => targetLevel; 
            set 
            {
                if (targetLevel != value)
                {
                    ResetAllScales();
                    targetLevel = value;
                    if (fatAliensEnabled)
                    {
                        ApplyFatAliens();
                    }
                }
            } 
        }
        
        public void OnSceneLoaded()
        {
            invasionGameObjects.Clear();
            originalScales.Clear();
            lastScanTime = 0f;
        }
        
        public void UpdateFatAliens(List<(float x, float y, string level)> invasionLocations)
        {
            if (!fatAliensEnabled || string.IsNullOrEmpty(targetLevel))
                return;
                
            if (Time.time - lastScanTime > SCAN_INTERVAL)
            {
                RefreshInvasionGameObjects();
                lastScanTime = Time.time;
                
                ApplyFatAliens();
            }
        }
        
        private void RefreshInvasionGameObjects()
        {
            invasionGameObjects.Clear();
            
            var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allGameObjects)
            {
                if (go != null && go.name != null && go.name.Contains("Invasion") && go.activeInHierarchy)
                {
                    var alienActivityCanvas = go.GetComponent("AlienActivityCanvas");
                    if (alienActivityCanvas != null)
                    {
                        var type = alienActivityCanvas.GetType();
                        System.Type abstractLocationCanvasType = null;
                        while (type != null)
                        {
                            if (type.Name == "AbstractLocationCanvas")
                            {
                                abstractLocationCanvasType = type;
                                break;
                            }
                            type = type.BaseType;
                        }
                        
                        if (abstractLocationCanvasType != null)
                        {
                            var getXMethod = abstractLocationCanvasType.GetMethod("GetX");
                            var getYMethod = abstractLocationCanvasType.GetMethod("GetY");
                            if (getXMethod != null && getYMethod != null)
                            {
                                var xVal = getXMethod.Invoke(alienActivityCanvas, null);
                                var yVal = getYMethod.Invoke(alienActivityCanvas, null);
                                
                                if (xVal is int xi && yVal is int yi)
                                {
                                    invasionGameObjects[(xi, yi)] = go;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void ApplyFatAliens()
        {
            if (!fatAliensEnabled || string.IsNullOrEmpty(targetLevel))
                return;
                
            foreach (var kvp in invasionGameObjects)
            {
                var coordinates = kvp.Key;
                var invasionObject = kvp.Value;
                
                if (invasionObject == null)
                    continue;
                    
                string invasionLevel = GetInvasionLevel(invasionObject);
                
                if (invasionLevel == targetLevel)
                {
                    ScaleInvasionObject(coordinates, invasionObject);
                }
            }
        }
        
        private string GetInvasionLevel(GameObject invasionObject)
        {
            var levelBracket = invasionObject.transform.Find("Level Bracket");
            if (levelBracket != null)
            {
                var levelTextObj = levelBracket.Find("Level Text");
                if (levelTextObj != null)
                {
                    var textComp = levelTextObj.GetComponent("Text");
                    if (textComp != null)
                    {
                        var textProp = textComp.GetType().GetProperty("text");
                        if (textProp != null)
                        {
                            return textProp.GetValue(textComp) as string ?? "?";
                        }
                    }
                }
            }
            return "?";
        }
        
        private void ScaleInvasionObject((float, float) coordinates, GameObject invasionObject)
        {
            var alienActivity = invasionObject.transform.Find("Alien Activity");
            var levelBracket = invasionObject.transform.Find("Level Bracket");
            
            if (!originalScales.ContainsKey(coordinates))
            {
                Vector3 alienActivityOriginal = alienActivity != null ? alienActivity.localScale : Vector3.one;
                Vector3 levelBracketOriginal = levelBracket != null ? levelBracket.localScale : Vector3.one;
                originalScales[coordinates] = (alienActivityOriginal, levelBracketOriginal);
            }
            
            Vector3 fatScale = new Vector3(2f, 2f, 2f);
            
            if (alienActivity != null)
            {
                alienActivity.localScale = fatScale;
                MelonLogger.Msg($"Applied fat scale to Alien Activity at ({coordinates.Item1}, {coordinates.Item2})");
            }
            
            if (levelBracket != null)
            {
                levelBracket.localScale = fatScale;
                MelonLogger.Msg($"Applied fat scale to Level Bracket at ({coordinates.Item1}, {coordinates.Item2})");
            }
        }
        
        private void ResetAllScales()
        {
            foreach (var kvp in originalScales)
            {
                var coordinates = kvp.Key;
                var (alienActivityOriginal, levelBracketOriginal) = kvp.Value;
                
                if (invasionGameObjects.TryGetValue(coordinates, out GameObject invasionObject) && invasionObject != null)
                {
                    var alienActivity = invasionObject.transform.Find("Alien Activity");
                    var levelBracket = invasionObject.transform.Find("Level Bracket");
                    
                    if (alienActivity != null)
                    {
                        alienActivity.localScale = alienActivityOriginal;
                    }
                    
                    if (levelBracket != null)
                    {
                        levelBracket.localScale = levelBracketOriginal;
                    }
                }
            }
            
            MelonLogger.Msg("Reset all invasion scales to original");
        }
        
        public void Reset()
        {
            ResetAllScales();
            invasionGameObjects.Clear();
            originalScales.Clear();
        }
    }
}