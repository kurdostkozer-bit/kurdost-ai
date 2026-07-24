using System;
using System.Collections.Generic;

namespace KurdostAI.Context
{
    /// <summary>
    /// Optimizes context by sending only relevant data based on request type and intent.
    /// This reduces token usage and improves AI response quality by focusing on relevant information.
    /// </summary>
    public class ContextOptimizer
    {
        /// <summary>
        /// Optimize the context based on the detected intent and user message.
        /// Returns a filtered context with only relevant data.
        /// </summary>
        public UnityEditorContext OptimizeContext(UnityEditorContext fullContext, IntentDetector.IntentType intent, IntentDetectionResult intentResult)
        {
            var optimizedContext = new UnityEditorContext
            {
                Project = fullContext.Project,
                Scene = fullContext.Scene,
                Selection = fullContext.Selection,
                Console = fullContext.Console,
                ProjectStructure = fullContext.ProjectStructure,
                Packages = fullContext.Packages,
                ProjectSettings = fullContext.ProjectSettings,
                Hierarchy = fullContext.Hierarchy,
                ScriptAnalysis = fullContext.ScriptAnalysis,
                AssetTypes = fullContext.AssetTypes,
                ScriptDependencies = fullContext.ScriptDependencies,
                ArchitecturalSummary = fullContext.ArchitecturalSummary
            };

            // Apply intent-specific optimizations
            switch (intent)
            {
                case IntentDetector.IntentType.Analysis:
                    optimizedContext = OptimizeForAnalysis(optimizedContext, intentResult);
                    break;
                case IntentDetector.IntentType.Edit:
                    optimizedContext = OptimizeForEdit(optimizedContext, intentResult);
                    break;
                case IntentDetector.IntentType.Create:
                    optimizedContext = OptimizeForCreate(optimizedContext, intentResult);
                    break;
                case IntentDetector.IntentType.ErrorCheck:
                    optimizedContext = OptimizeForErrorCheck(optimizedContext, intentResult);
                    break;
                case IntentDetector.IntentType.Explain:
                    optimizedContext = OptimizeForExplain(optimizedContext, intentResult);
                    break;
                case IntentDetector.IntentType.Refactor:
                    optimizedContext = OptimizeForRefactor(optimizedContext, intentResult);
                    break;
                default:
                    optimizedContext = OptimizeForGeneral(optimizedContext, intentResult);
                    break;
            }

            return optimizedContext;
        }

        private UnityEditorContext OptimizeForAnalysis(UnityEditorContext context, IntentDetectionResult intentResult)
        {
            // Analysis needs full architectural context
            // Keep everything but limit detailed data
            context.ScriptAnalysis = LimitScriptAnalysis(context.ScriptAnalysis, maxScripts: 20);
            context.ScriptDependencies = context.ScriptDependencies; // Keep all for dependency analysis
            context.ProjectStructure = LimitProjectStructure(context.ProjectStructure, maxDepth: 3);
            context.ArchitecturalSummary = context.ArchitecturalSummary; // Keep summary
            return context;
        }

        private UnityEditorContext OptimizeForEdit(UnityEditorContext context, IntentDetectionResult intentResult)
        {
            // Edit needs focused context on mentioned scripts and their dependencies
            if (intentResult.Context.ContainsKey("mentioned_classes"))
            {
                var mentionedClasses = intentResult.Context["mentioned_classes"].Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                context.ScriptAnalysis = FilterScriptAnalysisByClasses(context.ScriptAnalysis, mentionedClasses);
                context.ScriptDependencies = FilterScriptDependenciesByClasses(context.ScriptDependencies, mentionedClasses);
            }
            else
            {
                context.ScriptAnalysis = LimitScriptAnalysis(context.ScriptAnalysis, maxScripts: 10);
            }

            context.Console = context.Console; // Keep console for error context
            context.Selection = context.Selection; // Keep selection for context
            context.ProjectStructure = LimitProjectStructure(context.ProjectStructure, maxDepth: 2);
            return context;
        }

        private UnityEditorContext OptimizeForCreate(UnityEditorContext context, IntentDetectionResult intentResult)
        {
            // Create needs project structure and existing similar scripts
            context.ProjectStructure = context.ProjectStructure; // Keep full structure for placement
            context.ScriptAnalysis = LimitScriptAnalysis(context.ScriptAnalysis, maxScripts: 5); // Just a few examples
            context.ScriptDependencies = null; // Not needed for creation
            context.Console = null; // Not needed for creation
            context.ArchitecturalSummary = context.ArchitecturalSummary; // Keep for architectural guidance
            return context;
        }

        private UnityEditorContext OptimizeForErrorCheck(UnityEditorContext context, IntentDetectionResult intentResult)
        {
            // Error check needs console errors and related scripts
            context.Console = context.Console; // Keep full console
            context.ScriptAnalysis = context.ScriptAnalysis; // Keep all for error analysis
            context.ScriptDependencies = context.ScriptDependencies; // Keep for dependency analysis
            context.ProjectStructure = null; // Not needed
            context.AssetTypes = null; // Not needed
            return context;
        }

