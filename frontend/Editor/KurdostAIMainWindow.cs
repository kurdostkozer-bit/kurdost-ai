using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using System;

public class KurdostAIMainWindow : EditorWindow
{
    private Vector2 _scrollPosition = Vector2.zero;
    private Vector2 _chatScrollPosition = Vector2.zero;
    private string _userMessage = "";
    private string _apiKeyInput = "";
    private string _settingsApiKeyInput = "";
    private int _selectedTab = 0;
    private List<ChatMessage> _chatHistory = new List<ChatMessage>();
    private bool _autoScroll = true;
    private bool _isLoading = false;
    private int _loadingFrame = 0;
    private float _animationProgress = 0f;
    private string[] _languages = { "English", "Arabic" };
    private int _selectedLanguage = 0;
    private string[] _themes = { "Dark", "Light" };
    private int _selectedTheme = 0;
    private List<Notification> _notifications = new List<Notification>();
    private float _notificationDisplayTime = 3f;
    private float _notificationTimer = 0f;

    // Colors - Modern Gradient Theme
    private static readonly Color HEADER_COLOR = new Color(0.1f, 0.5f, 0.9f, 1.0f);
    private static readonly Color HEADER_COLOR_END = new Color(0.4f, 0.2f, 0.8f, 1.0f);
    private static readonly Color TAB_ACTIVE = new Color(0.1f, 0.5f, 0.9f, 1.0f);
    private static readonly Color TAB_INACTIVE = new Color(0.3f, 0.3f, 0.35f, 1.0f);
    private static readonly Color SECTION_BG = new Color(0.12f, 0.12f, 0.15f, 1.0f);
    private static readonly Color INPUT_BG = new Color(0.08f, 0.08f, 0.1f, 1.0f);
    private static readonly Color SUCCESS_COLOR = new Color(0.2f, 0.8f, 0.4f, 1.0f);
    private static readonly Color ERROR_COLOR = new Color(0.95f, 0.3f, 0.3f, 1.0f);
    private static readonly Color WARNING_COLOR = new Color(0.95f, 0.7f, 0.2f, 1.0f);
    private static readonly Color INFO_COLOR = new Color(0.2f, 0.6f, 0.95f, 1.0f);
    private static readonly Color USER_MESSAGE_BG = new Color(0.15f, 0.45f, 0.85f, 0.25f);
    private static readonly Color AI_MESSAGE_BG = new Color(0.18f, 0.18f, 0.22f, 0.5f);
    private static readonly Color ERROR_MESSAGE_BG = new Color(0.8f, 0.15f, 0.15f, 0.3f);

    // Styles
    private GUIStyle _headerStyle;
    private GUIStyle _tabStyle;
    private GUIStyle _sectionStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _messageStyle;
    private GUIStyle _userMessageStyle;
    private GUIStyle _aiMessageStyle;
    private GUIStyle _errorMessageStyle;
    private GUIStyle _inputStyle;
    private GUIStyle _labelStyle;

    private string[] _tabNames = { "💬 Chat", "🔧 Tools", "⚙️ Settings" };
    private UnityWebRequest _currentRequest;
    private System.DateTime _requestStartTime;
    private const float REQUEST_TIMEOUT = 30f;

    [MenuItem("Window/Kurdost AI/Main")]
    public static void ShowWindow()
    {
        GetWindow<KurdostAIMainWindow>("Kurdost AI");
        GetWindow<KurdostAIMainWindow>().minSize = new Vector2(400, 500);
    }

    private void OnEnable()
    {
        // Styles will be initialized on first OnGUI call
    }

