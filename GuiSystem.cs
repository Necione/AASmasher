using UnityEngine;
using System.Collections.Generic;

namespace AASmasher
{
    public class GuiSystem
    {
        // Modern GUI styles
        private GUIStyle windowStyle;
        private GUIStyle headerStyle;
        private GUIStyle labelStyle;
        private GUIStyle inputFieldStyle;
        private GUIStyle tableHeaderStyle;
        private GUIStyle tableCellStyle;
        private GUIStyle tableRowStyle;
        private GUIStyle tableRowAltStyle;
        private GUIStyle buttonStyle;
        private GUIStyle actionButtonStyle;
        private GUIStyle titleBarStyle;
        private GUIStyle scrollViewStyle;
        private GUIStyle toggleStyle;
        private bool stylesInitialized = false;
        
        // Textures
        private Texture2D windowBackgroundTexture;
        private Texture2D headerBackgroundTexture;
        private Texture2D inputBackgroundTexture;
        private Texture2D buttonBackgroundTexture;
        private Texture2D buttonHoverTexture;
        private Texture2D buttonActiveTexture;
        private Texture2D tableHeaderTexture;
        private Texture2D tableRowTexture;
        private Texture2D tableRowAltTexture;
        private Texture2D highlightTexture;
        private Texture2D titleBarTexture;
        private Texture2D closeButtonTexture;
        private Texture2D closeButtonHoverTexture;
        
        // UI animation
        private float animationTime = 0f;
        private bool showSuccessMessage = false;
        private float successMessageTime = 0f;
        private string successMessage = "";
        
        // GUI rectangles
        private Rect guiRect = new Rect(20, 20, 550, 700);
        private Rect togglesRect = new Rect(590, 20, 200, 170); // Increased height to accommodate Fat Aliens toggle and level input
        private Rect settingsRect = new Rect(590, 210, 200, 150); // Settings window below toggles
        private Vector2 scrollPosition = Vector2.zero;
        
        // Window dragging
        private bool isDraggingWindow = false;
        
        public Rect GuiRect => guiRect;
        public Rect TogglesRect => togglesRect;
        public Rect SettingsRect => settingsRect;
        public bool IsDraggingWindow => isDraggingWindow;
        
        public void UpdateAnimation(float deltaTime)
        {
            animationTime += deltaTime;
            
            // Success message fade out
            if (showSuccessMessage)
            {
                successMessageTime += deltaTime;
                if (successMessageTime > 3f) // Display for 3 seconds
                {
                    showSuccessMessage = false;
                    successMessageTime = 0f;
                }
            }
        }
        
        public void ShowSuccessMessage(string message)
        {
            successMessage = message;
            showSuccessMessage = true;
            successMessageTime = 0f;
        }
        
        public void InitializeStyles()
        {
            if (stylesInitialized)
                return;
            
            // Color palette for modern UI
            Color windowColor = new Color(0.12f, 0.12f, 0.14f, 0.97f);       // Dark background
            Color headerColor = new Color(0.16f, 0.16f, 0.18f, 1f);          // Slightly lighter than background
            Color accentColor = new Color(0.0f, 0.5f, 0.8f, 1f);             // Bright blue accent
            Color accentHoverColor = new Color(0.1f, 0.6f, 0.9f, 1f);        // Lighter accent for hover states
            Color accentActiveColor = new Color(0.0f, 0.4f, 0.7f, 1f);       // Darker accent for active states
            Color textColor = new Color(0.9f, 0.9f, 0.9f, 1f);               // Light text
            Color inputBgColor = new Color(0.18f, 0.18f, 0.2f, 1f);          // Input field background
            Color tableHeaderColor = new Color(0.2f, 0.2f, 0.22f, 1f);       // Table header background
            Color tableRowColor = new Color(0.15f, 0.15f, 0.17f, 0.95f);     // Table row
            Color tableRowAltColor = new Color(0.17f, 0.17f, 0.19f, 0.95f);  // Alternating table row
            Color highlightColor = new Color(0.0f, 0.5f, 0.8f, 0.3f);        // Row highlight
            Color titleBarColor = new Color(0.08f, 0.08f, 0.1f, 1f);         // Title bar
                
            // Create textures for UI elements
            windowBackgroundTexture = MakeTex(2, 2, windowColor);
            headerBackgroundTexture = MakeTex(2, 2, headerColor);
            inputBackgroundTexture = MakeTex(2, 2, inputBgColor);
            buttonBackgroundTexture = MakeTex(2, 2, accentColor);
            buttonHoverTexture = MakeTex(2, 2, accentHoverColor);
            buttonActiveTexture = MakeTex(2, 2, accentActiveColor);
            tableHeaderTexture = MakeTex(2, 2, tableHeaderColor);
            tableRowTexture = MakeTex(2, 2, tableRowColor);
            tableRowAltTexture = MakeTex(2, 2, tableRowAltColor);
            highlightTexture = MakeTex(2, 2, highlightColor);
            titleBarTexture = MakeTex(2, 2, titleBarColor);
            closeButtonTexture = MakeTex(2, 2, new Color(0.8f, 0.2f, 0.2f, 1f));
            closeButtonHoverTexture = MakeTex(2, 2, new Color(1f, 0.3f, 0.3f, 1f));
            
            // Initialize all styles
            InitializeWindowStyle(textColor);
            InitializeTitleBarStyle(textColor);
            InitializeHeaderStyle(textColor);
            InitializeLabelStyle(textColor);
            InitializeInputFieldStyle(textColor);
            InitializeTableStyles(textColor);
            InitializeButtonStyles(textColor);
            InitializeScrollViewStyle();
            InitializeToggleStyle(textColor);
            
            stylesInitialized = true;
        }
        
