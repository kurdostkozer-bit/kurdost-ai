using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace KurdostAI.Context
{
    /// <summary>
    /// Collects project structure (files and folders hierarchy).
    /// This enables AI to understand the complete project layout.
    /// </summary>
    public class ProjectStructureCollector
    {
        private readonly string _projectPath;
        private readonly string _assetsPath;
        private readonly HashSet<string> _excludedFolders;
        private readonly HashSet<string> _excludedExtensions;

        public ProjectStructureCollector()
        {
            _projectPath = Application.dataPath;
            _assetsPath = "";
            
            // Folders to exclude (Unity default and common third-party)
            _excludedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Library",
                "Temp",
                "obj",
                ".git",
                ".vs",
                "Build",
                "Logs",
                "MemoryCaptures",
                "UserSettings",
                "Packages",
                "ProjectSettings"
            };

            // File extensions to exclude
            _excludedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".meta",
                ".dll",
                ".pdb",
                ".exe",
                ".so",
                ".dylib",
                ".a",
                ".lib",
                ".unity3d",
                ".asset",
                ".bytes",
                ".anim",
                ".controller",
                ".overrideController",
                ".physicMaterial",
                ".physicMaterial2D",
                ".mat",
                ".prefab",
                ".unity",
                ".guiskin",
                ".guitexture",
                ".shader",
                ".compute",
                ".rendertexture",
                ".spriteatlas",
                ".terrainlayer",
                ".mixer",
                ".playable",
                ".mask",
                ".brush",
                ".flare",
                ".fontsettings",
                ".cubemap",
                ".lighting",
                ".rendersettings",
                ".fbx",
                ".obj",
                ".blend",
                ".max",
                ".ma",
                ".mb",
                ".psd",
                ".tga",
                ".png",
                ".jpg",
                ".jpeg",
                ".gif",
                ".bmp",
                ".tif",
                ".tiff",
                ".wav",
                ".mp3",
                ".ogg",
                ".aac",
                ".aif",
                ".aiff",
                ".wma",
                ".mp4",
                ".mov",
                ".avi",
                ".webm",
                ".flv",
                ".wmv"
            };
        }

        /// <summary>
        /// Collect complete project structure.
        /// </summary>
        public ProjectStructureData Collect(int maxDepth = 10)
        {
            var data = new ProjectStructureData
            {
                RootPath = _assetsPath,
                Folders = new List<FolderData>(),
                Files = new List<FileData>()
            };

            CollectStructureRecursive(_assetsPath, data, depth: 0, maxDepth: maxDepth);

            return data;
        }

        /// <summary>
        /// Collect structure for a specific path (relative to Assets).
        /// </summary>
        public ProjectStructureData CollectPath(string relativePath)
        {
            var data = new ProjectStructureData
            {
                RootPath = relativePath,
                Folders = new List<FolderData>(),
                Files = new List<FileData>()
            };

            string fullPath = GetFullPath(relativePath);
            if (Directory.Exists(fullPath))
            {
                CollectStructureRecursive(relativePath, data, depth: 0, maxDepth: 10);
            }

            return data;
        }

        /// <summary>
        /// Collect only C# scripts in the project.
        /// </summary>
        public List<FileData> CollectScripts()
        {
            var scripts = new List<FileData>();
            CollectFilesRecursive(_assetsPath, scripts, ".cs", depth: 0, maxDepth: 10);
            return scripts;
        }

        private void CollectStructureRecursive(string relativePath, ProjectStructureData data, int depth, int maxDepth)
        {
            if (depth > maxDepth)
                return;

            string fullPath = GetFullPath(relativePath);

            if (!Directory.Exists(fullPath))
                return;

            // Skip excluded folders (only check for root level folders)
            string folderName = Path.GetFileName(relativePath);
            if (depth == 0 && _excludedFolders.Contains(folderName))
                return;

            // Collect folders
            try
            {
                var directories = Directory.GetDirectories(fullPath);
                foreach (var dir in directories)
                {
                    string dirName = Path.GetFileName(dir);
                    if (!_excludedFolders.Contains(dirName))
                    {
                        string dirRelativePath = Path.Combine(relativePath, dirName).Replace("\\", "/");
                        
                        var folderData = new FolderData
                        {
                            Name = dirName,
                            Path = dirRelativePath,
                            Depth = depth + 1
                        };

                        data.Folders.Add(folderData);

                        // Recursively collect subfolders
                        CollectStructureRecursive(dirRelativePath, data, depth + 1, maxDepth);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ProjectStructureCollector] Error accessing directories: {ex.Message}");
            }

            // Collect files
            try
            {
                var files = Directory.GetFiles(fullPath);
                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string extension = Path.GetExtension(file);

                    if (!_excludedExtensions.Contains(extension))
                    {
                        string fileRelativePath = Path.Combine(relativePath, fileName).Replace("\\", "/");

                        var fileData = new FileData
                        {
                            Name = fileName,
                            Path = fileRelativePath,
                            Extension = extension,
                            Size = new FileInfo(file).Length,
                            Depth = depth + 1
                        };

                        data.Files.Add(fileData);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip files we can't access
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ProjectStructureCollector] Error accessing files: {ex.Message}");
            }
        }

        private void CollectFilesRecursive(string relativePath, List<FileData> files, string extension, int depth, int maxDepth)
        {
            if (depth > maxDepth)
                return;

            string fullPath = GetFullPath(relativePath);

            if (!Directory.Exists(fullPath))
                return;

            // Skip excluded folders
            string folderName = Path.GetFileName(relativePath);
            if (_excludedFolders.Contains(folderName))
                return;

            // Collect files with specific extension
            try
            {
                var allFiles = Directory.GetFiles(fullPath, "*" + extension, SearchOption.TopDirectoryOnly);
                foreach (var file in allFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string fileRelativePath = Path.Combine(relativePath, fileName).Replace("\\", "/");

                    files.Add(new FileData
                    {
                        Name = fileName,
                        Path = fileRelativePath,
                        Extension = extension,
                        Size = new FileInfo(file).Length,
                        Depth = depth
                    });
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip files we can't access
            }

            // Recursively collect from subdirectories
            try
            {
                var directories = Directory.GetDirectories(fullPath);
                foreach (var dir in directories)
                {
                    string dirName = Path.GetFileName(dir);
                    if (!_excludedFolders.Contains(dirName))
                    {
                        string dirRelativePath = Path.Combine(relativePath, dirName).Replace("\\", "/");
                        CollectFilesRecursive(dirRelativePath, files, extension, depth + 1, maxDepth);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }
        }

        private string GetFullPath(string relativePath)
        {
            if (Path.IsPathRooted(relativePath))
            {
                return relativePath;
            }

            return Path.Combine(_projectPath, relativePath.Replace("/", "\\"));
        }
    }

    /// <summary>
    /// Data structure for project structure.
    /// </summary>
    [System.Serializable]
    public class ProjectStructureData
    {
        public string RootPath;
        public System.Collections.Generic.List<FolderData> Folders;
        public System.Collections.Generic.List<FileData> Files;
    }

    [System.Serializable]
    public class FolderData
    {
        public string Name;
        public string Path;
        public int Depth;
    }

    [System.Serializable]
    public class FileData
    {
        public string Name;
        public string Path;
        public string Extension;
        public long Size;
        public int Depth;
    }
}
