using UnityEngine;
using System.Collections;
using System;
using MelonLoader;

namespace AASmasher
{
    public class ButtonAutomation
    {
        public void SetInputsAndPressButtons(int x, int y)
        {
            try
            {
                var allInputs = Resources.FindObjectsOfTypeAll<UnityEngine.UI.InputField>();
                UnityEngine.UI.InputField xInput = null;
                UnityEngine.UI.InputField yInput = null;
                
                foreach (var input in allInputs)
                {
                    if (input != null)
                    {
                        if (input.gameObject.name.Contains("X") && input.gameObject.name.Contains("Input"))
                        {
                            xInput = input;
                        }
                        else if (input.gameObject.name.Contains("Y") && input.gameObject.name.Contains("Input"))
                        {
                            yInput = input;
                        }
                        
                        if (xInput == null && input.transform.parent != null && 
                            input.transform.parent.name.Contains("X"))
                        {
                            xInput = input;
                        }
                        else if (yInput == null && input.transform.parent != null && 
                                input.transform.parent.name.Contains("Y"))
                        {
                            yInput = input;
                        }
                    }
                }
                
                if (xInput != null && yInput != null)
                {
                    xInput.text = x.ToString();
                    xInput.onValueChanged.Invoke(xInput.text); 
                    yInput.text = y.ToString();
                    yInput.onValueChanged.Invoke(yInput.text); 
                    
                    xInput.ForceLabelUpdate();
                    yInput.ForceLabelUpdate();
                    
                    MelonLogger.Msg($"Successfully set coordinates: X={x}, Y={y}");
                }
                else
                {
                    MelonLogger.Warning($"Could not find input fields. Found X input: {xInput != null}, Found Y input: {yInput != null}");
                }
                
                MelonCoroutines.Start(PressButtonsCoroutine());
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error simulating button click or setting input fields: {ex}");
            }
        }

        private IEnumerator PressButtonsCoroutine()
        {
            yield return null;
            
            GameObject gameCanvas = null;
            Transform bookmarksPanel = null;
            UnityEngine.UI.Button bookmarksButton = null;
            
            try
            {
                gameCanvas = GameObject.Find("Game Canvas");
                if (gameCanvas == null)
                {
                    MelonLogger.Warning("Game Canvas not found");
                    yield break;
                }

                Transform iphoneXPanel = gameCanvas.transform.Find("IphoneX Panel");
                if (iphoneXPanel == null)
                {
                    MelonLogger.Warning("IphoneX Panel not found");
                    yield break;
                }

                Transform mainHudPanel = iphoneXPanel.Find("Main HUD Panel");
                if (mainHudPanel == null)
                {
                    MelonLogger.Warning("Main HUD Panel not found");
                    yield break;
                }

                Transform standaloneHUD = mainHudPanel.Find("Standalone HUD");
                if (standaloneHUD == null)
                {
                    MelonLogger.Warning("Standalone HUD not found");
                    yield break;
                }

                Transform mainButtonHolder = standaloneHUD.Find("Main Button Holder");
                if (mainButtonHolder == null)
                {
                    MelonLogger.Warning("Main Button Holder not found");
                    yield break;
                }

                bookmarksPanel = mainButtonHolder.Find("Standalone HUD : Bookmarks Panel");
                if (bookmarksPanel == null)
                {
                    MelonLogger.Warning("Standalone HUD : Bookmarks Panel not found");
                    yield break;
                }

                bookmarksButton = bookmarksPanel.GetComponent<UnityEngine.UI.Button>();
                if (bookmarksButton == null)
                {
                    MelonLogger.Warning("Button component not found on Bookmarks Panel");
                    yield break;
                }

                bookmarksButton.onClick.Invoke();
                MelonLogger.Msg("Successfully clicked bookmarks button via path");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error finding bookmarks button: {ex.Message}");
                yield break;
            }

            yield return null;
            yield return null;
            yield return null;

            try
            {
                UnityEngine.UI.Button goToButton = null;

                GameObject disabledUI = GameObject.Find("Enabled UI");
                if (disabledUI != null)
                {
                    Transform bookmarksPanelInDisabled = disabledUI.transform.Find("Bookmarks Panel");
                    if (bookmarksPanelInDisabled != null)
                    {
                        Transform container = bookmarksPanelInDisabled.Find("Container");
                        if (container != null)
                        {
                            Transform gotoPanel = container.Find("Goto Panel");
                            if (gotoPanel != null)
                            {
                                Transform buttonTransform = gotoPanel.Find("Button");
                                if (buttonTransform != null)
                                {
                                    goToButton = buttonTransform.GetComponent<UnityEngine.UI.Button>();
                                    if (goToButton != null)
                                    {
                                        MelonLogger.Msg("Successfully found Go To button via path: Enabled UI/Bookmarks Panel/Container/Goto Panel/Button");
                                    }
                                }
                                else
                                {
                                    MelonLogger.Warning("Button transform not found in Goto Panel");
                                }
                            }
                            else
                            {
                                MelonLogger.Warning("Goto Panel not found in Container");
                            }
                        }
                        else
                        {
                            MelonLogger.Warning("Container not found in Bookmarks Panel");
                        }
                    }
                    else
                    {
                        MelonLogger.Warning("Bookmarks Panel not found in Enabled UI");
                    }
                }
                else
                {
                    MelonLogger.Warning("Enabled UI GameObject not found");
                }
                
                if (goToButton == null)
                {
                    MelonLogger.Warning("Go To button not found via path, trying fallback method");
                    
                    var allButtons = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Button>();
                    
                    foreach (var button in allButtons)
                    {
                        if (button != null)
                        {
                            int id = button.gameObject.GetInstanceID();
                            if (id == 221902) 
                            {
                                goToButton = button;
                                MelonLogger.Msg("Found Go To button via legacy instance ID");
                                break;
                            }
                            
                            if (button.gameObject.name.Contains("Go") || 
                                button.gameObject.name.Contains("Navigate") ||
                                button.gameObject.name.Contains("Travel") ||
                                button.gameObject.name.Equals("Button", StringComparison.OrdinalIgnoreCase))
                            {
                                Transform parent = button.transform.parent;
                                if (parent != null && (parent.name.Contains("Goto") || parent.name.Contains("Go To")))
                                {
                                    goToButton = button;
                                    MelonLogger.Msg($"Found Go To button via name/parent search: {button.gameObject.name} (parent: {parent.name})");
                                    break;
                                }
                                else if (button.gameObject.name.Contains("Go") || button.gameObject.name.Contains("Navigate") || button.gameObject.name.Contains("Travel"))
                                {
                                    goToButton = button;
                                    MelonLogger.Msg($"Found Go To button via name search: {button.gameObject.name}");
                                    break;
                                }
                            }
                        }
                    }
                }

                if (goToButton != null)
                {
                    if (goToButton.interactable)
                    {
                        goToButton.onClick.Invoke();
                        MelonLogger.Msg("Successfully clicked Go To button");
                    }
                    else
                    {
                        MelonLogger.Warning("Go To button found but is not interactable");
                    }
                }
                else
                {
                    MelonLogger.Warning("Go To button not found using any method");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error finding Go To button: {ex.Message}");
            }
        }
    }
}