
using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BepInEx.StationeerModLoader
{
    public class Mod
    {
        public string PluginsPath { get; set; }
        public string ModDir { get; set; }
    }

    public static class ModLoader
    {
        public static readonly List<Mod> Mods = new List<Mod>();
        public static string directory = null;
        public static ModConfig mConfig = null;

        public static void Init()
        {
            try
            {
                if (ConfigFile.AttemptToLoad())
                {
                    if (ConfigFile.AllowStationeersMods)
                        StationeerModLoader.Logger.LogInfo($"Set to Allow StationeersMod to load BepinEx Mods (BepinEx Mods Will Load Twice)");
                    else
                        StationeerModLoader.Logger.LogInfo($"Set to not Allow StationeersMod to load BepinEx Mods");
                    InitInternal();
                    UpdateFiles();
                }
            }

            catch (Exception e)
            {
                StationeerModLoader.Logger.LogError($"Failed to index mods, no mods will be loaded: {e}");
            }
        }

        private static void InitInternal()
        {
            if (mConfig != null)
            {
                foreach (var dir in mConfig.Mods)
                {
                    if (!dir.IsCore)
                        LoadFrom(dir);
                }
            }
        }

        public static void LoadFrom(ModData modDir)
        {
            if (modDir.IsEnabled)
            {
                var modsBaseDirFull = Path.GetFullPath(modDir.LocalPath);
                if (!Directory.Exists(modsBaseDirFull))
                {
                    StationeerModLoader.Logger.LogWarning("No Mod Folders Found in modconfig.xml");
                    return;
                }
                directory = modsBaseDirFull;
            }

            // Also resolve assemblies like bepin does
            AppDomain.CurrentDomain.AssemblyResolve += ResolveModDirectories;
        }

        private static Assembly ResolveModDirectories(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);

            foreach (var mod in mConfig.Mods)
                    if (Utility.TryResolveDllAssembly(name, mod.LocalPath, out var ass))
                        return ass;

            return null;
        }

        public static IEnumerable<string> GetPluginDirs()
        {
            return mConfig.Mods.Select(m => m.LocalPath).Where(m => m != "");
        }

        //This Update the Files so StationeersMods https://github.com/jixxed/StationeersMods do not load BepinEx mods which is toggable in StationeersModLoader.xml in the game root
        public static void UpdateFiles()
        {
            string dir = directory;
            string[] filePaths = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            if (!ConfigFile.AllowStationeersMods)
            {
                foreach (var d in filePaths)
                {
                    if (d.Contains("bepinex"))
                    {
                        if (!d.Contains("bepinexStationeersModsNoLoad"))
                        {
                            File.Move(d, d + "StationeersModsNoLoad");
                            StationeerModLoader.Logger.LogInfo($"bepinex renamed to bepinexStationeersModsNoLoad for mod {dir}");
                        }
                    }
                }
            }
            else
            {
                foreach (var d in filePaths)
                {
                    if (d.Contains("bepinexStationeersModsNoLoad"))
                    {
                        var replace = d.Replace("StationeersModsNoLoad", "");
                        File.Move(d, replace);
                        StationeerModLoader.Logger.LogInfo($"bepinexStationeersModsNoLoad renamed to bepinex for mod {dir}");
                    }
                }
            }
        }
    }
}