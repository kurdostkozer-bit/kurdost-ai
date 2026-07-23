using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;

public class KurdostAIMainWindow : EditorWindow
{
    private Vector2 _scrollPosition = Vector2.zero;
    private Vector2 _chatScrollPosition = Vector2.zero;
    private string _userMessage = "";
    private string _apiKeyInput = "";
    private string _settingsApiKeyInput = "";
    private int _selectedTab = 0;
    private List<ChatMessage> _chatHistory = new List<ChatMessage>();

    // Colors
    private static readonly Color HEADER_COLOR = new Color(0.2f, 0.6f, 1.0f, 1.0f);
    private static readonly Color TAB_ACTIVE = new Color(0.2f, 0.6f, 1.0f, 1.0f);
    private static readonly Color TAB_INACTIVE = new Color(0.2f, 0.2f, 0.2f, 1.0f);
    private static readonly Color SECTION_BG = new Color(0.15f, 0.15f, 0.15f, 1.0f);
    private static readonly Color INPUT_BG = new Color(0.1f, 0.1f, 0.1f, 1.0f);
    private static readonly Color SUCCESS_COLOR = new Color(0.2f, 0.8f, 0.3f, 1.0f);
    private static readonly Color ERROR_COLOR = new Color(1.0f, 0.3f, 0.3f, 1.0f);

