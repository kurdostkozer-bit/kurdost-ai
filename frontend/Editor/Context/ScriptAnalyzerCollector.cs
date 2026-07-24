using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace KurdostAI.Context
{
    /// <summary>
    /// Analyzes C# scripts to extract structural information (namespace, class, methods, fields).
    /// This enables AI to understand script structure without reading full file contents.
    /// </summary>
    public class ScriptAnalyzerCollector
    {
        private readonly string _projectPath;

        public ScriptAnalyzerCollector()
        {
            _projectPath = Application.dataPath;
        }

        /// <summary>
        /// Analyze all C# scripts in the project.
        /// </summary>
        public ScriptAnalysisData Collect(int maxScripts = 3)
        {
            var data = new ScriptAnalysisData
            {
                Scripts = new List<ScriptMetadata>()
            };

            // Find all .cs files
            var csFiles = Directory.GetFiles(_projectPath, "*.cs", SearchOption.AllDirectories);

            // Limit to maxScripts to avoid overwhelming the context
            var filesToAnalyze = csFiles.Take(maxScripts).ToArray();

            foreach (var file in filesToAnalyze)
            {
                try
                {
                    var relativePath = file.Substring(_projectPath.Length + 1).Replace("\\", "/");
                    var metadata = AnalyzeScript(file, relativePath);
                    if (metadata != null)
                    {
                        data.Scripts.Add(metadata);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ScriptAnalyzerCollector] Failed to analyze {file}: {ex.Message}");
                }
            }

            return data;
        }

        /// <summary>
        /// Analyze a single script file.
        /// </summary>
        private ScriptMetadata AnalyzeScript(string filePath, string relativePath)
        {
            var content = File.ReadAllText(filePath);
            var metadata = new ScriptMetadata
            {
                Name = Path.GetFileName(filePath),
                Path = relativePath,
                Size = new FileInfo(filePath).Length
            };

            // Extract namespace
            metadata.Namespace = ExtractNamespace(content);

            // Extract class name
            metadata.Class = ExtractClassName(content);

            // Extract base class
            metadata.BaseClass = ExtractBaseClass(content);

            // Extract methods
            metadata.Methods = ExtractMethods(content);

            // Extract serialized fields
            metadata.SerializedFields = ExtractSerializedFields(content);

            return metadata;
        }

        private string ExtractNamespace(string content)
        {
            var match = Regex.Match(content, @"namespace\s+([a-zA-Z0-9_.]+)");
            return match.Success ? match.Groups[1].Value : "";
        }

        private string ExtractClassName(string content)
        {
            var match = Regex.Match(content, @"class\s+([a-zA-Z0-9_]+)");
            return match.Success ? match.Groups[1].Value : "";
        }

        private string ExtractBaseClass(string content)
        {
            var match = Regex.Match(content, @"class\s+[a-zA-Z0-9_]+\s*:\s*([a-zA-Z0-9_]+)");
            return match.Success ? match.Groups[1].Value : "";
        }

        private List<string> ExtractMethods(string content)
        {
            var methods = new List<string>();
            var matches = Regex.Matches(content, @"(?:public|private|protected|internal)\s+(?:static\s+)?(?:async\s+)?(?:void|[a-zA-Z0-9_<>]+)\s+([a-zA-Z0-9_]+)\s*\(");

            foreach (Match match in matches)
            {
                methods.Add(match.Groups[1].Value);
            }

            return methods;
        }

        private List<string> ExtractSerializedFields(string content)
        {
            var fields = new List<string>();
            var matches = Regex.Matches(content, @"\[SerializeField\]\s*(?:public|private)\s+([a-zA-Z0-9_<>[\]]+)\s+([a-zA-Z0-9_]+)");

            foreach (Match match in matches)
            {
                var fieldType = match.Groups[1].Value;
                var fieldName = match.Groups[2].Value;
                fields.Add($"{fieldType} {fieldName}");
            }

            return fields;
        }
    }

    /// <summary>
    /// Container for script analysis data.
    /// </summary>
    [System.Serializable]
    public class ScriptAnalysisData
    {
        public List<ScriptMetadata> Scripts;
    }

    /// <summary>
    /// Metadata for a single script.
    /// </summary>
    [System.Serializable]
    public class ScriptMetadata
    {
        public string Name;
        public string Path;
        public long Size;
        public string Namespace;
        public string Class;
        public string BaseClass;
        public List<string> Methods;
        public List<string> SerializedFields;
    }
}
