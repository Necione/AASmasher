using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using MelonLoader;

namespace AASmasher
{
    public class InvasionManager
    {
        private List<(float x, float y, string level)> invasionLocations = new List<(float x, float y, string level)>();
        private bool invasionLocationsScanned = false;
        private int selectedInvasionIndex = 0;
        private List<(float x, float y, string level)> lastFilteredInvasions = new List<(float x, float y, string level)>();
        
        public List<(float x, float y, string level)> InvasionLocations => invasionLocations;
        public bool InvasionLocationsScanned => invasionLocationsScanned;
        public int SelectedInvasionIndex 
        { 
            get => selectedInvasionIndex; 
            set => selectedInvasionIndex = value; 
        }
        
        public void UpdateInvasionLocations()
        {
            invasionLocations.Clear();
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.name != "Game Scene")
            {
                return;
            }
            
            var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allGameObjects)
            {
                if (go != null && go.name != null && go.name.Contains("Invasion"))
                {
                    var alienActivityCanvas = go.GetComponent("AlienActivityCanvas");
                    if (alienActivityCanvas != null)
                    {
                        var type = alienActivityCanvas.GetType();
                        Type abstractLocationCanvasType = null;
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
                                string level = "?";
                                
                                var levelBracket = go.transform.Find("Level Bracket");
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
                                                level = textProp.GetValue(textComp) as string ?? "?";
                                            }
                                        }
                                    }
                                }
                                if (xVal is int xi && yVal is int yi)
                                {
                                    if (xi != 0 || yi != 0)
                                    {
                                        invasionLocations.Add((xi, yi, level));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            MelonLogger.Msg($"Found {invasionLocations.Count} invasion locations");
            invasionLocationsScanned = true;
        }
        
        public List<(float x, float y, string level)> GetFilteredInvasions(
            float userX, float userY, 
            string minLevelInput, string maxLevelInput)
        {
            int.TryParse(minLevelInput, out int minLevel);
            int.TryParse(maxLevelInput, out int maxLevel);

            var filtered = invasionLocations
                .Where(loc =>
                    (string.IsNullOrEmpty(minLevelInput) || (int.TryParse(loc.level, out int lvl) && lvl >= minLevel)) &&
                    (string.IsNullOrEmpty(maxLevelInput) || (int.TryParse(loc.level, out int lvl2) && lvl2 <= maxLevel))
                )
                .OrderBy(loc => Math.Sqrt(Math.Pow(loc.x - userX, 2) + Math.Pow(loc.y - userY, 2)))
                .ToList();

            // Update last filtered invasions and reset index if the list changed
            if (lastFilteredInvasions.Count != filtered.Count || !lastFilteredInvasions.SequenceEqual(filtered))
            {
                selectedInvasionIndex = 0;
                lastFilteredInvasions = new List<(float x, float y, string level)>(filtered);
            }

            return filtered;
        }
        
        public (float x, float y, string level)? GetNextInvasion(
            float userX, float userY, 
            string minLevelInput, string maxLevelInput)
        {
            if (!invasionLocationsScanned || invasionLocations.Count == 0)
            {
                UpdateInvasionLocations();
            }

            var filtered = GetFilteredInvasions(userX, userY, minLevelInput, maxLevelInput);

            if (filtered.Count > 0)
            {
                if (selectedInvasionIndex >= filtered.Count)
                    selectedInvasionIndex = 0;
                
                var invasion = filtered[selectedInvasionIndex];
                selectedInvasionIndex = (selectedInvasionIndex + 1) % filtered.Count;
                
                return invasion;
            }
            
            return null;
        }
        
        public void Reset()
        {
            invasionLocationsScanned = false;
            invasionLocations.Clear();
            selectedInvasionIndex = 0;
            lastFilteredInvasions.Clear();
        }
        
        public void ForceRefresh()
        {
            invasionLocationsScanned = false;
            UpdateInvasionLocations();
        }
    }
}