        private UnityEditorContext OptimizeForExplain(UnityEditorContext context, IntentDetectionResult intentResult)
        {
            // Explain needs focused context on mentioned items
            if (intentResult.Context.ContainsKey("mentioned_classes"))
            {
                var mentionedClasses = intentResult.Context["mentioned_classes"].Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                context.ScriptAnalysis = FilterScriptAnalysisByClasses(context.ScriptAnalysis, mentionedClasses);
                context.ScriptDependencies = FilterScriptDependenciesByClasses(context.ScriptDependencies, mentionedClasses);
            }
            else
            {
                context.ScriptAnalysis = LimitScriptAnalysis(context.ScriptAnalysis, maxScripts: 5);
            }

            context.ArchitecturalSummary = context.ArchitecturalSummary; // Keep for context
            context.ProjectStructure = LimitProjectStructure(context.ProjectStructure, maxDepth: 2);
            return context;
        }

        private UnityEditorContext OptimizeForRefactor(UnityEditorContext context, IntentDetectionResult intentResult)
        {
            // Refactor needs dependency data and architectural context
            context.ScriptDependencies = context.ScriptDependencies; // Keep all
            context.ScriptAnalysis = context.ScriptAnalysis; // Keep all
            context.ArchitecturalSummary = context.ArchitecturalSummary; // Keep for recommendations
            context.ProjectStructure = LimitProjectStructure(context.ProjectStructure, maxDepth: 2);
            context.Console = null; // Not needed
            return context;
        }

        private UnityEditorContext OptimizeForGeneral(UnityEditorContext context, IntentDetectionResult intentResult)
        {
            // General mode - limit everything to reduce token usage
            context.ScriptAnalysis = LimitScriptAnalysis(context.ScriptAnalysis, maxScripts: 10);
            context.ScriptDependencies = LimitScriptDependencies(context.ScriptDependencies, maxScripts: 10);
            context.ProjectStructure = LimitProjectStructure(context.ProjectStructure, maxDepth: 2);
            context.AssetTypes = LimitAssetTypes(context.AssetTypes, maxPerCategory: 2);
            context.ArchitecturalSummary = context.ArchitecturalSummary; // Keep summary
            return context;
        }

        // Helper methods for filtering and limiting data

        private ScriptAnalysisData LimitScriptAnalysis(ScriptAnalysisData analysis, int maxScripts)
        {
            if (analysis == null || analysis.Scripts == null) return analysis;
            
            var limited = new ScriptAnalysisData
            {
                Scripts = analysis.Scripts.Take(maxScripts).ToList()
            };
            return limited;
        }

        private ScriptAnalysisData FilterScriptAnalysisByClasses(ScriptAnalysisData analysis, string[] classNames)
        {
            if (analysis == null || analysis.Scripts == null) return analysis;
            
            var filtered = new ScriptAnalysisData
            {
                Scripts = analysis.Scripts
                    .Where(s => Array.Exists(classNames, c => s.Name.Contains(c) || s.Class.Contains(c)))
                    .ToList()
            };
            return filtered;
        }

        private ScriptDependencyData LimitScriptDependencies(ScriptDependencyData dependencies, int maxScripts)
        {
            if (dependencies == null || dependencies.Dependencies == null) return dependencies;
            
            var limited = new ScriptDependencyData
            {
                Dependencies = dependencies.Dependencies.Take(maxScripts).ToList()
            };
            return limited;
        }

        private ScriptDependencyData FilterScriptDependenciesByClasses(ScriptDependencyData dependencies, string[] classNames)
        {
            if (dependencies == null || dependencies.Dependencies == null) return dependencies;
            
            var filtered = new ScriptDependencyData
            {
                Dependencies = dependencies.Dependencies
                    .Where(d => Array.Exists(classNames, c => d.ScriptName.Contains(c)))
                    .ToList()
            };
            return filtered;
        }

        private ProjectStructureData LimitProjectStructure(ProjectStructureData structure, int maxDepth)
        {
            if (structure == null || structure.Folders == null) return structure;
            
            var limited = new ProjectStructureData
            {
                Folders = structure.Folders.Where(f => f.Depth <= maxDepth).ToList(),
                Files = structure.Files.Where(f => f.Depth <= maxDepth + 1).ToList()
            };
            return limited;
        }

        private AssetTypeData LimitAssetTypes(AssetTypeData assets, int maxPerCategory)
        {
            if (assets == null) return assets;
            
            var limited = new AssetTypeData
            {
                Scripts = assets.Scripts?.Take(maxPerCategory).ToList(),
                Scenes = assets.Scenes?.Take(maxPerCategory).ToList(),
                Prefabs = assets.Prefabs?.Take(maxPerCategory).ToList(),
                Materials = assets.Materials?.Take(maxPerCategory).ToList(),
                Shaders = assets.Shaders?.Take(maxPerCategory).ToList(),
                Fonts = assets.Fonts?.Take(maxPerCategory).ToList(),
                Audio = assets.Audio?.Take(maxPerCategory).ToList(),
                Textures = assets.Textures?.Take(maxPerCategory).ToList(),
                Animations = assets.Animations?.Take(maxPerCategory).ToList(),
                Other = assets.Other?.Take(maxPerCategory).ToList()
            };
            return limited;
        }
    }
}
