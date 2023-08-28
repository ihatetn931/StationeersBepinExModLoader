using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BepInEx.StationeerModLoader
{
    public class Mod
    {
        public string PluginsPath { get; set; }
        public string ModDir { get; set; }
    }

    public static class ModLoader
    {
        public static ModConfig mConfig = null;

        public static void Init()
        {
            try
            {
                if (ConfigFile.AttemptToLoad())
                {
                    InitInternal();
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

        //Gets All the plugin folder read from modconfig.xml
        public static IEnumerable<string> GetPluginDirs()
        {
            return mConfig.Mods.Select(m => m.LocalPath).Where(m => m != "");
        }


        //This Update the Files so StationeersMods https://github.com/jixxed/StationeersMods do not load BepinEx mods which is toggable in ModLoaderSettings.xml in the game root
        public static void UpdateFiles(List<ModData> modData, List<ModsLoaded> modsLoaded)
        {
            bool isCore = false;
            foreach (var mdata in modData)
            {
                isCore = mdata.IsCore;
            }
            foreach (var loaded in modsLoaded)
            {
                if (!isCore)
                {
                    if (!loaded.LoadWithStationeersMod)
                    {
                        if (!File.Exists(loaded.ModPath + "\\About\\bepinexNoStationeersModsLoad"))
                        {
                            if (File.Exists(loaded.ModPath + "\\About\\bepinex"))
                            {
                                File.Move(loaded.ModPath + "\\About\\bepinex", loaded.ModPath + "\\About\\bepinexNoStationeersModsLoad");
                                StationeerModLoader.Logger.LogInfo($"bepinex renamed to bepinexNoStationeersModsLoad for mod {loaded.ModName} ");
                            }
                        }
                    }
                    else
                    {
                        if (!File.Exists(loaded.ModPath + "\\About\\bepinex"))
                        {
                            if (File.Exists(loaded.ModPath + "\\About\\bepinexNoStationeersModsLoad"))
                            {
                                File.Move(loaded.ModPath + "\\About\\bepinexNoStationeersModsLoad", loaded.ModPath + "\\About\\bepinex");
                                StationeerModLoader.Logger.LogInfo($"bepinexNoStationeersModsLoad renamed to bepinex for mod {loaded.ModName} ");
                            }
                        }
                    }
                }
            }
        }
    }
}