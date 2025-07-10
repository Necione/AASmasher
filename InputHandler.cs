using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace AASmasher
{
    public class InputHandler
    {
        // Import Windows API functions for direct keyboard detection
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        
        // Virtual key codes for different keys
        private const int VK_RSHIFT = 0xA1;
        private const int VK_LSHIFT = 0xA0;
        private const int VK_F1 = 0x70;
        private const int VK_F2 = 0x71;
        private const int VK_F3 = 0x72;
        private const int VK_F4 = 0x73;
        private const int VK_F5 = 0x74;
        private const int VK_F6 = 0x75;
        private const int VK_TAB = 0x09;
        private const int VK_TILDE = 0xC0;
        private const int VK_INSERT = 0x2D;
        private const int VK_HOME = 0x24;
        private const int VK_END = 0x23;
        
        private const float KEY_DEBOUNCE_TIME = 0.2f; // 200ms debounce time
        
        // Enhanced key detection with debouncing
        private bool wasToggleKeyPressedLastFrame = false;
        private float lastKeyPressTime = 0f;
        
        // Settings reference
        private SettingsManager settingsManager;
        
        // Input handling
        private Dictionary<KeyCode, bool> blockedKeys = new Dictionary<KeyCode, bool>();
        private HashSet<KeyCode> whitelistedKeys = new HashSet<KeyCode>();
        private bool isMouseOverGUI = false;
        private bool isDraggingWindow = false;
        private Vector2 lastMousePosition;
        private Vector2 mouseDelta = Vector2.zero;
        private bool isScrolling = false;
        private float lastScrollTime = 0f;
        private float scrollCooldownTime = 0.1f;
        private bool preventNextScroll = false;
        
        // Text input focus tracking
        private bool isAnyTextFieldFocused = false;
        
        public bool IsMouseOverGUI => isMouseOverGUI;
        public bool IsDraggingWindow => isDraggingWindow;
        public bool IsScrolling => isScrolling;
        public bool IsAnyTextFieldFocused => isAnyTextFieldFocused;
        public bool PreventNextScroll => preventNextScroll;
        
        public InputHandler(SettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
            InitializeBlockedKeys();
            UpdateWhitelistedKeys();
            
            // Subscribe to settings changes
            settingsManager.OnGuiToggleKeyChanged += OnGuiToggleKeyChanged;
        }
        
        private void InitializeBlockedKeys()
        {
            // Add commonly used game keys to block when GUI is open
            KeyCode[] keysToBlock = new KeyCode[] {
                KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Q, KeyCode.E, KeyCode.R,
                KeyCode.F, KeyCode.Space, KeyCode.Tab, KeyCode.LeftShift, KeyCode.LeftControl,
                KeyCode.Return, KeyCode.Escape, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
                KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8,
                KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2
            };
            
            foreach (var key in keysToBlock)
            {
                blockedKeys[key] = false;
            }
        }
        
        private void UpdateWhitelistedKeys()
        {
            whitelistedKeys.Clear();
            whitelistedKeys.Add(settingsManager.GuiToggleKey);
        }
        
        private void OnGuiToggleKeyChanged(KeyCode newKey)
        {
            UpdateWhitelistedKeys();
        }
        
        // Get the virtual key code for a Unity KeyCode
        private int GetVirtualKeyCode(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.RightShift: return VK_RSHIFT;
                case KeyCode.LeftShift: return VK_LSHIFT;
                case KeyCode.F1: return VK_F1;
                case KeyCode.F2: return VK_F2;
                case KeyCode.F3: return VK_F3;
                case KeyCode.F4: return VK_F4;
                case KeyCode.F5: return VK_F5;
                case KeyCode.F6: return VK_F6;
                case KeyCode.Tab: return VK_TAB;
                case KeyCode.BackQuote: return VK_TILDE;
                case KeyCode.Insert: return VK_INSERT;
                case KeyCode.Home: return VK_HOME;
                case KeyCode.End: return VK_END;
                default: return -1; // No virtual key code mapping
            }
        }
        
        // Detect if a specific key is pressed using the Windows API (works even with focused text fields)
        private bool IsKeyPressedLowLevel(int vKey)
        {
            return (GetAsyncKeyState(vKey) & 0x8000) != 0;
        }
        
        // Improved toggle key detection with proper debouncing
        public bool CheckToggleKeyPressed()
        {
            // Get current time
            float currentTime = Time.time;
            
            // Check if we're still in debounce period
            if (currentTime - lastKeyPressTime < KEY_DEBOUNCE_TIME)
            {
                return false;
            }
            
            bool keyPressed = false;
            KeyCode toggleKey = settingsManager.GuiToggleKey;
            
            // First try Unity's input system (works when text fields don't have focus)
            if (!isAnyTextFieldFocused)
            {
                keyPressed = Input.GetKeyDown(toggleKey);
            }
            else
            {
                // Use low-level API when text fields have focus
                int vKey = GetVirtualKeyCode(toggleKey);
                if (vKey != -1)
                {
                    bool currentlyPressed = IsKeyPressedLowLevel(vKey);
                    
                    // Detect key press (transition from not pressed to pressed)
                    keyPressed = currentlyPressed && !wasToggleKeyPressedLastFrame;
                    wasToggleKeyPressedLastFrame = currentlyPressed;
                }
                else
                {
                    // Fallback to Unity input for keys without virtual key mapping
                    keyPressed = Input.GetKeyDown(toggleKey);
                }
            }
            
            // If key was pressed, record the time and prevent multiple triggers
            if (keyPressed)
            {
                lastKeyPressTime = currentTime;
                return true;
            }
            
            return false;
        }
        
        // Check if the action key is pressed
        public bool CheckActionKeyPressed()
        {
            return Input.GetKeyDown(settingsManager.ActionKey);
        }
        
        public void UpdateInputState(float deltaTime)
        {
            // Track scroll cooldown
            if (isScrolling)
            {
                lastScrollTime += deltaTime;
                if (lastScrollTime > scrollCooldownTime)
                {
                    isScrolling = false;
                    lastScrollTime = 0f;
                }
            }
            
            // Always track mouse deltas for detecting drags
            Vector2 currentMousePos = Input.mousePosition;
            mouseDelta = currentMousePos - lastMousePosition;
            lastMousePosition = currentMousePos;
        }
        
        public void HandleInputBlockingWhenGuiIsOpen(Rect guiRect)
        {
            // Block all key presses by tracking them to avoid repeating actions
            foreach (var key in blockedKeys.Keys.ToList())
            {
                if (Input.GetKeyDown(key) && !blockedKeys[key])
                {
                    blockedKeys[key] = true;
                    
                    // Block event processing for this frame
                    Event.current?.Use();
                }
                else if (Input.GetKeyUp(key))
                {
                    blockedKeys[key] = false;
                    
                    // Block event processing for this frame
                    Event.current?.Use();
                }
            }
            
            // Enhanced scroll wheel blocking - always block regardless of position
            if (Input.mouseScrollDelta.y != 0)
            {
                // Always block scroll events from reaching the game when GUI is open
                Input.ResetInputAxes();
                
                // Set the scrolling state to prevent scroll events from getting to the game
                isScrolling = true;
                lastScrollTime = 0f;
                
                // Set flag to handle in OnGUI
                preventNextScroll = false;
            }
            
            // Block mouse movement when dragging
            if (isDraggingWindow || (guiRect.Contains(Input.mousePosition) && Input.GetMouseButton(0)))
            {
                isDraggingWindow = true;
                
                // Reset input axes to prevent camera movement
                Input.ResetInputAxes();
                
                // Release dragging state when mouse is released
                if (!Input.GetMouseButton(0))
                {
                    isDraggingWindow = false;
                }
            }
            
            // Detect if any mouse button is down and the mouse is over our GUI
            if ((Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)) && isMouseOverGUI)
            {
                // Block all mouse input to the game
                Input.ResetInputAxes();
            }
        }
        
        public void HandleGuiEvents(Rect guiRect)
        {
            // Check if any text field has focus
            isAnyTextFieldFocused = GUI.GetNameOfFocusedControl() != "";
            
            // Handle all input events to prevent them from passing through the UI
            Event currentEvent = Event.current;
            if (currentEvent != null)
            {
                // Always capture and consume ALL event types when GUI is open
                if (currentEvent.type == EventType.MouseDown || 
                    currentEvent.type == EventType.MouseDrag || 
                    currentEvent.type == EventType.MouseUp ||
                    currentEvent.type == EventType.MouseMove)
                {
                    // Mark mouse as over GUI when interacting
                    isMouseOverGUI = true;
                    
                    // Consume the event to prevent game from seeing it
                    currentEvent.Use();
                }
                
                // Specially handle scroll wheel events
                if (currentEvent.type == EventType.ScrollWheel)
                {
                    // Mark mouse as over GUI
                    isMouseOverGUI = true;
                    
                    // Set scrolling state active
                    isScrolling = true;
                    lastScrollTime = 0f;
                    
                    if (preventNextScroll)
                    {
                        preventNextScroll = false;
                        currentEvent.Use();
                    }
                    else
                    {
                        // Only allow scrolling if mouse is over the GUI window
                        if (!guiRect.Contains(currentEvent.mousePosition))
                        {
                            currentEvent.Use(); // Consume the event if it's outside the window
                            preventNextScroll = true; // Set flag to prevent next scroll
                        }
                    }
                }
                
                // Always consume keyboard events when GUI is open except whitelisted ones
                if (currentEvent.type == EventType.KeyDown || currentEvent.type == EventType.KeyUp)
                {
                    // Even if this is a whitelisted key, if a text field is focused, 
                    // we still need to intercept the event to make our low-level check work
                    if (isAnyTextFieldFocused || !whitelistedKeys.Contains(currentEvent.keyCode))
                    {
                        currentEvent.Use();
                    }
                }
            }
            
            // Always consider the mouse over GUI when the GUI is shown
            isMouseOverGUI = true;
            
            // Make sure all input events are captured and not passed to the game
            if (currentEvent != null && currentEvent.type != EventType.Repaint && currentEvent.type != EventType.Layout)
            {
                currentEvent.Use();
            }
        }
        
        public void ResetInputState()
        {
            // Reset text field focus state
            isAnyTextFieldFocused = false;
            
            // Reset key state tracking
            wasToggleKeyPressedLastFrame = false;
            
            // Reset all blocked key states when toggling
            foreach (var key in blockedKeys.Keys.ToList())
            {
                blockedKeys[key] = false;
            }
            
            isDraggingWindow = false;
        }
        
        public void ResetInputAxesIfNeeded()
        {
            if (isDraggingWindow || isScrolling || Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                Input.ResetInputAxes();
            }
        }
    }
}