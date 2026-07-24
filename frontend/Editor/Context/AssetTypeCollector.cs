using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace KurdostAI.Context
{
    /// <summary>
    /// Categorizes project assets by type (Scripts, Scenes, Prefabs, Materials, Shaders, Fonts, Audio, etc.).
    /// This prevents AI from confusing different asset types when analyzing the project.
    /// </summary>
    public class AssetTypeCollector
    {
        private readonly string _projectPath;

        public AssetTypeCollector()
        {
            _projectPath = Application.dataPath;
        }

        /// <summary>
        /// Collect and categorize all assets in the project.
        /// </summary>
        public AssetTypeData Collect(int maxFilesPerCategory = 2)
        {
            var data = new AssetTypeData
            {
                Scripts = new List<AssetInfo>(),
                Scenes = new List<AssetInfo>(),
                Prefabs = new List<AssetInfo>(),
                Materials = new List<AssetInfo>(),
                Shaders = new List<AssetInfo>(),
                Fonts = new List<AssetInfo>(),
                Audio = new List<AssetInfo>(),
                Textures = new List<AssetInfo>(),
                Animations = new List<AssetInfo>(),
                Other = new List<AssetInfo>()
            };

            // Get all files in the project
            var allFiles = Directory.GetFiles(_projectPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in allFiles)
            {
                try
                {
                    var relativePath = file.Substring(_projectPath.Length + 1).Replace("\\", "/");
                    var extension = Path.GetExtension(file).ToLower();
                    var fileInfo = new FileInfo(file);
                    var assetInfo = new AssetInfo
                    {
                        Name = fileInfo.Name,
                        Path = relativePath,
                        Extension = extension,
                        Size = fileInfo.Length
                    };

                    // Categorize by extension with limits
                    switch (extension)
                    {
                        case ".cs":
                            if (data.Scripts.Count < maxFilesPerCategory) data.Scripts.Add(assetInfo);
                            break;
                        case ".unity":
                            if (data.Scenes.Count < maxFilesPerCategory) data.Scenes.Add(assetInfo);
                            break;
                        case ".prefab":
                            if (data.Prefabs.Count < maxFilesPerCategory) data.Prefabs.Add(assetInfo);
                            break;
                        case ".mat":
                            if (data.Materials.Count < maxFilesPerCategory) data.Materials.Add(assetInfo);
                            break;
                        case ".shader":
                        case ".shadergraph":
                        case ".cginc":
                        case ".hlsl":
                            if (data.Shaders.Count < maxFilesPerCategory) data.Shaders.Add(assetInfo);
                            break;
                        case ".ttf":
                        case ".otf":
                        case ".woff":
                        case ".woff2":
                            if (data.Fonts.Count < maxFilesPerCategory) data.Fonts.Add(assetInfo);
                            break;
                        case ".mp3":
                        case ".wav":
                        case ".ogg":
                        case ".aif":
                        case ".aiff":
                            if (data.Audio.Count < maxFilesPerCategory) data.Audio.Add(assetInfo);
                            break;
                        case ".png":
                        case ".jpg":
                        case ".jpeg":
                        case ".tga":
                        case ".psd":
                            if (data.Textures.Count < maxFilesPerCategory) data.Textures.Add(assetInfo);
                            break;
                        case ".anim":
                        case ".controller":
                            if (data.Animations.Count < maxFilesPerCategory) data.Animations.Add(assetInfo);
                            break;
                        default:
                            // Skip meta files and other Unity internal files
                            if (!extension.Equals(".meta", StringComparison.OrdinalIgnoreCase) &&
                                !extension.Equals(".asset", StringComparison.OrdinalIgnoreCase) &&
                                data.Other.Count < maxFilesPerCategory)
                            {
                                data.Other.Add(assetInfo);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AssetTypeCollector] Failed to categorize {file}: {ex.Message}");
                }
            }

            return data;
        }
    }

    /// <summary>
    /// Container for asset type data.
    /// </summary>
    [System.Serializable]
    public class AssetTypeData
    {
        public List<AssetInfo> Scripts;
        public List<AssetInfo> Scenes;
        public List<AssetInfo> Prefabs;
        public List<AssetInfo> Materials;
        public List<AssetInfo> Shaders;
        public List<AssetInfo> Fonts;
        public List<AssetInfo> Audio;
        public List<AssetInfo> Textures;
        public List<AssetInfo> Animations;
        public List<AssetInfo> Other;
    }

    /// <summary>
    /// Information about a single asset.
    /// </summary>
    [System.Serializable]
    public class AssetInfo
    {
        public string Name;
        public string Path;
        public string Extension;
        public long Size;
    }
}
