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
        public ScriptDependencyData Collect(int maxScripts = 15)
        {
            var data = new ScriptDependencyData
            {
                Dependencies = new List<ScriptDependency>()
            };

            // Find all .cs files
            var csFiles = Directory.GetFiles(_projectPath, "*.cs", SearchOption.AllDirectories);

            // Limit to maxScripts to avoid overwhelming the context
            var filesToAnalyze = csFiles.Take(maxScripts).ToList();

            // First pass: collect all dependencies
            var allDependencies = new Dictionary<string, ScriptDependency>();
            
            foreach (var file in filesToAnalyze)
            {
                try
                {
                    var relativePath = file.Substring(_projectPath.Length + 1).Replace("\\", "/");
                    var content = File.ReadAllText(file);
                    
                    // Remove string literals and comments before analysis
                    var cleanedContent = RemoveStringLiteralsAndComments(content);
                    
                    var fileName = Path.GetFileNameWithoutExtension(file);

                    var dependency = new ScriptDependency
                    {
                        ScriptName = fileName,
                        ScriptPath = relativePath,
                        UsingStatements = ExtractUsingStatements(cleanedContent),
                        BaseClass = ExtractBaseClass(cleanedContent),
                        ReferencedTypes = new List<string>(),
                        ReferencedFields = new List<string>(),
                        ReferencedMethods = new List<string>(),
                        ObjectCreations = new List<string>(),
                        Attributes = new List<string>(),
                        DependsOn = new List<string>(),
                        ReferencedBy = new List<string>()
                    };

                    ExtractReferences(cleanedContent, dependency);

                    allDependencies[fileName] = dependency;
                    data.Dependencies.Add(dependency);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ScriptDependencyCollector] Failed to analyze {file}: {ex.Message}");
                }
            }

            // Second pass: build dependency graph
            foreach (var dependency in data.Dependencies)
            {
                // Find scripts that this script depends on
                foreach (var refType in dependency.ReferencedTypes)
                {
                    if (allDependencies.ContainsKey(refType))
                    {
                        dependency.DependsOn.Add(refType);
                    }
                }

                // Add base class as dependency
                if (!string.IsNullOrEmpty(dependency.BaseClass) && allDependencies.ContainsKey(dependency.BaseClass))
                {
                    dependency.DependsOn.Add(dependency.BaseClass);
                }
            }

            // Build ReferencedBy (reverse dependencies)
            foreach (var dependency in data.Dependencies)
            {
                foreach (var depOn in dependency.DependsOn)
                {
                    if (allDependencies.ContainsKey(depOn))
                    {
                        allDependencies[depOn].ReferencedBy.Add(dependency.ScriptName);
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Remove string literals and comments from code to avoid false positives.
        /// </summary>
        private string RemoveStringLiteralsAndComments(string content)
        {
            // Remove multi-line comments /* ... */
            content = Regex.Replace(content, @"/\*[\s\S]*?\*/", "");
            
            // Remove single-line comments // ...
            content = Regex.Replace(content, @"//.*$", "", RegexOptions.Multiline);
            
            // Remove regular string literals "..."
            content = Regex.Replace(content, @"""[^""\\]*(?:\\.[^""\\]*)*""", "");
            
            // Remove interpolated strings $"..."
            content = Regex.Replace(content, @"\$""[^""\\]*(?:\\.[^""\\]*)*""", "");
            
            // Remove character literals '...'
            content = Regex.Replace(content, @"'[^']'", "");
            
            return content;
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
        /// Extract all references from script content and separate them by type.
        /// </summary>
        private void ExtractReferences(string content, ScriptDependency dependency)
        {
            var csharpKeywords = new HashSet<string>
            {
                "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
                "checked", "class", "const", "continue", "decimal", "default", "delegate",
                "do", "double", "else", "enum", "event", "explicit", "extern", "false",
                "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
                "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
                "new", "null", "object", "operator", "out", "override", "params", "private",
                "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
                "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
                "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
                "unsafe", "ushort", "using", "var", "virtual", "void", "volatile", "while"
            };

            var attributeParams = new HashSet<string>
            {
                "fileName", "menuName", "order", "subfolder", "icon", "helpURL",
                "color", "headerImage", "headerText", "headerTextHeight",
                "min", "max", "step", "tooltip", "radius", "angle", "number",
                "e", "event", "args", "context", "obj", "target", "source"
            };

            var unityMethods = new HashSet<string>
            {
                "GetComponent", "GetComponentInChildren", "GetComponentInParent",
                "GetComponents", "GetComponentsInChildren", "GetComponentsInParent",
                "SendMessage", "BroadcastMessage", "Invoke", "InvokeRepeating",
                "CancelInvoke", "StartCoroutine", "StopCoroutine", "StopAllCoroutines",
                "Instantiate", "Destroy", "DestroyImmediate", "DontDestroyOnLoad",
                "FindObjectOfType", "FindObjectsOfType", "FindGameObjectWithTag",
                "FindGameObjectsWithTag", "Log", "LogError", "LogWarning"
            };

            // Extract field types (public/private/protected fields)
            var fieldPattern = @"(public|private|protected|internal)\s+([A-Z]\w+)\s+\w+";
            var fieldMatches = Regex.Matches(content, fieldPattern);
            foreach (Match match in fieldMatches)
            {
                var type = match.Groups[2].Value;
                if (!csharpKeywords.Contains(type) && !IsUnityType(type) && !IsCSharpPrimitive(type) && !attributeParams.Contains(type))
                {
                    dependency.ReferencedTypes.Add(type);
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
                        if (!csharpKeywords.Contains(type) && !IsUnityType(type) && !IsCSharpPrimitive(type) && !attributeParams.Contains(type))
                        {
                            dependency.ReferencedTypes.Add(type);
                        }
                    }
                }
            }

            // Extract generic type parameters (List<T>, Dictionary<K,V>)
            var genericPattern = @"([A-Z]\w+)<[^>]+>";
            var genericMatches = Regex.Matches(content, genericPattern);
            foreach (Match match in genericMatches)
            {
                var type = match.Groups[1].Value;
                if (!csharpKeywords.Contains(type) && !IsUnityType(type) && !IsCSharpPrimitive(type) && !attributeParams.Contains(type))
                {
                    dependency.ReferencedTypes.Add(type);
                }
            }

            // Extract field references (this.fieldName, fieldName) - lowercase identifiers
            var fieldRefPattern = @"(?<!\w)([a-z]\w+)(?=\s*[=;,\)])";
            var fieldRefMatches = Regex.Matches(content, fieldRefPattern);
            foreach (Match match in fieldRefMatches)
            {
                var field = match.Groups[1].Value;
                if (!csharpKeywords.Contains(field) && !attributeParams.Contains(field))
                {
                    dependency.ReferencedFields.Add(field);
                }
            }

            // Extract method calls (methodName()) - exclude Unity methods
            var methodCallPattern = @"(?<!\w)([A-Z]\w+)\s*\(";
            var methodCallMatches = Regex.Matches(content, methodCallPattern);
            foreach (Match match in methodCallMatches)
            {
                var method = match.Groups[1].Value;
                if (!csharpKeywords.Contains(method) && !IsUnityType(method) && !IsCSharpPrimitive(method) && !attributeParams.Contains(method) && !unityMethods.Contains(method))
                {
                    dependency.ReferencedMethods.Add(method);
                }
            }

            // Extract object creation (new Type())
            var objectCreationPattern = @"new\s+([A-Z]\w+)";
            var objectCreationMatches = Regex.Matches(content, objectCreationPattern);
            foreach (Match match in objectCreationMatches)
            {
                var type = match.Groups[1].Value;
                if (!csharpKeywords.Contains(type) && !IsUnityType(type) && !IsCSharpPrimitive(type) && !attributeParams.Contains(type))
                {
                    dependency.ObjectCreations.Add(type);
                }
            }

            // Extract attributes (Header, Range, ContextMenu, etc.)
            var attributePattern = @"\[(\w+)";
            var attributeMatches = Regex.Matches(content, attributePattern);
            foreach (Match match in attributeMatches)
            {
                var attribute = match.Groups[1].Value;
                if (char.IsUpper(attribute[0])) // Attributes start with uppercase
                {
                    dependency.Attributes.Add(attribute);
                }
            }

            // Remove duplicates
            dependency.ReferencedTypes = dependency.ReferencedTypes.Distinct().ToList();
            dependency.ReferencedFields = dependency.ReferencedFields.Distinct().ToList();
            dependency.ReferencedMethods = dependency.ReferencedMethods.Distinct().ToList();
            dependency.ObjectCreations = dependency.ObjectCreations.Distinct().ToList();
            dependency.Attributes = dependency.Attributes.Distinct().ToList();
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
        public List<string> ReferencedTypes; // Only actual type references (classes, interfaces)
        public List<string> ReferencedFields; // Field references
        public List<string> ReferencedMethods; // Method references
        public List<string> ObjectCreations; // Object creation (new keyword)
        public List<string> Attributes; // Attribute names (Header, Range, ContextMenu, etc.)
        public List<string> DependsOn; // Scripts this script depends on
        public List<string> ReferencedBy; // Scripts that depend on this script
    }
}
