using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KurdostAI.Context
{
    /// <summary>
    /// Builds architectural summaries from context data to improve AI understanding.
    /// This layer analyzes the collected data and generates high-level insights about the project structure.
    /// </summary>
    public class KnowledgeLayer
    {
        /// <summary>
        /// Build an architectural summary from the collected context data.
        /// </summary>
        public ArchitecturalSummary BuildSummary(UnityEditorContext context)
        {
            var summary = new ArchitecturalSummary
            {
                ProjectInfo = BuildProjectInfo(context),
                Architecture = BuildArchitectureInfo(context),
                KeySystems = IdentifyKeySystems(context),
                Dependencies = BuildDependencyAnalysis(context),
                Patterns = IdentifyDesignPatterns(context),
                Recommendations = GenerateRecommendations(context)
            };

            return summary;
        }

        private ProjectInfo BuildProjectInfo(UnityEditorContext context)
        {
            return new ProjectInfo
            {
                Name = context.Project.ProjectName,
                UnityVersion = context.Project.UnityVersion,
                Platform = context.Project.Platform,
                ScriptingRuntime = context.Project.ScriptingRuntimeVersion,
                RenderPipeline = context.ProjectSettings.RenderPipeline,
                InputSystem = context.ProjectSettings.InputSystem,
                TotalScripts = context.ScriptAnalysis?.Scripts?.Count ?? 0,
                TotalScenes = context.AssetTypes?.Scenes?.Count ?? 0,
                TotalPrefabs = context.AssetTypes?.Prefabs?.Count ?? 0
            };
        }

        private ArchitectureInfo BuildArchitectureInfo(UnityEditorContext context)
        {
            var info = new ArchitectureInfo
            {
                FolderStructure = AnalyzeFolderStructure(context.ProjectStructure),
                ScriptOrganization = AnalyzeScriptOrganization(context.ScriptAnalysis),
                AssetDistribution = AnalyzeAssetDistribution(context.AssetTypes)
            };

            return info;
        }

        private List<KeySystem> IdentifyKeySystems(UnityEditorContext context)
        {
            var systems = new List<KeySystem>();

            if (context.ScriptDependencies?.Dependencies != null)
            {
                // Identify systems based on dependency clusters
                var dependencyGroups = context.ScriptDependencies.Dependencies
                    .GroupBy(d => d.BaseClass)
                    .Where(g => g.Count() > 1);

                foreach (var group in dependencyGroups)
                {
                    var system = new KeySystem
                    {
                        Name = $"{group.Key} System",
                        BaseClass = group.Key,
                        ComponentCount = group.Count(),
                        Components = group.Select(d => d.ScriptName).ToList()
                    };
                    systems.Add(system);
                }

                // Identify systems by folder structure
                if (context.ProjectStructure?.Folders != null)
                {
                    var scriptFolders = context.ProjectStructure.Folders
                        .Where(f => f.Path.Contains("Scripts") && f.Depth == 2)
                        .ToList();

                    foreach (var folder in scriptFolders)
                    {
                        var scriptsInFolder = context.ScriptAnalysis?.Scripts?
                            .Where(s => s.Path.StartsWith(folder.Path))
                            .ToList();

                        if (scriptsInFolder != null && scriptsInFolder.Count > 0)
                        {
                            var system = new KeySystem
                            {
                                Name = folder.Name,
                                BaseClass = "Folder-based",
                                ComponentCount = scriptsInFolder.Count,
                                Components = scriptsInFolder.Select(s => s.Name).ToList()
                            };
                            systems.Add(system);
                        }
                    }
                }
            }

            return systems;
        }

        private DependencyAnalysis BuildDependencyAnalysis(UnityEditorContext context)
        {
            var analysis = new DependencyAnalysis
            {
                TotalDependencies = 0,
                MostReferencedScripts = new List<string>(),
                CircularDependencies = new List<string>(),
                CouplingScore = 0.0f
            };

            if (context.ScriptDependencies?.Dependencies != null)
            {
                // Count total dependencies
                analysis.TotalDependencies = context.ScriptDependencies.Dependencies
                    .Sum(d => d.DependsOn.Count);

                // Find most referenced scripts
                var referenceCounts = context.ScriptDependencies.Dependencies
                    .SelectMany(d => d.ReferencedBy)
                    .GroupBy(r => r)
                    .Select(g => new { Script = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .Take(5);

                analysis.MostReferencedScripts = referenceCounts.Select(r => r.Script).ToList();

                // Calculate coupling score (average dependencies per script)
                var totalScripts = context.ScriptDependencies.Dependencies.Count;
                if (totalScripts > 0)
                {
                    analysis.CouplingScore = (float)analysis.TotalDependencies / totalScripts;
                }

                // Detect potential circular dependencies (simplified)
                foreach (var dep in context.ScriptDependencies.Dependencies)
                {
                    foreach (var dependsOn in dep.DependsOn)
                    {
                        var referencedScript = context.ScriptDependencies.Dependencies
                            .FirstOrDefault(d => d.ScriptName == dependsOn);
                        if (referencedScript != null && referencedScript.DependsOn.Contains(dep.ScriptName))
                        {
                            var cycle = $"{dep.ScriptName} <-> {dependsOn}";
                            if (!analysis.CircularDependencies.Contains(cycle))
                            {
                                analysis.CircularDependencies.Add(cycle);
                            }
                        }
                    }
                }
            }

            return analysis;
        }

        private List<string> IdentifyDesignPatterns(UnityEditorContext context)
        {
            var patterns = new List<string>();

            if (context.ScriptAnalysis?.Scripts != null)
            {
                // Singleton pattern detection
                var singletons = context.ScriptAnalysis.Scripts
                    .Where(s => s.Methods.Any(m => m.ToLower().Contains("instance")))
                    .Select(s => s.Name)
                    .ToList();
                if (singletons.Count > 0)
                {
                    patterns.Add($"Singleton Pattern: {string.Join(", ", singletons)}");
                }

                // Manager pattern detection
                var managers = context.ScriptAnalysis.Scripts
                    .Where(s => s.Name.ToLower().Contains("manager"))
                    .Select(s => s.Name)
                    .ToList();
                if (managers.Count > 0)
                {
                    patterns.Add($"Manager Pattern: {string.Join(", ", managers)}");
                }

                // Controller pattern detection
                var controllers = context.ScriptAnalysis.Scripts
                    .Where(s => s.Name.ToLower().Contains("controller"))
                    .Select(s => s.Name)
                    .ToList();
                if (controllers.Count > 0)
                {
                    patterns.Add($"Controller Pattern: {string.Join(", ", controllers)}");
                }

                // Observer pattern (events/delegates)
                var eventUsers = context.ScriptAnalysis.Scripts
                    .Where(s => s.Methods.Any(m => m.ToLower().Contains("add") || m.ToLower().Contains("subscribe")))
                    .Select(s => s.Name)
                    .ToList();
                if (eventUsers.Count > 0)
                {
                    patterns.Add($"Observer Pattern: {string.Join(", ", eventUsers)}");
                }
            }

            return patterns;
        }

        private List<string> GenerateRecommendations(UnityEditorContext context)
        {
            var recommendations = new List<string>();

            if (context.ScriptDependencies?.Dependencies != null)
            {
                // Check for high coupling
                var highCouplingScripts = context.ScriptDependencies.Dependencies
                    .Where(d => d.DependsOn.Count > 5)
                    .Select(d => d.ScriptName)
                    .ToList();
                if (highCouplingScripts.Count > 0)
                {
                    recommendations.Add($"Consider refactoring high-coupling scripts: {string.Join(", ", highCouplingScripts)}");
                }

                // Check for scripts with no dependencies (potential entry points)
                var entryPoints = context.ScriptDependencies.Dependencies
                    .Where(d => d.DependsOn.Count == 0 && d.BaseClass == "MonoBehaviour")
                    .Select(d => d.ScriptName)
                    .ToList();
                if (entryPoints.Count > 0)
                {
                    recommendations.Add($"Potential entry points (no dependencies): {string.Join(", ", entryPoints)}");
                }

                // Check for circular dependencies
                var circularDeps = BuildDependencyAnalysis(context).CircularDependencies;
                if (circularDeps.Count > 0)
                {
                    recommendations.Add($"Resolve circular dependencies: {string.Join(", ", circularDeps)}");
                }
            }

            if (context.ScriptAnalysis?.Scripts != null)
            {
                // Check for large scripts
                var largeScripts = context.ScriptAnalysis.Scripts
                    .Where(s => s.Size > 2000)
                    .Select(s => s.Name)
                    .ToList();
                if (largeScripts.Count > 0)
                {
                    recommendations.Add($"Consider splitting large scripts: {string.Join(", ", largeScripts)}");
                }
            }

            return recommendations;
        }

        private string AnalyzeFolderStructure(ProjectStructureData structure)
        {
            if (structure?.Folders == null) return "No folder structure data";

            var sb = new StringBuilder();
            var rootFolders = structure.Folders.Where(f => f.Depth == 1).ToList();
            sb.AppendLine($"Root folders: {rootFolders.Count}");
            foreach (var folder in rootFolders.Take(10))
            {
                sb.AppendLine($"  - {folder.Name}");
            }

            return sb.ToString();
        }

        private string AnalyzeScriptOrganization(ScriptAnalysisData analysis)
        {
            if (analysis?.Scripts == null) return "No script analysis data";

            var sb = new StringBuilder();
            var monoBehaviours = analysis.Scripts.Count(s => s.BaseClass == "MonoBehaviour");
            var scriptableObjects = analysis.Scripts.Count(s => s.BaseClass == "ScriptableObject");
            var editorScripts = analysis.Scripts.Count(s => s.BaseClass == "EditorWindow");

            sb.AppendLine($"Total scripts: {analysis.Scripts.Count}");
            sb.AppendLine($"MonoBehaviours: {monoBehaviours}");
            sb.AppendLine($"ScriptableObjects: {scriptableObjects}");
            sb.AppendLine($"Editor scripts: {editorScripts}");

            return sb.ToString();
        }

        private string AnalyzeAssetDistribution(AssetTypeData assets)
        {
            if (assets == null) return "No asset data";

            var sb = new StringBuilder();
            sb.AppendLine($"Scripts: {assets.Scripts?.Count ?? 0}");
            sb.AppendLine($"Scenes: {assets.Scenes?.Count ?? 0}");
            sb.AppendLine($"Prefabs: {assets.Prefabs?.Count ?? 0}");
            sb.AppendLine($"Materials: {assets.Materials?.Count ?? 0}");
            sb.AppendLine($"Shaders: {assets.Shaders?.Count ?? 0}");
            sb.AppendLine($"Textures: {assets.Textures?.Count ?? 0}");

            return sb.ToString();
        }
    }

    /// <summary>
    /// Comprehensive architectural summary of the project.
    /// </summary>
    [System.Serializable]
    public class ArchitecturalSummary
    {
        public ProjectInfo ProjectInfo;
        public ArchitectureInfo Architecture;
        public List<KeySystem> KeySystems;
        public DependencyAnalysis Dependencies;
        public List<string> Patterns;
        public List<string> Recommendations;

        public string ToSummaryString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== PROJECT ARCHITECTURAL SUMMARY ===");
            sb.AppendLine();
            sb.AppendLine("PROJECT INFO:");
            sb.AppendLine($"  Name: {ProjectInfo.Name}");
            sb.AppendLine($"  Unity Version: {ProjectInfo.UnityVersion}");
            sb.AppendLine($"  Platform: {ProjectInfo.Platform}");
            sb.AppendLine($"  Render Pipeline: {ProjectInfo.RenderPipeline}");
            sb.AppendLine($"  Input System: {ProjectInfo.InputSystem}");
            sb.AppendLine($"  Total Scripts: {ProjectInfo.TotalScripts}");
            sb.AppendLine();
            sb.AppendLine("KEY SYSTEMS:");
            foreach (var system in KeySystems)
            {
                sb.AppendLine($"  - {system.Name} ({system.ComponentCount} components)");
            }
            sb.AppendLine();
            sb.AppendLine("DEPENDENCIES:");
            sb.AppendLine($"  Total Dependencies: {Dependencies.TotalDependencies}");
            sb.AppendLine($"  Coupling Score: {Dependencies.CouplingScore:F2}");
            sb.AppendLine($"  Most Referenced: {string.Join(", ", Dependencies.MostReferencedScripts)}");
            sb.AppendLine();
            sb.AppendLine("DESIGN PATTERNS:");
            foreach (var pattern in Patterns)
            {
                sb.AppendLine($"  - {pattern}");
            }
            sb.AppendLine();
            sb.AppendLine("RECOMMENDATIONS:");
            foreach (var rec in Recommendations)
            {
                sb.AppendLine($"  - {rec}");
            }

            return sb.ToString();
        }
    }

    [System.Serializable]
    public class ProjectInfo
    {
        public string Name;
        public string UnityVersion;
        public string Platform;
        public string ScriptingRuntime;
        public string RenderPipeline;
        public string InputSystem;
        public int TotalScripts;
        public int TotalScenes;
        public int TotalPrefabs;
    }

    [System.Serializable]
    public class ArchitectureInfo
    {
        public string FolderStructure;
        public string ScriptOrganization;
        public string AssetDistribution;
    }

    [System.Serializable]
    public class KeySystem
    {
        public string Name;
        public string BaseClass;
        public int ComponentCount;
        public List<string> Components;
    }

    [System.Serializable]
    public class DependencyAnalysis
    {
        public int TotalDependencies;
        public List<string> MostReferencedScripts;
        public List<string> CircularDependencies;
        public float CouplingScore;
    }
}
