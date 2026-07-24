using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KurdostAI.Context
{
    /// <summary>
    /// Detects the user's intent from their request to generate appropriate prompts.
    /// </summary>
    public class IntentDetector
    {
        public enum IntentType
        {
            Analysis,      // Analyze project, scripts, architecture
            Edit,          // Modify existing code
            Create,        // Create new files/scripts/systems
            ErrorCheck,    // Fix errors, debug issues
            Explain,       // Explain code or concepts
            Refactor,      // Refactor code with dependency analysis
            Unknown        // Could not determine intent
        }

        /// <summary>
        /// Detect the intent from the user's message.
        /// </summary>
        public IntentDetectionResult DetectIntent(string userMessage)
        {
            var result = new IntentDetectionResult
            {
                OriginalMessage = userMessage,
                Intent = IntentType.Unknown,
                Confidence = 0.0f,
                Keywords = new List<string>(),
                Context = new Dictionary<string, string>()
            };

            var message = userMessage.ToLower();

            // Analysis patterns
            var analysisPatterns = new List<string>
            {
                @"\b(analyze|analysis|examine|inspect|review|audit|check|investigate)\b",
                @"\b(architecture|structure|design|overview|summary)\b",
                @"\b(dependencies|relations|connections|graph)\b",
                @"\b(how does|how do|what is|explain)\b"
            };

            // Edit patterns
            var editPatterns = new List<string>
            {
                @"\b(change|modify|update|edit|alter|adjust)\b",
                @"\b(fix|correct|repair|resolve)\b",
                @"\b(replace|swap|substitute)\b",
                @"\b(remove|delete|eliminate)\b"
            };

            // Create patterns
            var createPatterns = new List<string>
            {
                @"\b(create|make|build|generate|add|implement)\b",
                @"\b(new|fresh|additional)\b",
                @"\b(script|class|system|feature|function)\b",
                @"\b(from scratch|from beginning)\b"
            };

            // Error check patterns
            var errorPatterns = new List<string>
            {
                @"\b(error|exception|bug|issue|problem|crash)\b",
                @"\b(debug|troubleshoot|diagnose)\b",
                @"\b(not working|broken|failed|failure)\b",
                @"\b(compilation|compile|build)\b"
            };

            // Explain patterns
            var explainPatterns = new List<string>
            {
                @"\b(explain|describe|tell me about|what does)\b",
                @"\b(understand|clarify|elaborate)\b",
                @"\b(meaning|purpose|function|role)\b"
            };

            // Refactor patterns
            var refactorPatterns = new List<string>
            {
                @"\b(refactor|restructure|reorganize|optimize)\b",
                @"\b(clean|improve|enhance|better)\b",
                @"\b(best practice|pattern|design)\b"
            };

            // Score each intent
            float analysisScore = ScoreIntent(message, analysisPatterns);
            float editScore = ScoreIntent(message, editPatterns);
            float createScore = ScoreIntent(message, createPatterns);
            float errorScore = ScoreIntent(message, errorPatterns);
            float explainScore = ScoreIntent(message, explainPatterns);
            float refactorScore = ScoreIntent(message, refactorPatterns);

            // Determine the highest scoring intent
            float maxScore = Math.Max(analysisScore, Math.Max(editScore, Math.Max(createScore, 
                Math.Max(errorScore, Math.Max(explainScore, refactorScore)))));

            if (maxScore < 0.3f)
            {
                result.Intent = IntentType.Unknown;
                result.Confidence = 0.0f;
            }
            else if (maxScore == analysisScore)
            {
                result.Intent = IntentType.Analysis;
                result.Confidence = analysisScore;
                ExtractKeywords(message, analysisPatterns, result.Keywords);
            }
            else if (maxScore == editScore)
            {
                result.Intent = IntentType.Edit;
                result.Confidence = editScore;
                ExtractKeywords(message, editPatterns, result.Keywords);
            }
            else if (maxScore == createScore)
            {
                result.Intent = IntentType.Create;
                result.Confidence = createScore;
                ExtractKeywords(message, createPatterns, result.Keywords);
            }
            else if (maxScore == errorScore)
            {
                result.Intent = IntentType.ErrorCheck;
                result.Confidence = errorScore;
                ExtractKeywords(message, errorPatterns, result.Keywords);
            }
            else if (maxScore == explainScore)
            {
                result.Intent = IntentType.Explain;
                result.Confidence = explainScore;
                ExtractKeywords(message, explainPatterns, result.Keywords);
            }
            else if (maxScore == refactorScore)
            {
                result.Intent = IntentType.Refactor;
                result.Confidence = refactorScore;
                ExtractKeywords(message, refactorPatterns, result.Keywords);
            }

            // Extract context information
            ExtractContext(userMessage, result.Context);

            return result;
        }

        private float ScoreIntent(string message, List<string> patterns)
        {
            float score = 0f;
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(message, pattern, RegexOptions.IgnoreCase);
                score += matches.Count * 0.3f;
            }
            return Math.Min(score, 1.0f);
        }

        private void ExtractKeywords(string message, List<string> patterns, List<string> keywords)
        {
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(message, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 0)
                    {
                        var keyword = match.Value;
                        if (!keywords.Contains(keyword))
                        {
                            keywords.Add(keyword);
                        }
                    }
                }
            }
        }

        private void ExtractContext(string message, Dictionary<string, string> context)
        {
            // Extract script/class names (capitalized words)
            var classPattern = @"\b([A-Z]\w+)\b";
            var classMatches = Regex.Matches(message, classPattern);
            if (classMatches.Count > 0)
            {
                var classNames = new List<string>();
                foreach (Match match in classMatches)
                {
                    classNames.Add(match.Value);
                }
                context["mentioned_classes"] = string.Join(", ", classNames);
            }

            // Extract file paths
            var pathPattern = @"[\w/\\]+\.(cs|unity|prefab|mat)";
            var pathMatches = Regex.Matches(message, pathPattern);
            if (pathMatches.Count > 0)
            {
                var paths = new List<string>();
                foreach (Match match in pathMatches)
                {
                    paths.Add(match.Value);
                }
                context["mentioned_files"] = string.Join(", ", paths);
            }

            // Extract Unity-specific terms
            var unityTerms = new[] { "monobehaviour", "scriptableobject", "gameobject", "transform", "rigidbody", "collider", "animator", "prefab", "scene" };
            var foundUnityTerms = new List<string>();
            foreach (var term in unityTerms)
            {
                if (message.ToLower().Contains(term))
                {
                    foundUnityTerms.Add(term);
                }
            }
            if (foundUnityTerms.Count > 0)
            {
                context["unity_terms"] = string.Join(", ", foundUnityTerms);
            }
        }
    }

    /// <summary>
    /// Result of intent detection.
    /// </summary>
    [System.Serializable]
    public class IntentDetectionResult
    {
        public string OriginalMessage;
        public IntentDetector.IntentType Intent;
        public float Confidence; // 0.0 to 1.0
        public List<string> Keywords;
        public Dictionary<string, string> Context; // Additional context extracted from the message
    }
}
