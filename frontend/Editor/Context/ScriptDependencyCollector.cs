using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace KurdostAI.Context
{
    /// <summary>
    /// Analyzes dependencies between C# scripts to build a dependency graph.
    /// This helps AI understand how scripts relate to each other (using statements, inheritance, field types).
    /// </summary>
    public class ScriptDependencyCollector
    {
        private readonly string _projectPath;

        public ScriptDependencyCollector()
        {
            _projectPath = Application.dataPath;
        }

        /// <summary>
        /// Collect dependency information for all C# scripts in the project.
        /// </summary>
        public ScriptDependencyData Collect(int maxScripts = 3)
        {
            var data = new ScriptDependencyData
            {
                Dependencies = new List<ScriptDependency>()
            };

            // Find all .cs files
            var csFiles = Directory.GetFiles(_projectPath, "*.cs", SearchOption.AllDirectories);

            // Limit to maxScripts to avoid overwhelming the context
            var filesToAnalyze = csFiles.Take(maxScripts).ToList();

            foreach (var file in filesToAnalyze)
            {
                try
                {
                    var relativePath = file.Substring(_projectPath.Length + 1).Replace("\\", "/");
                    var content = File.ReadAllText(file);
                    var fileName = Path.GetFileNameWithoutExtension(file);

                    var dependency = new ScriptDependency
                    {
                        ScriptName = fileName,
                        ScriptPath = relativePath,
                        UsingStatements = ExtractUsingStatements(content),
                        BaseClass = ExtractBaseClass(content),
                        ReferencedTypes = ExtractReferencedTypes(content)
                    };

                    data.Dependencies.Add(dependency);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ScriptDependencyCollector] Failed to analyze {file}: {ex.Message}");
                }
            }

            return data;
        }

        /// <summary>
        /// Extract using statements from script content.
        /// </summary>
        private List<string> ExtractUsingStatements(string content)
        {
            var usingPattern = @"using\s+([a-zA-Z0-9_.]+);";
            var matches = Regex.Matches(content, usingPattern);
            
            return matches.Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Extract base class from class definition.
        /// </summary>
        private string ExtractBaseClass(string content)
        {
            var classPattern = @"class\s+\w+\s*:\s*(\w+)";
            var match = Regex.Match(content, classPattern);
            
            return match.Success ? match.Groups[1].Value : "";
        }

        /// <summary>
        /// Extract referenced types from field declarations and method signatures.
        /// </summary>
        private List<string> ExtractReferencedTypes(string content)
        {
            var referencedTypes = new List<string>();

            // Extract field types
            var fieldPattern = @"public\s+(\w+)\s+\w+";
            var fieldMatches = Regex.Matches(content, fieldPattern);
            foreach (Match match in fieldMatches)
            {
                var type = match.Groups[1].Value;
                if (!IsUnityType(type) && !IsCSharpPrimitive(type))
                {
                    referencedTypes.Add(type);
                }
            }

            // Extract method parameter types
            var paramPattern = @"\(([^)]+)\)";
            var paramMatches = Regex.Matches(content, paramPattern);
            foreach (Match match in paramMatches)
            {
                var parameters = match.Groups[1].Value.Split(',');
                foreach (var param in parameters)
                {
                    var parts = param.Trim().Split(' ');
                    if (parts.Length >= 2)
                    {
                        var type = parts[0];
                        if (!IsUnityType(type) && !IsCSharpPrimitive(type))
                        {
                            referencedTypes.Add(type);
                        }
                    }
                }
            }

            return referencedTypes.Distinct().ToList();
        }

        /// <summary>
        /// Check if type is a Unity built-in type.
        /// </summary>
        private bool IsUnityType(string type)
        {
            var unityTypes = new HashSet<string>
            {
                "GameObject", "Transform", "MonoBehaviour", "ScriptableObject",
                "Vector2", "Vector3", "Vector4", "Quaternion", "Color",
                "Rigidbody", "Collider", "Renderer", "Camera", "Light",
                "AudioSource", "AudioListener", "Animator", "Animation",
                "UI", "RectTransform", "Canvas", "Image", "Text",
                "Button", "InputField", "Slider", "Toggle"
            };
            
            return unityTypes.Contains(type);
        }

        /// <summary>
        /// Check if type is a C# primitive type.
        /// </summary>
        private bool IsCSharpPrimitive(string type)
        {
            var primitives = new HashSet<string>
            {
                "int", "float", "double", "bool", "string", "char",
                "long", "short", "byte", "object", "void"
            };
            
            return primitives.Contains(type);
        }
    }

    /// <summary>
    /// Container for script dependency data.
    /// </summary>
    [System.Serializable]
    public class ScriptDependencyData
    {
        public List<ScriptDependency> Dependencies;
    }

    /// <summary>
    /// Dependency information for a single script.
    /// </summary>
    [System.Serializable]
    public class ScriptDependency
    {
        public string ScriptName;
        public string ScriptPath;
        public List<string> UsingStatements;
        public string BaseClass;
        public List<string> ReferencedTypes;
    }
}