    // Styles
    private GUIStyle _headerStyle;
    private GUIStyle _tabStyle;
    private GUIStyle _sectionStyle;
    private GUIStyle _buttonStyle;

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
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 15, 15),
        };

        _tabStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 11,
            padding = new RectOffset(15, 15, 8, 8),
            margin = new RectOffset(2, 2, 2, 2),
        };

        _sectionStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(12, 12, 12, 12),
            margin = new RectOffset(5, 5, 5, 5),
        };

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(10, 10, 8, 8),
            fixedHeight = 35,
        };
    }

    private void OnGUI()
    {
        if (_headerStyle == null)
        {
            InitializeStyles();
        }

        bool isAuthenticated = EditorPrefs.HasKey("KurdostAI_ApiKey");

        if (!isAuthenticated)
        {
            DrawAuthenticationPanel();
            return;
        }

        // Header
        GUI.backgroundColor = HEADER_COLOR;
        EditorGUILayout.LabelField("🤖 Kurdost AI Assistant", _headerStyle, GUILayout.Height(35));
        GUI.backgroundColor = Color.white;

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

                if (GUILayout.Button("💾 Save API Key", _buttonStyle))
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

                if (GUILayout.Button("🔗 Get Free API Key ↗", GUILayout.Height(30)))
                {
                    Application.OpenURL("https://console.groq.com/keys");
                }
            }
        }
    }

    private void DrawChatTab()
    {
        EditorGUILayout.LabelField("Chat with AI", EditorStyles.boldLabel, GUILayout.Height(25));
        EditorGUILayout.Space(10);

        using (new EditorGUILayout.VerticalScope(_sectionStyle, GUILayout.ExpandHeight(true)))
        {
            _chatScrollPosition = EditorGUILayout.BeginScrollView(_chatScrollPosition, GUILayout.ExpandHeight(true));

            if (_chatHistory.Count == 0)
            {
                GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
                EditorGUILayout.LabelField(
                    "Start a conversation\n\nType your message below and press Send to chat with Kurdost AI",
                    EditorStyles.helpBox,
                    GUILayout.Height(60)
                );
                GUI.backgroundColor = Color.white;
            }
            else
            {
                foreach (var msg in _chatHistory)
                {
                    DrawChatMessage(msg);
                    EditorGUILayout.Space(5);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Your Message:", EditorStyles.label);
        GUI.backgroundColor = INPUT_BG;
        _userMessage = EditorGUILayout.TextArea(_userMessage, GUILayout.Height(70));
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("🚀 Send Message", _buttonStyle))
            {
                if (!string.IsNullOrEmpty(_userMessage))
                {
                    SendChatMessage();
                }
            }

            if (GUILayout.Button("Clear", GUILayout.Height(35)))
            {
                _userMessage = "";
                _chatHistory.Clear();
            }
        }
    }

    private void DrawChatMessage(ChatMessage msg)
    {
        if (msg.IsUser)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.Space(5);
                GUI.backgroundColor = new Color(0.2f, 0.5f, 0.8f, 0.3f);
                EditorGUILayout.TextArea(msg.Content, EditorStyles.helpBox, GUILayout.MinHeight(40));
                GUI.backgroundColor = Color.white;
            }
        }
        else
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.Space(5);
                GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.3f);
                EditorGUILayout.TextArea(msg.Content, EditorStyles.helpBox, GUILayout.MinHeight(40));
                GUI.backgroundColor = Color.white;
            }
        }
    }

    private void SendChatMessage()
    {
        if (string.IsNullOrEmpty(_userMessage))
            return;

        _chatHistory.Add(new ChatMessage { Content = _userMessage, IsUser = true });

        string userMsg = _userMessage;
        _userMessage = "";

        _chatHistory.Add(new ChatMessage { Content = "Loading...", IsUser = false });

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
        string apiUrl = "https://kurdost-ai-backend.onrender.com/api/v1/chat";
        string apiKey = EditorPrefs.GetString("KurdostAI_ApiKey", "");
        string provider = EditorPrefs.GetString("KurdostAI_Provider", "groq");

        if (string.IsNullOrEmpty(apiKey))
        {
            UpdateAIResponse("Error: No API Key provided", isError: true);
            return;
        }

        // Manually construct JSON to avoid JsonUtility limitations
        string escapedMessage = message.Replace("\"", "\\\"");
        string jsonBody = $"{{\"provider\":\"{provider}\",\"messages\":[{{\"role\":\"user\",\"content\":\"{escapedMessage}\"}}]}}";
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

        _chatHistory.Add(new ChatMessage { Content = response, IsUser = false });

        Repaint();
    }

    private void DrawToolsTab()
    {
        EditorGUILayout.LabelField("Analysis Tools", EditorStyles.boldLabel, GUILayout.Height(20));

        using (new EditorGUILayout.VerticalScope(_sectionStyle))
        {
            EditorGUILayout.LabelField("Quick Actions:", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            if (GUILayout.Button("📝 Analyze Selected Script", _buttonStyle))
            {
                EditorUtility.DisplayDialog("Coming Soon", "Script analysis coming in v0.4", "OK");
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("🐛 Fix Console Errors", _buttonStyle))
            {
                EditorUtility.DisplayDialog("Coming Soon", "Error analysis coming in v0.4", "OK");
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("✨ Generate Script", _buttonStyle))
            {
                EditorUtility.DisplayDialog("Coming Soon", "Code generation coming in v0.4", "OK");
            }

            EditorGUILayout.Space(15);
            EditorGUILayout.HelpBox("Advanced tools coming in v0.4", MessageType.Info);
        }
    }

    private void DrawSettingsTab()
    {
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel, GUILayout.Height(20));

        using (new EditorGUILayout.VerticalScope(_sectionStyle))
        {
            EditorGUILayout.LabelField("API Configuration", EditorStyles.boldLabel);

            bool hasKey = EditorPrefs.HasKey("KurdostAI_ApiKey");
            GUI.backgroundColor = hasKey ? SUCCESS_COLOR : ERROR_COLOR;
            EditorGUILayout.LabelField(
                hasKey ? "✓ API Key Configured" : "✗ No API Key",
                EditorStyles.helpBox
            );
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(15);

            EditorGUILayout.TextField("Server:", "https://kurdost-ai-backend.onrender.com");
            EditorGUILayout.TextField("Status:", "🟢 Connected");

            EditorGUILayout.Space(15);

            EditorGUILayout.LabelField("Change API Key", EditorStyles.boldLabel);
            _settingsApiKeyInput = EditorGUILayout.PasswordField(_settingsApiKeyInput);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save New Key", GUILayout.Height(30)))
                {
                    if (!string.IsNullOrEmpty(_settingsApiKeyInput))
                    {
                        EditorPrefs.SetString("KurdostAI_ApiKey", _settingsApiKeyInput);
                        _settingsApiKeyInput = "";
                        EditorUtility.DisplayDialog("Success", "API Key updated successfully!", "OK");
                        Repaint();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Please enter an API key", "OK");
                    }
                }

                if (GUILayout.Button("Logout", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Logout", "Remove API key?", "Yes", "No"))
                    {
                        EditorPrefs.DeleteKey("KurdostAI_ApiKey");
                        _chatHistory.Clear();
                        Repaint();
                    }
                }
            }

            EditorGUILayout.Space(15);

            if (GUILayout.Button("🔗 Open Groq Console", GUILayout.Height(30)))
            {
                Application.OpenURL("https://console.groq.com/keys");
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("📂 Open Repository", GUILayout.Height(30)))
            {
                Application.OpenURL("https://github.com/kurdostkozer-bit/kurdost-ai");
            }
        }
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
    }
}