    private void InitializeStyles()
    {
        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 18, 18),
            normal = { textColor = Color.white },
            hover = { textColor = Color.white }
        };

        _tabStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(18, 18, 10, 10),
            margin = new RectOffset(2, 2, 2, 2),
            normal = { textColor = Color.white },
            hover = { textColor = Color.white }
        };

        _sectionStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(16, 16, 16, 16),
            margin = new RectOffset(8, 8, 8, 8),
            normal = { background = MakeTexture(2, 2, SECTION_BG) }
        };

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(14, 14, 10, 10),
            fixedHeight = 38,
            normal = { textColor = Color.white },
            hover = { textColor = Color.white }
        };

        _messageStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
        {
            fontSize = 13,
            padding = new RectOffset(16, 16, 12, 12),
            wordWrap = true,
            richText = true
        };

        _userMessageStyle = new GUIStyle(_messageStyle)
        {
            normal = { background = MakeTexture(2, 2, USER_MESSAGE_BG) }
        };

        _aiMessageStyle = new GUIStyle(_messageStyle)
        {
            normal = { background = MakeTexture(2, 2, AI_MESSAGE_BG) }
        };

        _errorMessageStyle = new GUIStyle(_messageStyle)
        {
            normal = { background = MakeTexture(2, 2, ERROR_MESSAGE_BG) }
        };

        _inputStyle = new GUIStyle(EditorStyles.textArea)
        {
            fontSize = 13,
            padding = new RectOffset(12, 12, 10, 10),
            wordWrap = true,
            richText = true
        };

        _labelStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(0, 0, 4, 4)
        };
    }

    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private void OnGUI()
    {
        if (_headerStyle == null)
        {
            InitializeStyles();
        }

        // Handle keyboard shortcuts
        HandleKeyboardShortcuts();

        // Update loading animation
        if (_isLoading)
        {
            _loadingFrame++;
            Repaint();
        }

        // Update notifications
        UpdateNotifications();

        bool isAuthenticated = EditorPrefs.HasKey("KurdostAI_ApiKey");

        if (!isAuthenticated)
        {
            DrawAuthenticationPanel();
            return;
        }

        // Header with gradient effect
        DrawGradientHeader();

        EditorGUILayout.Space(10);

        // Tabs
        DrawTabs();

        EditorGUILayout.Space(10);

        // Content
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        switch (_selectedTab)
        {
            case 0:
                DrawChatTab();
                break;
            case 1:
                DrawToolsTab();
                break;
            case 2:
                DrawSettingsTab();
                break;
        }

        EditorGUILayout.EndScrollView();

        // Draw notifications
        DrawNotifications();
    }

    private void HandleKeyboardShortcuts()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            // Enter to send message (when not holding Shift for new line)
            if (e.keyCode == KeyCode.Return && !e.shift)
            {
                if (_selectedTab == 0 && !string.IsNullOrEmpty(_userMessage))
                {
                    SendChatMessage();
                    e.Use();
                }
            }

            // Ctrl+Enter to send message (alternative)
            if (e.control && e.keyCode == KeyCode.Return)
            {
                if (_selectedTab == 0 && !string.IsNullOrEmpty(_userMessage))
                {
                    SendChatMessage();
                    e.Use();
                }
            }

            // Ctrl+E to export chat
            if (e.control && e.keyCode == KeyCode.E)
            {
                if (_selectedTab == 0)
                {
                    ExportChatHistory();
                    e.Use();
                }
            }

            // Ctrl+N to clear chat
            if (e.control && e.keyCode == KeyCode.N)
            {
                if (_selectedTab == 0)
                {
                    if (EditorUtility.DisplayDialog("Clear Chat", "Are you sure you want to clear all messages?", "Yes", "No"))
                    {
                        _userMessage = "";
                        _chatHistory.Clear();
                    }
                    e.Use();
                }
            }

            // Ctrl+1 to switch to Chat tab
            if (e.control && e.keyCode == KeyCode.Alpha1)
            {
                _selectedTab = 0;
                Repaint();
                e.Use();
            }

            // Ctrl+2 to switch to Tools tab
            if (e.control && e.keyCode == KeyCode.Alpha2)
            {
                _selectedTab = 1;
                Repaint();
                e.Use();
            }

            // Ctrl+3 to switch to Settings tab
            if (e.control && e.keyCode == KeyCode.Alpha3)
            {
                _selectedTab = 2;
                Repaint();
                e.Use();
            }
        }
    }

    private void DrawGradientHeader()
    {
        Rect headerRect = EditorGUILayout.GetControlRect(false, 50);
        GUI.DrawTexture(headerRect, CreateGradientTexture((int)headerRect.width, (int)headerRect.height, HEADER_COLOR, HEADER_COLOR_END));
        GUI.Label(headerRect, "🤖 Kurdost AI Assistant", _headerStyle);
    }

    private Texture2D CreateGradientTexture(int width, int height, Color color1, Color color2)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] colors = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float t = (float)y / height;
            Color lerpedColor = Color.Lerp(color1, color2, t);

            for (int x = 0; x < width; x++)
            {
                colors[y * width + x] = lerpedColor;
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }

    private void DrawTabs()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            for (int i = 0; i < _tabNames.Length; i++)
            {
                GUI.backgroundColor = _selectedTab == i ? TAB_ACTIVE : TAB_INACTIVE;

                if (GUILayout.Button(_tabNames[i], _tabStyle, GUILayout.Height(30)))
                {
                    _selectedTab = i;
                }

                GUI.backgroundColor = Color.white;
            }
        }
    }

    private void DrawAuthenticationPanel()
    {
        EditorGUILayout.Space(50);

        using (new EditorGUILayout.VerticalScope())
        {
            GUI.backgroundColor = ERROR_COLOR;
            EditorGUILayout.LabelField("🔐 Authentication Required", _headerStyle, GUILayout.Height(50));
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(30);

            using (new EditorGUILayout.VerticalScope(_sectionStyle))
            {
                EditorGUILayout.LabelField(
                    "Welcome to Kurdost AI!\n\n" +
                    "Set up your Groq API key to get started.\n\n" +
                    "Get a free key from Groq Console.",
                    EditorStyles.helpBox,
                    GUILayout.Height(100)
                );

                EditorGUILayout.Space(20);

                EditorGUILayout.LabelField("API Key:", EditorStyles.label);
                _apiKeyInput = EditorGUILayout.PasswordField(_apiKeyInput);

                EditorGUILayout.Space(10);

                if (GUILayout.Button(new GUIContent("💾 Save API Key", "Save your Groq API key"), _buttonStyle))
                {
                    if (!string.IsNullOrEmpty(_apiKeyInput))
                    {
                        EditorPrefs.SetString("KurdostAI_ApiKey", _apiKeyInput);
                        EditorUtility.DisplayDialog("✅ Success", "API Key saved! Window will refresh.", "OK");
                        Repaint();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("❌ Error", "Please enter an API key", "OK");
                    }
                }

                EditorGUILayout.Space(10);

                if (GUILayout.Button(new GUIContent("🔗 Get Free API Key ↗", "Open Groq Console to get a free API key"), GUILayout.Height(30)))
                {
                    Application.OpenURL("https://console.groq.com/keys");
                }
            }
        }
    }

    private void DrawChatTab()
    {
        EditorGUILayout.LabelField("💬 Chat with AI", _labelStyle, GUILayout.Height(30));
        EditorGUILayout.Space(12);

        using (new EditorGUILayout.VerticalScope(_sectionStyle, GUILayout.ExpandHeight(true)))
        {
            _chatScrollPosition = EditorGUILayout.BeginScrollView(_chatScrollPosition, GUILayout.ExpandHeight(true));

            if (_chatHistory.Count == 0)
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();
                    
                    GUI.backgroundColor = new Color(0.15f, 0.45f, 0.85f, 0.15f);
                    EditorGUILayout.LabelField(
                        "🚀 Start a conversation\n\nType your message below and press Enter to chat with Kurdost AI",
                        _messageStyle,
                        GUILayout.Height(80)
                    );
                    GUI.backgroundColor = Color.white;
                    
                    GUILayout.FlexibleSpace();
                }
            }
            else
            {
                foreach (var msg in _chatHistory)
                {
                    DrawChatMessage(msg);
                    EditorGUILayout.Space(8);
                }
            }

            // Loading indicator
            if (_isLoading)
            {
                DrawLoadingIndicator();
            }

            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space(12);

        EditorGUILayout.LabelField("✍️ Your Message:", _labelStyle);
        GUI.backgroundColor = INPUT_BG;
        _userMessage = EditorGUILayout.TextArea(_userMessage, _inputStyle, GUILayout.Height(85));
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(12);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(new GUIContent("🚀 Send Message", "Send message (Enter)"), _buttonStyle))
            {
                if (!string.IsNullOrEmpty(_userMessage))
                {
                    SendChatMessage();
                }
            }

            if (GUILayout.Button(new GUIContent("📤 Export", "Export chat history (Ctrl+E)"), _buttonStyle))
            {
                ExportChatHistory();
            }

            if (GUILayout.Button(new GUIContent("🗑️ Clear", "Clear chat (Ctrl+N)"), _buttonStyle))
            {
                if (EditorUtility.DisplayDialog("Clear Chat", "Are you sure you want to clear all messages?", "Yes", "No"))
                {
                    _userMessage = "";
                    _chatHistory.Clear();
                    ShowNotification("Chat cleared", NotificationType.Info);
                }
            }
        }
    }

    private void DrawLoadingIndicator()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();

            // Animated loading dots with smooth transition
            float animationSpeed = 0.05f;
            _animationProgress += animationSpeed;
            if (_animationProgress > 1f)
                _animationProgress = 0f;

            string[] dots = { ".", "..", "...", "...." };
            int dotIndex = (int)(_animationProgress * dots.Length * 2) % dots.Length;
            string loadingText = "Loading" + dots[dotIndex];

            // Pulsing color effect
            float pulse = Mathf.Sin(_animationProgress * Mathf.PI * 2) * 0.3f + 0.7f;
            Color pulsingColor = new Color(0.2f, 0.6f, 1.0f, pulse);

            GUI.backgroundColor = pulsingColor;
            EditorGUILayout.LabelField(loadingText, _headerStyle, GUILayout.Width(120));
            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
        }

        EditorGUILayout.Space(12);
    }

    private void DrawChatMessage(ChatMessage msg)
    {
        using (new EditorGUILayout.VerticalScope())
        {
            EditorGUILayout.Space(8);

            // Header with timestamp and action buttons
            using (new EditorGUILayout.HorizontalScope())
            {
                if (msg.IsUser)
                {
                    EditorGUILayout.LabelField("👤 You", _labelStyle, GUILayout.Width(60));
                }
                else
                {
                    EditorGUILayout.LabelField(msg.IsError ? "❌ Error" : "🤖 AI", _labelStyle, GUILayout.Width(60));
                }

                if (!string.IsNullOrEmpty(msg.Timestamp))
                {
                    EditorGUILayout.LabelField(msg.Timestamp, EditorStyles.miniLabel, GUILayout.Width(60));
                }

                GUILayout.FlexibleSpace();

                // Retry button for error messages
                if (msg.IsError && !string.IsNullOrEmpty(msg.OriginalMessage))
                {
                    if (GUILayout.Button("🔄", GUILayout.Width(28), GUILayout.Height(24)))
                    {
                        RetryMessage(msg.OriginalMessage);
                        return;
                    }
                }

                // Copy button
                if (GUILayout.Button("📋", GUILayout.Width(28), GUILayout.Height(24)))
                {
                    GUIUtility.systemCopyBuffer = msg.Content;
                    ShowNotification("Message copied to clipboard", NotificationType.Success);
                }

                // Delete button
                if (GUILayout.Button("🗑️", GUILayout.Width(28), GUILayout.Height(24)))
                {
                    _chatHistory.Remove(msg);
                    ShowNotification("Message deleted", NotificationType.Info);
                    Repaint();
                    return;
                }
            }

            // Message content with word wrap and proper styling
            GUIStyle messageStyle;
            if (msg.IsUser)
            {
                messageStyle = _userMessageStyle;
            }
            else if (msg.IsError)
            {
                messageStyle = _errorMessageStyle;
            }
            else
            {
                messageStyle = _aiMessageStyle;
            }

            // Parse and display markdown for AI messages
            string displayContent = msg.IsUser ? msg.Content : ParseMarkdown(msg.Content);
            
            // Calculate required height for the message
            float contentHeight = messageStyle.CalcHeight(new GUIContent(displayContent), position.width - 80);
            float minHeight = Mathf.Max(40, contentHeight);
            
            EditorGUILayout.LabelField(displayContent, messageStyle, GUILayout.Height(minHeight));
        }
    }

    private string ParseMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        string result = text;

        // Code blocks ```code```
        result = System.Text.RegularExpressions.Regex.Replace(result, @"```([^`]+)```", "📦 $1");

        // Inline code `code`
        result = System.Text.RegularExpressions.Regex.Replace(result, @"`([^`]+)`", "🔹 $1");

        // Bold **text**
        result = System.Text.RegularExpressions.Regex.Replace(result, @"\*\*([^*]+)\*\*", "★ $1 ★");

        // Italic *text*
        result = System.Text.RegularExpressions.Regex.Replace(result, @"\*()\*", "» $1 «");

        // Headers # text
        result = System.Text.RegularExpressions.Regex.Replace(result, @"^#+\s+(.+)$", "▶ $1", System.Text.RegularExpressions.RegexOptions.Multiline);

        return result;
    }

    private void SendChatMessage()
    {
        if (string.IsNullOrEmpty(_userMessage))
            return;

        _chatHistory.Add(new ChatMessage { Content = _userMessage, IsUser = true, Timestamp = System.DateTime.Now.ToString("HH:mm:ss") });

        string userMsg = _userMessage;
        _userMessage = "";

        _chatHistory.Add(new ChatMessage { Content = "Loading...", IsUser = false, Timestamp = System.DateTime.Now.ToString("HH:mm:ss"), OriginalMessage = userMsg });

        _isLoading = true;
        _loadingFrame = 0;

        if (_autoScroll)
        {
            _chatScrollPosition = new Vector2(_chatScrollPosition.x, float.MaxValue);
        }

        Repaint();

        SendToBackendCoroutine(userMsg);
    }

    private void SendToBackendCoroutine(string message)
    {
        SendToBackend(message);
        EditorApplication.update += UpdateBackendRequest;
    }

    private void SendToBackend(string message)
    {
        string apiUrl = EditorPrefs.GetString("KurdostAI_ServerUrl", "https://kurdost-ai-backend.onrender.com/api/v1/chat");
        string apiKey = EditorPrefs.GetString("KurdostAI_ApiKey", "");
        string provider = EditorPrefs.GetString("KurdostAI_Provider", "groq");
        string model = EditorPrefs.GetString("KurdostAI_Model", "llama-3.1-8b-instant");
        float temperature = EditorPrefs.GetFloat("KurdostAI_Temperature", 0.7f);
        int maxTokens = EditorPrefs.GetInt("KurdostAI_MaxTokens", 1000);

        if (string.IsNullOrEmpty(apiKey))
        {
            UpdateAIResponse("Error: No API Key provided", isError: true);
            return;
        }

        // Manually construct JSON to avoid JsonUtility limitations
        string escapedMessage = message.Replace("\"", "\\\"");
        string jsonBody = $"{{\"provider\":\"{provider}\",\"messages\":[{{\"role\":\"user\",\"content\":\"{escapedMessage}\"}}],\"model\":\"{model}\",\"temperature\":{temperature:F1},\"max_tokens\":{maxTokens}}}";
        Debug.Log($"[KurdostAI] Request body: {jsonBody}");

        _currentRequest = new UnityWebRequest(apiUrl, "POST");
        _currentRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonBody));
        _currentRequest.downloadHandler = new DownloadHandlerBuffer();
        _currentRequest.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(apiKey))
        {
            _currentRequest.SetRequestHeader("X-API-Key", apiKey);
        }

        _currentRequest.SendWebRequest();
        _requestStartTime = System.DateTime.Now;

        Debug.Log($"[KurdostAI] Request sent to {apiUrl}");
    }

    private void UpdateBackendRequest()
    {
        if (_currentRequest == null)
        {
            EditorApplication.update -= UpdateBackendRequest;
            return;
        }

        float elapsed = (float)(System.DateTime.Now - _requestStartTime).TotalSeconds;
        if (elapsed > REQUEST_TIMEOUT)
        {
            _currentRequest.Abort();
            UpdateAIResponse("Error: Backend timeout", isError: true);
            EditorApplication.update -= UpdateBackendRequest;
            _currentRequest = null;
            return;
        }

        if (_currentRequest.isDone)
        {
            if (_currentRequest.result == UnityWebRequest.Result.Success)
            {
                string responseText = _currentRequest.downloadHandler.text;
                Debug.Log($"[KurdostAI] Response: {responseText}");

                try
                {
                    var response = JsonUtility.FromJson<ChatResponse>(responseText);
                    if (response.success && !string.IsNullOrEmpty(response.message))
                    {
                        UpdateAIResponse(response.message, isError: false);
                    }
                    else if (!string.IsNullOrEmpty(response.response))
                    {
                        UpdateAIResponse(response.response, isError: false);
                    }
                    else
                    {
                        UpdateAIResponse(response.error ?? "Error: No response from backend", isError: true);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[KurdostAI] Parse error: {ex}");
                    UpdateAIResponse(responseText, isError: false);
                }
            }
            else
            {
                string errorMsg = $"Backend Error: {_currentRequest.error} (Status: {_currentRequest.responseCode})";
                Debug.LogError($"[KurdostAI] {errorMsg}");
                UpdateAIResponse(errorMsg, isError: true);
            }

            _currentRequest.Dispose();
            _currentRequest = null;
            EditorApplication.update -= UpdateBackendRequest;
        }

        Repaint();
    }

    private void UpdateAIResponse(string response, bool isError)
    {
        if (_chatHistory.Count > 0 && _chatHistory[_chatHistory.Count - 1].Content.Contains("Loading"))
        {
            _chatHistory.RemoveAt(_chatHistory.Count - 1);
        }

        _chatHistory.Add(new ChatMessage { Content = response, IsUser = false, Timestamp = System.DateTime.Now.ToString("HH:mm:ss"), IsError = isError });

        _isLoading = false;

        if (_autoScroll)
        {
            _chatScrollPosition = new Vector2(_chatScrollPosition.x, float.MaxValue);
        }

        Repaint();
    }

    private void RetryMessage(string originalMessage)
    {
        _chatHistory.Add(new ChatMessage { Content = "Retrying...", IsUser = false, Timestamp = System.DateTime.Now.ToString("HH:mm:ss"), OriginalMessage = originalMessage });

        _isLoading = true;
        _loadingFrame = 0;

        if (_autoScroll)
        {
            _chatScrollPosition = new Vector2(_chatScrollPosition.x, float.MaxValue);
        }

        Repaint();

        SendToBackendCoroutine(originalMessage);
    }

    private void ExportChatHistory()
    {
        if (_chatHistory.Count == 0)
        {
            EditorUtility.DisplayDialog("Export", "No messages to export", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Export Chat History", "", "kurdost_chat", "json");
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"exportDate\": \"{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\",");
            sb.AppendLine($"  \"messageCount\": {_chatHistory.Count},");
            sb.AppendLine("  \"messages\": [");

            for (int i = 0; i < _chatHistory.Count; i++)
            {
                var msg = _chatHistory[i];
                string escapedContent = msg.Content.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
                sb.AppendLine("    {");
                sb.AppendLine($"      \"content\": \"{escapedContent}\",");
                sb.AppendLine($"      \"isUser\": {msg.IsUser.ToString().ToLower()},");
                sb.AppendLine($"      \"timestamp\": \"{msg.Timestamp}\",");
                sb.AppendLine($"      \"isError\": {msg.IsError.ToString().ToLower()}");
                sb.Append(i < _chatHistory.Count - 1 ? "    }," : "    }");
                sb.AppendLine();
            }

            sb.AppendLine("  ]");
            sb.AppendLine("}");

            System.IO.File.WriteAllText(path, sb.ToString());

            EditorUtility.DisplayDialog("Export Success", $"Chat history exported to:\n{path}", "OK");
            ShowNotification("Chat history exported successfully", NotificationType.Success);
            Debug.Log($"[KurdostAI] Chat history exported to: {path}");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Export Error", $"Failed to export: {ex.Message}", "OK");
            ShowNotification("Export failed", NotificationType.Error);
            Debug.LogError($"[KurdostAI] Export error: {ex}");
        }
    }

    private void DrawToolsTab()
    {
        EditorGUILayout.LabelField("🔧 Analysis Tools", _labelStyle, GUILayout.Height(30));
        EditorGUILayout.Space(12);

        using (new EditorGUILayout.VerticalScope(_sectionStyle))
        {
            EditorGUILayout.LabelField("⚡ Quick Actions:", _labelStyle);
            EditorGUILayout.Space(12);

            if (GUILayout.Button(new GUIContent("📝 Analyze Selected Script", "Analyze the selected C# script for code quality and improvements"), _buttonStyle))
            {
                AnalyzeSelectedScript();
            }

            EditorGUILayout.Space(8);

            if (GUILayout.Button(new GUIContent("🐛 Fix Console Errors", "Analyze and provide fixes for console errors"), _buttonStyle))
            {
                FixConsoleErrors();
            }

            EditorGUILayout.Space(8);

            if (GUILayout.Button(new GUIContent("✨ Generate Script", "Generate a new Unity script with AI assistance"), _buttonStyle))
            {
                GenerateScript();
            }

            EditorGUILayout.Space(16);
            
            GUI.backgroundColor = new Color(0.15f, 0.45f, 0.85f, 0.15f);
            EditorGUILayout.HelpBox("💡 Tip: Select a script in the Project view to analyze it", MessageType.Info);
            GUI.backgroundColor = Color.white;
        }
    }

    private void AnalyzeSelectedScript()
    {
        var selectedObject = Selection.activeObject;
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select a script in the Project view", "OK");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selectedObject);
        if (!assetPath.EndsWith(".cs"))
        {
            EditorUtility.DisplayDialog("Invalid Selection", "Please select a C# script file", "OK");
            return;
        }

        string scriptContent = System.IO.File.ReadAllText(assetPath);
        if (string.IsNullOrEmpty(scriptContent))
        {
            EditorUtility.DisplayDialog("Error", "Could not read script content", "OK");
            return;
        }

        // Add analysis request to chat
        _chatHistory.Add(new ChatMessage { Content = $"Analyzing script: {selectedObject.name}", IsUser =
true, Timestamp = System.DateTime.Now.ToString("HH:mm:ss") });

        string analysisPrompt = $"Analyze this C# script for Unity:\n\n{scriptContent}\n\nProvide feedback on code quality, potential issues, and suggestions for improvement.";

        _chatHistory.Add(new ChatMessage { Content = "Loading analysis...", IsUser = false, Timestamp = System.DateTime.Now.ToString("HH:mm:ss"), OriginalMessage = analysisPrompt });

        _isLoading = true;
        _loadingFrame = 0;

        if (_autoScroll)
        {
            _chatScrollPosition = new Vector2(_chatScrollPosition.x, float.MaxValue);
        }

        Repaint();

        SendToBackendCoroutine(analysisPrompt);
    }

    private void FixConsoleErrors()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor");
        if (logEntries == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not access console logs", "OK");
            return;
        }

        var startMethod = logEntries.GetMethod("Start", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        var endMethod = logEntries.GetMethod("End", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        var getEntryMethod = logEntries.GetMethod("GetEntry", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

        if (startMethod == null || endMethod == null || getEntryMethod == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not access console log methods", "OK");
            return;
        }

        int count = (int)startMethod.Invoke(null, null);
        System.Text.StringBuilder errorLog = new System.Text.StringBuilder();

        for (int i = 0; i < count; i++)
        {
            var entry = getEntryMethod.Invoke(null, new object[] { i });
            var modeProp = entry.GetType().GetProperty("mode");
            var messageProp = entry.GetType().GetProperty("message");

            if (modeProp != null && messageProp != null)
            {
                int mode = (int)modeProp.GetValue(entry);
                string message = (string)messageProp.GetValue(entry);

                if (mode == 1) // Error mode
                {
                    errorLog.AppendLine(message);
                }
            }
        }

        endMethod.Invoke(null, null);

        if (errorLog.Length == 0)
        {
            EditorUtility.DisplayDialog("No Errors", "No errors found in console", "OK");
            return;
        }

        // Add error fixing request to chat
        _chatHistory.Add(new ChatMessage { Content = "Fixing console errors", IsUser = true, Timestamp = System.DateTime.Now.ToString("HH:mm:ss") });

        string fixPrompt = $"Fix these Unity console errors:\n\n{errorLog.ToString()}\n\nProvide solutions for each error.";

        _chatHistory.Add(new ChatMessage { Content = "Loading fixes...", IsUser = false, Timestamp = System.DateTime.Now.ToString("HH:mm:ss"), OriginalMessage = fixPrompt });

        _isLoading = true;
        _loadingFrame = 0;

        if (_autoScroll)
        {
            _chatScrollPosition = new Vector2(_chatScrollPosition.x, float.MaxValue);
        }

        Repaint();

        SendToBackendCoroutine(fixPrompt);
    }

    private void GenerateScript()
    {
        // Switch to chat tab for script generation
        _selectedTab = 0;
        Repaint();

        // Add prompt message to chat
        _chatHistory.Add(new ChatMessage { Content = "I want to generate a Unity script. Please describe what you need.", IsUser = true, Timestamp = System.DateTime.Now.ToString("HH:mm:ss") });

        if (_autoScroll)
        {
            _chatScrollPosition = new Vector2(_chatScrollPosition.x, float.MaxValue);
        }

        Repaint();
    }

    private void DrawSettingsTab()
    {
        EditorGUILayout.LabelField("⚙️ Settings", _labelStyle, GUILayout.Height(30));
        EditorGUILayout.Space(12);

        using (new EditorGUILayout.VerticalScope(_sectionStyle))
        {
            EditorGUILayout.LabelField("🔑 API Configuration", _labelStyle);
            EditorGUILayout.Space(8);

            bool hasKey = EditorPrefs.HasKey("KurdostAI_ApiKey");
            GUI.backgroundColor = hasKey ? SUCCESS_COLOR : ERROR_COLOR;
            EditorGUILayout.LabelField(
                hasKey ? "✓ API Key Configured" : "✗ No API Key",
                EditorStyles.helpBox
            );
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(16);

            // Provider Selection
            EditorGUILayout.LabelField("🌐 Provider", _labelStyle);
            string[] providers = { "groq", "gemini" };
            int currentProviderIndex = Array.IndexOf(providers, EditorPrefs.GetString("KurdostAI_Provider", "groq"));
            int newProviderIndex = EditorGUILayout.Popup(currentProviderIndex, providers);
            if (newProviderIndex != currentProviderIndex)
            {
                if (EditorUtility.DisplayDialog("Change Provider", $"Change provider from {providers[currentProviderIndex]} to {providers[newProviderIndex]}?", "Yes", "No"))
                {
                    EditorPrefs.SetString("KurdostAI_Provider", providers[newProviderIndex]);
                    ShowNotification($"Provider changed to {providers[newProviderIndex]}", NotificationType.Success);
                    Repaint();
                }
            }

            EditorGUILayout.Space(12);

            // Model Selection
            EditorGUILayout.LabelField("🤖 Model", _labelStyle);
            string[] models = { "llama-3.1-8b-instant", "llama-3.1-70b-versatile", "mixtral-8x7b-32768" };
            int currentModelIndex = Array.IndexOf(models, EditorPrefs.GetString("KurdostAI_Model", "llama-3.1-8b-instant"));
            int newModelIndex = EditorGUILayout.Popup(currentModelIndex, models);
            if (newModelIndex != currentModelIndex)
            {
                if (EditorUtility.DisplayDialog("Change Model", $"Change model from {models[currentModelIndex]} to {models[newModelIndex]}?", "Yes", "No"))
                {
                    EditorPrefs.SetString("KurdostAI_Model", models[newModelIndex]);
                    ShowNotification($"Model changed to {models[newModelIndex]}", NotificationType.Success);
                    Repaint();
                }
            }

            EditorGUILayout.Space(12);

            // Temperature Control
            EditorGUILayout.LabelField("🌡️ Temperature", _labelStyle);
            float temperature = EditorPrefs.GetFloat("KurdostAI_Temperature", 0.7f);
            float newTemperature = EditorGUILayout.Slider(temperature, 0.0f, 2.0f);
            if (Mathf.Abs(newTemperature - temperature) > 0.01f)
            {
                EditorPrefs.SetFloat("KurdostAI_Temperature", newTemperature);
            }
            EditorGUILayout.LabelField($"Value: {newTemperature:F2}", EditorStyles.miniLabel);

            EditorGUILayout.Space(12);

            // Max Tokens Control
            EditorGUILayout.LabelField("📊 Max Tokens", _labelStyle);
            int maxTokens = EditorPrefs.GetInt("KurdostAI_MaxTokens", 1000);
            int newMaxTokens = EditorGUILayout.IntSlider(maxTokens, 100, 4000);
            if (newMaxTokens != maxTokens)
            {
                EditorPrefs.SetInt("KurdostAI_MaxTokens", newMaxTokens);
            }
            EditorGUILayout.LabelField($"Value: {newMaxTokens}", EditorStyles.miniLabel);

            EditorGUILayout.Space(16);

            // Custom Server URL
            EditorGUILayout.LabelField("🌍 Server URL", _labelStyle);
            string serverUrl = EditorPrefs.GetString("KurdostAI_ServerUrl", "https://kurdost-ai-backend.onrender.com/api/v1/chat");
            string newServerUrl = EditorGUILayout.TextField(serverUrl);
            if (newServerUrl != serverUrl)
            {
                EditorPrefs.SetString("KurdostAI_ServerUrl", newServerUrl);
            }

            EditorGUILayout.Space(16);

            // Server Status
            EditorGUILayout.LabelField("📡 Server Status", _labelStyle);
            bool isConnected = CheckServerConnection();
            GUI.backgroundColor = isConnected ? SUCCESS_COLOR : ERROR_COLOR;
            EditorGUILayout.LabelField(
                isConnected ? "🟢 Connected" : "🔴 Disconnected",
                EditorStyles.helpBox
            );
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(16);

            // Language Preference
            EditorGUILayout.LabelField("🌐 Language", _labelStyle);
            _selectedLanguage = EditorPrefs.GetInt("KurdostAI_Language", 0);
            int newLanguage = EditorGUILayout.Popup(_selectedLanguage, _languages);
            if (newLanguage != _selectedLanguage)
            {
                _selectedLanguage = newLanguage;
                EditorPrefs.SetInt("KurdostAI_Language", _selectedLanguage);
                Repaint();
            }

            EditorGUILayout.Space(12);

            // Theme Selection
            EditorGUILayout.LabelField("🎨 Theme", _labelStyle);
            _selectedTheme = EditorPrefs.GetInt("KurdostAI_Theme", 0);
            int newTheme = EditorGUILayout.Popup(_selectedTheme, _themes);
            if (newTheme != _selectedTheme)
            {
                _selectedTheme = newTheme;
                EditorPrefs.SetInt("KurdostAI_Theme", _selectedTheme);
                ApplyTheme();
                Repaint();
            }

            EditorGUILayout.Space(16);

            EditorGUILayout.LabelField("🔐 Change API Key", _labelStyle);
            _settingsApiKeyInput = EditorGUILayout.PasswordField(_settingsApiKeyInput);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("💾 Save New Key", "Save your new API key"), _buttonStyle))
                {
                    if (!string.IsNullOrEmpty(_settingsApiKeyInput))
                    {
                        EditorPrefs.SetString("KurdostAI_ApiKey", _settingsApiKeyInput);
                        _settingsApiKeyInput = "";
                        EditorUtility.DisplayDialog("✅ Success", "API Key updated successfully!", "OK");
                        ShowNotification("API Key updated successfully", NotificationType.Success);
                        Repaint();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("❌ Error", "Please enter an API key", "OK");
                    }
                }

                if (GUILayout.Button(new GUIContent("🚪 Logout", "Remove API key and logout"), _buttonStyle))
                {
                    if (EditorUtility.DisplayDialog("Logout", "Remove API key?", "Yes", "No"))
                    {
                        EditorPrefs.DeleteKey("KurdostAI_ApiKey");
                        _chatHistory.Clear();
                        ShowNotification("Logged out successfully", NotificationType.Info);
                        Repaint();
                    }
                }
            }

            EditorGUILayout.Space(16);

            if (GUILayout.Button(new GUIContent("🔗 Open Groq Console", "Get your API key from Groq Console"), _buttonStyle))
            {
                Application.OpenURL("https://console.groq.com/keys");
            }

            EditorGUILayout.Space(8);

            if (GUILayout.Button(new GUIContent("📂 Open Repository", "View the project on GitHub"), _buttonStyle))
            {
                Application.OpenURL("https://github.com/kurdostkozer-bit/kurdost-ai");
            }
        }
    }

    private bool CheckServerConnection()
    {
        string serverUrl = EditorPrefs.GetString("KurdostAI_ServerUrl", "https://kurdost-ai-backend.onrender.com/api/v1/chat");
        return !string.IsNullOrEmpty(serverUrl) && serverUrl.StartsWith("http");
    }

    private void ApplyTheme()
    {
        // Theme colors would be applied here based on selection
        // For now, we keep the dark theme as default
        // Light theme implementation can be added later
    }

    private void UpdateNotifications()
    {
        if (_notifications.Count > 0)
        {
            _notificationTimer += Time.deltaTime;

            if (_notificationTimer >= 0.1f)
            {
                _notificationTimer = 0f;

                for (int i = _notifications.Count - 1; i >= 0; i--)
                {
                    _notifications[i].TimeLeft -= 0.1f;

                    if (_notifications[i].TimeLeft <= 0)
                    {
                        _notifications.RemoveAt(i);
                    }
                }

                if (_notifications.Count > 0)
                {
                    Repaint();
                }
            }
        }
    }

    private void DrawNotifications()
    {
        if (_notifications.Count == 0)
            return;

        float notificationWidth = 300f;
        float notificationHeight = 60f;
        float spacing = 10f;
        float startY = 10f;

        for (int i = 0; i < _notifications.Count; i++)
        {
            var notification = _notifications[i];
            Rect notificationRect = new Rect(
                position.width - notificationWidth - 10f,
                startY + (i * (notificationHeight + spacing)),
                notificationWidth,
                notificationHeight
            );

            Color bgColor;
            switch (notification.Type)
            {
                case NotificationType.Success:
                    bgColor = SUCCESS_COLOR;
                    break;
                case NotificationType.Error:
                    bgColor = ERROR_COLOR;
                    break;
                case NotificationType.Warning:
                    bgColor = new Color(1.0f, 0.8f, 0.2f, 1.0f);
                    break;
                default:
                    bgColor = new Color(0.2f, 0.6f, 1.0f, 1.0f);
                    break;
            }

            GUI.backgroundColor = new Color(bgColor.r, bgColor.g, bgColor.b, 0.9f);
            GUI.Box(notificationRect, "");
            GUI.backgroundColor = Color.white;

            GUIStyle notificationStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 10, 10),
                wordWrap = true
            };

            GUI.Label(notificationRect, notification.Message, notificationStyle);
        }
    }

    private void ShowNotification(string message, NotificationType type)
    {
        _notifications.Add(new Notification
        {
            Message = message,
            Type = type,
            TimeLeft = _notificationDisplayTime
        });

        Repaint();
    }

    [System.Serializable]
    private class ChatResponse
    {
        public bool success;
        public string message;
        public string response;
        public string model;
        public int tokens_used;
        public string error;
    }

    private class ChatMessage
    {
        public string Content { get; set; }
        public bool IsUser { get; set; }
        public string Timestamp { get; set; }
        public bool IsError { get; set; }
        public string OriginalMessage { get; set; }
    }

    private class Notification
    {
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public float TimeLeft { get; set; }
    }

    private enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }
}
