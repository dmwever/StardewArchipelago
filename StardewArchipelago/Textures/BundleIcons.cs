﻿using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace StardewArchipelago.Textures
{
    public static class BundleIcons
    {
        public const string BUNDLE_SUFFIX = "bundle";

        public static Texture2D GetBundleIcon(IMonitor monitor, IModHelper modHelper, string bundleName, LogLevel failureLogLevel = LogLevel.Error)
        {
            var bundlesFolder = "Bundles";
            var cleanName = bundleName.Replace("'", "").Replace(" ", "_").ToLower();
            var fileNameBundleName = $"{cleanName}_{BUNDLE_SUFFIX}.png";
            var pathToTexture = Path.Combine(bundlesFolder, fileNameBundleName);
            monitor.Log($"Attempting to load bundle icon '{pathToTexture}'", LogLevel.Trace);
            return TexturesLoader.GetTexture(monitor, modHelper, pathToTexture, failureLogLevel);
        }
    }
}