        private void InitializeWindowStyle(Color textColor)
        {
            windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.normal.background = windowBackgroundTexture;
            windowStyle.onNormal.background = windowBackgroundTexture;
            windowStyle.active.background = windowBackgroundTexture;
            windowStyle.onActive.background = windowBackgroundTexture;
            windowStyle.focused.background = windowBackgroundTexture;
            windowStyle.onFocused.background = windowBackgroundTexture;
            windowStyle.normal.textColor = textColor;
            windowStyle.onNormal.textColor = textColor;
            windowStyle.active.textColor = textColor;
            windowStyle.onActive.textColor = textColor;
            windowStyle.focused.textColor = textColor;
            windowStyle.onFocused.textColor = textColor;
            windowStyle.border = new RectOffset(1, 1, 20, 1);
            windowStyle.margin = new RectOffset(0, 0, 0, 0);
            windowStyle.padding = new RectOffset(10, 10, 30, 10);
            windowStyle.fontSize = 14;
            windowStyle.fontStyle = FontStyle.Bold;
        }
        
        private void InitializeTitleBarStyle(Color textColor)
        {
            titleBarStyle = new GUIStyle();
            titleBarStyle.normal.background = titleBarTexture;
            titleBarStyle.fixedHeight = 30;
            titleBarStyle.alignment = TextAnchor.MiddleLeft;
            titleBarStyle.normal.textColor = textColor;
            titleBarStyle.padding = new RectOffset(10, 10, 6, 0);
            titleBarStyle.fontSize = 14;
            titleBarStyle.fontStyle = FontStyle.Bold;
        }
        
        private void InitializeHeaderStyle(Color textColor)
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = textColor;
            headerStyle.alignment = TextAnchor.MiddleLeft;
        }
        
