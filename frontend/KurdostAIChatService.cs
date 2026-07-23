using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace KurdostAI
{
    /// <summary>
    /// Service for handling chat conversations with Groq AI
    /// MVP v0.2: Multi-purpose assistant with secure API key handling
    /// FIXED: API key no longer sent in request body
    /// </summary>
    public class KurdostAIChatService
    {
        // ✅ Backend URL - Production on Render
        private string _chatUrl = "https://kurdost-ai-backend.onrender.com/api/v1/chat";
        private const int TIMEOUT = 60;
        private Dictionary<UnityWebRequest, Action<string>> _pendingRequests = 
            new Dictionary<UnityWebRequest, Action<string>>();
        private List<ChatTurn> _conversationHistory = new List<ChatTurn>();

        private string _apiKey = "";
        private string _currentModel = "llama-3.1-8b-instant";  // ✅ Updated model
        private string _currentProvider = "groq";  // ✅ Added provider selection
        private float _temperature = 0.7f;
        private int _maxTokens = 1000;
        private bool _initialized = false;

        public KurdostAIChatService()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.update += CheckPendingRequests;
            #endif
        }

        /// <summary>
        /// Initialize the chat service with API key and configuration
        /// </summary>
        public void Initialize(string apiKey, string model, float temperature, int maxTokens, string provider = "groq")
        {
            _apiKey = apiKey;
            _currentModel = model;
            _currentProvider = provider;
            _temperature = temperature;
            _maxTokens = maxTokens;
            _initialized = true;

            Debug.Log($"[KurdostAI] Chat Service initialized with provider: {_currentProvider}, model: {_currentModel}");
        }

        /// <summary>
        /// Update the AI model
        /// </summary>
        public void UpdateModel(string model)
        {
            _currentModel = model;
            Debug.Log($"[KurdostAI] Model updated to: {_currentModel}");
        }

        /// <summary>
        /// Update temperature and max tokens
        /// </summary>
        public void UpdateSettings(float temperature, int maxTokens)
        {
            _temperature = temperature;
            _maxTokens = maxTokens;
            Debug.Log($"[KurdostAI] Settings updated - Temp: {_temperature}, Tokens: {_maxTokens}");
        }

        /// <summary>
        /// Get response from AI
        /// </summary>
        public void GetResponse(string userMessage, Action<string> onResponse)
        {
            if (!_initialized)
            {
                onResponse?.Invoke("Error: Service not initialized.");
                return;
            }
            
            // Note: API key is no longer used by the client
            // Backend handles authentication server-side
            // See BACKEND_SECURITY_REQUIREMENTS.md for details

            // Add user message to history
            _conversationHistory.Add(new ChatTurn
            {
                Role = "user",
                Content = userMessage
            });

            // Build request with new API format
            var request = new ChatRequestBody
            {
                provider = _currentProvider,  // ✅ Send provider
                messages = _conversationHistory.ToArray(),
                model = _currentModel,
                temperature = _temperature,
                max_tokens = _maxTokens
            };

            SendChatRequest(request, (response) =>
            {
                // Add AI response to history
                _conversationHistory.Add(new ChatTurn
                {
                    Role = "assistant",
                    Content = response
                });

                onResponse?.Invoke(response);
            });
        }

        /// <summary>
        /// Clear conversation history
        /// </summary>
        public void ClearHistory()
        {
            _conversationHistory.Clear();
            Debug.Log("[KurdostAI] Conversation history cleared");
        }

        private void SendChatRequest(ChatRequestBody requestBody, Action<string> onResponse)
        {
            // SECURITY: API key NOT sent in request body
            string jsonData = JsonUtility.ToJson(requestBody);
            #if UNITY_EDITOR && KURDOST_DEBUG
            Debug.Log($"[KurdostAI] Sending request to: {CHAT_URL}");
            Debug.Log($"[KurdostAI] Request body: {jsonData}");
            #endif

            var www = new UnityWebRequest(_chatUrl, "POST");
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            // SECURITY: API key can be passed via header if backend supports it
            // For now, backend uses environment variable
            www.timeout = TIMEOUT;

            var asyncOp = www.SendWebRequest();
            _pendingRequests[www] = (responseText) =>
            {
                try
                {
                    #if UNITY_EDITOR && KURDOST_DEBUG
                    Debug.Log($"[KurdostAI] Raw response: {responseText}");
                    #endif
                    
                    // Check if response is an error message (not JSON)
                    if (responseText.StartsWith("Error:"))
                    {
                        onResponse?.Invoke(responseText);
                        return;
                    }
                    
                    var response = JsonUtility.FromJson<ChatResponse>(responseText);
                    if (response != null && response.success)
                    {
                        onResponse?.Invoke(response.message);
                    }
                    else
                    {
                        onResponse?.Invoke("Error: " + (response?.error ?? "Unknown error"));
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[KurdostAI] JSON Parse Error: {ex.Message}");
                    #if UNITY_EDITOR && KURDOST_DEBUG
                    Debug.LogError($"[KurdostAI] Response was: {responseText}");
                    #endif
                    onResponse?.Invoke("Error: Failed to parse response - " + ex.Message);
                }
            };
        }

        private void CheckPendingRequests()
        {
            var completedRequests = new List<UnityWebRequest>();

            foreach (var kvp in _pendingRequests)
            {
                var www = kvp.Key;
                if (www.isDone)
                {
                    completedRequests.Add(www);

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        string responseText = www.downloadHandler.text;
                        Debug.Log($"[KurdostAI] Response success: {responseText}");
                        kvp.Value?.Invoke(responseText);
                    }
                    else
                    {
                        string errorMsg = $"HTTP {www.responseCode}: {www.error}";
                        string responseBody = www.downloadHandler?.text ?? "";
                        if (!string.IsNullOrEmpty(responseBody))
                        {
                            errorMsg += $"\nBody: {responseBody}";
                        }
                        Debug.LogError($"[KurdostAI] {errorMsg}");
                        kvp.Value?.Invoke("Error: " + errorMsg);
                    }

                    www.Dispose();
                }
            }

            foreach (var www in completedRequests)
            {
                _pendingRequests.Remove(www);
            }
        }

        public void Cleanup()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= CheckPendingRequests;
            #endif
        }

        /// <summary>
        /// Represents a single chat message
        /// </summary>
        public class ChatMessage
        {
            public string Content { get; set; }
            public bool IsUser { get; set; }
            public System.DateTime Timestamp { get; set; }
        }
    }

    [System.Serializable]
    public class ChatTurn
    {
        public string role;
        public string content;

        public string Role
        {
            get => role;
            set => role = value;
        }

        public string Content
        {
            get => content;
            set => content = value;
        }
    }

    [System.Serializable]
    public class ChatRequestBody
    {
        public string provider;  // ✅ Added provider (groq, gemini, openrouter)
        public ChatTurn[] messages;
        public string model;
        public float temperature;
        public int max_tokens;
    }

    [System.Serializable]
    public class ChatResponse
    {
        public bool success;
        public string message;
        public string error;
    }
}
