using UnityEngine;

namespace KurdostAI.Context
{
    /// <summary>
    /// Aggregates context from all collectors into a unified context structure.
    /// </summary>
    public class ContextBuilder
    {
        private readonly ProjectContextCollector _projectCollector;
        private readonly SelectionContextCollector _selectionCollector;
        private readonly ConsoleContextCollector _consoleCollector;
        private readonly SceneContextCollector _sceneCollector;
        private readonly ProjectStructureCollector _structureCollector;
        private readonly PackagesContextCollector _packagesCollector;
        private readonly ProjectSettingsContextCollector _projectSettingsCollector;
        private readonly HierarchyContextCollector _hierarchyCollector;
        private readonly ScriptAnalyzerCollector _scriptAnalyzerCollector;
        private readonly AssetTypeCollector _assetTypeCollector;
        private readonly ScriptDependencyCollector _scriptDependencyCollector;

        public ContextBuilder()
        {
            _projectCollector = new ProjectContextCollector();
            _selectionCollector = new SelectionContextCollector();
            _consoleCollector = new ConsoleContextCollector();
            _sceneCollector = new SceneContextCollector();
            _structureCollector = new ProjectStructureCollector();
            _packagesCollector = new PackagesContextCollector();
            _projectSettingsCollector = new ProjectSettingsContextCollector();
            _hierarchyCollector = new HierarchyContextCollector();
            _scriptAnalyzerCollector = new ScriptAnalyzerCollector();
            _assetTypeCollector = new AssetTypeCollector();
            _scriptDependencyCollector = new ScriptDependencyCollector();
        }

        /// <summary>
        /// Build complete context for the current Unity Editor state.
        /// </summary>
        /// <param name="includeConsoleErrors">Whether to include console errors (can be expensive)</param>
        /// <param name="includeSelection">Whether to include selection context</param>
        /// <param name="includeProjectStructure">Whether to include project structure (can be expensive)</param>
        /// <returns>Complete context data</returns>
        public UnityEditorContext Build(bool includeConsoleErrors = true, bool includeSelection = true, bool includeProjectStructure = false)
        {
            var context = new UnityEditorContext
            {
                Project = _projectCollector.Collect(),
                Scene = _sceneCollector.Collect(),
                Packages = _packagesCollector.Collect(),
                ProjectSettings = _projectSettingsCollector.Collect(),
                Hierarchy = _hierarchyCollector.Collect()
            };

            if (includeSelection)
            {
                context.Selection = _selectionCollector.Collect();
            }

            if (includeConsoleErrors)
            {
                context.Console = _consoleCollector.Collect();
            }

            if (includeProjectStructure)
            {
                context.ProjectStructure = _structureCollector.Collect();
            }

            return context;
        }

        /// <summary>
        /// Build minimal context (project and scene only).
        /// Use this for general queries where detailed context isn't needed.
        /// </summary>
        public UnityEditorContext BuildMinimal()
        {
            return new UnityEditorContext
            {
                Project = _projectCollector.Collect(),
                Scene = _sceneCollector.Collect(),
                Packages = _packagesCollector.Collect(),
                ProjectSettings = _projectSettingsCollector.Collect()
            };
        }

        /// <summary>
        /// Build context focused on selected objects/assets.
        /// Use this when the user is asking about specific selected items.
        /// </summary>
        public UnityEditorContext BuildSelectionFocused()
        {
            var context = new UnityEditorContext
            {
                Project = _projectCollector.Collect(),
                Scene = _sceneCollector.Collect(),
                Selection = _selectionCollector.Collect(),
                Hierarchy = _hierarchyCollector.Collect()
            };

            return context;
        }

        /// <summary>
        /// Build context focused on console errors.
        /// Use this when the user is asking about errors or debugging.
        /// </summary>
        public UnityEditorContext BuildErrorFocused()
        {
            var context = new UnityEditorContext
            {
                Project = _projectCollector.Collect(),
                Scene = _sceneCollector.Collect(),
                Console = _consoleCollector.Collect(),
                ProjectSettings = _projectSettingsCollector.Collect()
            };

            return context;
        }

        /// <summary>
        /// Build context focused on project structure.
        /// Use this when the user is asking about project files, structure, or wants to create/modify files.
        /// </summary>
        public UnityEditorContext BuildStructureFocused()
        {
            var context = new UnityEditorContext
            {
                Project = _projectCollector.Collect(),
                Scene = _sceneCollector.Collect(),
                ProjectStructure = _structureCollector.Collect(),
                Packages = _packagesCollector.Collect()
            };

            return context;
        }

        /// <summary>
        /// Build complete environment awareness context.
        /// This includes all Unity environment information (Phase 1 + Phase 2).
        /// Script analysis and asset types are always included for consistency.
        /// Packages are excluded to reduce context size.
        /// </summary>
        public UnityEditorContext BuildEnvironmentAwareness()
        {
            var context = new UnityEditorContext
            {
                Project = _projectCollector.Collect(),
                Scene = _sceneCollector.Collect(),
                ProjectSettings = _projectSettingsCollector.Collect(),
                Hierarchy = _hierarchyCollector.Collect(),
                Console = _consoleCollector.Collect(),
                // ProjectStructure = _structureCollector.Collect(), // Removed to reduce context size (too heavy)
                ScriptAnalysis = _scriptAnalyzerCollector.Collect(),
                AssetTypes = _assetTypeCollector.Collect(),
                ScriptDependencies = _scriptDependencyCollector.Collect()
            };

            return context;
        }
    }

    /// <summary>
    /// Complete Unity Editor context structure.
    /// </summary>
    [System.Serializable]
    public class UnityEditorContext
    {
        public ProjectContextData Project;
        public SceneContextData Scene;
        public SelectionContextData Selection;
        public ConsoleContextData Console;
        public ProjectStructureData ProjectStructure;
        public PackagesContextData Packages;
        public ProjectSettingsContextData ProjectSettings;
        public HierarchyContextData Hierarchy;
        public ScriptAnalysisData ScriptAnalysis;
        public AssetTypeData AssetTypes;
        public ScriptDependencyData ScriptDependencies;
    }
}