        private void InitializeLabelStyle(Color textColor)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 12;
            labelStyle.fontStyle = FontStyle.Normal;
            labelStyle.normal.textColor = textColor;
            labelStyle.alignment = TextAnchor.MiddleLeft;
        }
        
        private void InitializeInputFieldStyle(Color textColor)
        {
            inputFieldStyle = new GUIStyle(GUI.skin.textField);
            inputFieldStyle.normal.background = inputBackgroundTexture;
            inputFieldStyle.normal.textColor = textColor;
            inputFieldStyle.alignment = TextAnchor.MiddleLeft;
            inputFieldStyle.padding = new RectOffset(8, 8, 4, 4);
            inputFieldStyle.fontSize = 12;
        }
        
        private void InitializeTableStyles(Color textColor)
        {
            // Table header style
            tableHeaderStyle = new GUIStyle(GUI.skin.label);
            tableHeaderStyle.normal.background = tableHeaderTexture;
            tableHeaderStyle.fontSize = 12;
            tableHeaderStyle.fontStyle = FontStyle.Bold;
            tableHeaderStyle.normal.textColor = textColor;
            tableHeaderStyle.alignment = TextAnchor.MiddleCenter;
            tableHeaderStyle.padding = new RectOffset(5, 5, 8, 8);
            
            // Table cell style
            tableCellStyle = new GUIStyle(GUI.skin.label);
            tableCellStyle.fontSize = 12;
            tableCellStyle.fontStyle = FontStyle.Normal;
            tableCellStyle.normal.textColor = textColor;
            tableCellStyle.alignment = TextAnchor.MiddleCenter;
            tableCellStyle.padding = new RectOffset(5, 5, 6, 6);
            
            // Table row style
            tableRowStyle = new GUIStyle(GUI.skin.button);
            tableRowStyle.normal.background = tableRowTexture;
            tableRowStyle.hover.background = highlightTexture;
            tableRowStyle.active.background = highlightTexture;
            tableRowStyle.normal.textColor = textColor;
            tableRowStyle.hover.textColor = textColor;
            tableRowStyle.active.textColor = textColor;
            tableRowStyle.margin = new RectOffset(0, 0, 0, 0);
            tableRowStyle.padding = new RectOffset(0, 0, 0, 0);
            tableRowStyle.border = new RectOffset(0, 0, 0, 0);
            
            // Alternate row style
            tableRowAltStyle = new GUIStyle(tableRowStyle);
            tableRowAltStyle.normal.background = tableRowAltTexture;
        }
        
        private void InitializeButtonStyles(Color textColor)
        {
            // Button style
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.background = buttonBackgroundTexture;
            buttonStyle.hover.background = buttonHoverTexture;
            buttonStyle.active.background = buttonActiveTexture;
            buttonStyle.normal.textColor = textColor;
            buttonStyle.hover.textColor = textColor;
            buttonStyle.active.textColor = textColor;
            buttonStyle.padding = new RectOffset(10, 10, 6, 6);
            buttonStyle.fontSize = 12;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.border = new RectOffset(1, 1, 1, 1);
            
            // Action button style (for "Go" buttons)
            actionButtonStyle = new GUIStyle(buttonStyle);
            actionButtonStyle.padding = new RectOffset(5, 5, 3, 3);
            actionButtonStyle.fontSize = 11;
        }
        
        private void InitializeScrollViewStyle()
        {
            scrollViewStyle = new GUIStyle(GUI.skin.scrollView);
            scrollViewStyle.normal.background = tableRowTexture;
        }
        
        private void InitializeToggleStyle(Color textColor)
        {
            toggleStyle = new GUIStyle(GUI.skin.toggle);
            toggleStyle.normal.textColor = textColor;
            toggleStyle.onNormal.textColor = textColor;
            toggleStyle.hover.textColor = textColor;
            toggleStyle.onHover.textColor = textColor;
            toggleStyle.active.textColor = textColor;
            toggleStyle.onActive.textColor = textColor;
            toggleStyle.focused.textColor = textColor;
            toggleStyle.onFocused.textColor = textColor;
            toggleStyle.fontSize = 12;
            toggleStyle.fontStyle = FontStyle.Bold;
            toggleStyle.padding = new RectOffset(0, 0, 0, 0);
            toggleStyle.margin = new RectOffset(0, 0, 0, 0);
            toggleStyle.alignment = TextAnchor.MiddleLeft;
        }
        
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        
        // Style getters
        public GUIStyle WindowStyle => windowStyle;
        public GUIStyle HeaderStyle => headerStyle;
        public GUIStyle LabelStyle => labelStyle;
        public GUIStyle InputFieldStyle => inputFieldStyle;
        public GUIStyle TableHeaderStyle => tableHeaderStyle;
        public GUIStyle TableCellStyle => tableCellStyle;
        public GUIStyle TableRowStyle => tableRowStyle;
        public GUIStyle TableRowAltStyle => tableRowAltStyle;
        public GUIStyle ButtonStyle => buttonStyle;
        public GUIStyle ActionButtonStyle => actionButtonStyle;
        public GUIStyle TitleBarStyle => titleBarStyle;
        public GUIStyle ScrollViewStyle => scrollViewStyle;
        public GUIStyle ToggleStyle => toggleStyle;
        
        // Texture getters
        public Texture2D HeaderBackgroundTexture => headerBackgroundTexture;
        public Texture2D TableHeaderTexture => tableHeaderTexture;
        public Texture2D CloseButtonTexture => closeButtonTexture;
        public Texture2D CloseButtonHoverTexture => closeButtonHoverTexture;
        
        // Animation properties
        public float AnimationTime => animationTime;
        public bool ShowingSuccessMessage => showSuccessMessage;
        public float SuccessMessageTime => successMessageTime;
        public string SuccessMessageText => successMessage;
        
        // Scroll position
        public Vector2 ScrollPosition 
        { 
            get => scrollPosition; 
            set => scrollPosition = value; 
        }
        
        // Window dragging
        public void SetDragging(bool dragging)
        {
            isDraggingWindow = dragging;
        }
        
        // Window position updates
        public void UpdateGuiRect(Rect newRect)
        {
            guiRect = newRect;
        }
        
        public void UpdateTogglesRect(Rect newRect)
        {
            togglesRect = newRect;
        }
        
        public void UpdateSettingsRect(Rect newRect)
        {
            settingsRect = newRect;
        }
    }
}