using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BepInEx.StationeerModLoader
{
    public static class ModLoader
    {
        public static void Init()
        {
            try
            {
                InitInternal();
            }

            catch (Exception e)
            {
                StationeerModLoader.Logger.LogError($"Failed to index mods, no mods will be loaded: {e}");
            }
        }

        private static void InitInternal()
        {
            if (ConfigFile.modsloaded != null)
            {
                foreach (var dir in ConfigFile.modsloaded)
                {
                    if (!dir.IsCore)
                        LoadFrom(dir);
                }
            }
        }

        public static void LoadFrom(Mod modDir)
        {
            if (modDir.IsEnabled)
            {
                var modsBaseDirFull = Path.GetFullPath(modDir.ModPath);
                if (!Directory.Exists(modsBaseDirFull))
                {
                    StationeerModLoader.Logger.LogWarning($"{modDir.ModName} : {modDir.ModPath} Not Found in modconfig.xml");
                    return;
                }
            }
            // Also resolve assemblies like bepinEx does
            AppDomain.CurrentDomain.AssemblyResolve += ResolveModDirectories;
        }

        private static Assembly ResolveModDirectories(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);

            foreach (var mod in ConfigFile.modsloaded)
                if (Utility.TryResolveDllAssembly(name, mod.ModPath, out var ass))
                    return ass;

            return null;
        }


        //Gets All the plugin folder read from modconfig.xml
        public static IEnumerable<string> GetPluginDirs()
        {
            return ConfigFile.modsloaded.Select(m => m.ModPath).Where(m => m != "");
        }


        //This Update the Files so StationeersMods https://github.com/jixxed/StationeersMods do not load BepinEx mods which is toggable in ModLoaderSettings.xml in the game root
        public static void UpdateFiles(List<ModData> modData, List<Mod> modsLoaded)
        { 
            bool isCore = false;
            bool isEnabled = false;
            foreach (var mdata in modData)
            {
                isCore = mdata.IsCore;
                isEnabled = mdata.IsEnabled;
            }

            foreach (var loaded in modsLoaded)
            {
                if (!isCore && isEnabled)
                {
                    if (!loaded.LoadWithStationeersMod)
                    {
                        if (!File.Exists(loaded.ModPath + "\\About\\NoStationeersModsLoad"))
                        {
                            if (File.Exists(loaded.ModPath + "\\About\\bepinex"))
                            {
                                File.Move(loaded.ModPath + "\\About\\bepinex", loaded.ModPath + "\\About\\NoStationeersModsLoad");
                                StationeerModLoader.Logger.LogInfo($"bepinex renamed to NoStationeersModsLoad for mod {loaded.ModName} ");
                            }
                        }
                    }
                    else
                    {
                        if (!File.Exists(loaded.ModPath + "\\About\\bepinex"))
                        {
                            if (File.Exists(loaded.ModPath + "\\About\\NoStationeersModsLoad"))
                            {
                                File.Move(loaded.ModPath + "\\About\\NoStationeersModsLoad", loaded.ModPath + "\\About\\bepinex");
                                StationeerModLoader.Logger.LogInfo($"NoStationeersModsLoad renamed to bepinex for mod {loaded.ModName} ");
                            }
                        }
                    }
                }
            }
        }
    }
}