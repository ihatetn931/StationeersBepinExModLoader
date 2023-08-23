using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private class ModDirSpec
        {
            public string baseDir;
            public string localDir;
            //need to implant this
            //public HashSet<string> blockedMods, enabledMods;
        }

        public static readonly List<Mod> Mods = new List<Mod>();
        private static readonly List<ModDirSpec> ModDirs = new List<ModDirSpec>();
        public static string test = null;

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
            if (!InitPaths())
                return;
            foreach (var dir in ModDirs)
            {
                LoadFrom(dir);
            }
        }

        private static void LoadFrom(ModDirSpec modDir)
        {
            var modsBaseDirFull = Path.GetFullPath(modDir.baseDir);
            if (!Directory.Exists(modsBaseDirFull))
            {
                StationeerModLoader.Logger.LogWarning("No Steam Work Shop mod folders found!");
                return;
            }
            var localBaseDirFull = Path.GetFullPath(modDir.localDir);
            if (!Directory.Exists(localBaseDirFull))
            {
                StationeerModLoader.Logger.LogWarning("No Local mod folders found!");
                return;
            }

            foreach (var dir in Directory.GetDirectories(modsBaseDirFull))
            {
                var dirName = Path.GetFileName(dir);
                AddMod(dir);
            }
            foreach (var dir in Directory.GetDirectories(localBaseDirFull))
            {
                var dirName = Path.GetFileName(dir);
                AddMod(dir);
            }

            // Also resolve assemblies like bepin does
            AppDomain.CurrentDomain.AssemblyResolve += ResolveModDirectories;
        }

        private static bool InitPaths()
        {
            try
            {
                var spec = new ModDirSpec();

                spec.baseDir = Path.GetFullPath(Environment.ExpandEnvironmentVariables(ConfigFile.workshopPath));
                spec.localDir = Path.GetFullPath(Environment.ExpandEnvironmentVariables(ConfigFile.localModPath));
                ModDirs.Add(spec);
                return true;
            }
            catch (Exception e)
            {
                StationeerModLoader.Logger.LogWarning($"Failed to read {e}");
                return false;
            }
        }

        private static Assembly ResolveModDirectories(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);

            foreach (var mod in Mods)
                if (Utility.TryResolveDllAssembly(name, mod.ModDir, out var ass))
                    return ass;

            return null;
        }

        public static IEnumerable<string> GetPluginDirs()
        {
            return Mods.Select(m => m.PluginsPath).Where(s => s != null);
        }

        public static void UpdateFiles()
        {
            string dir = test;
            string[] filePaths = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            if (!ConfigFile.AlloStationeersMods)
            {
                foreach (var d in filePaths)
                {
                    if (d.Contains("bepinex"))
                    {
                        if (!d.Contains("bepinexrenamed"))
                        {
                            File.Move(d, d + "renamed");
                            StationeerModLoader.Logger.LogInfo("bepinex renamed to bepinexrenamed");
                        }
                    }
                }
            }
            else
            {
                foreach (var d in filePaths)
                {
                    if (d.Contains("bepinexrenamed"))
                    {
                        var replace = d.Replace("renamed", "");
                        File.Move(d, replace);
                        StationeerModLoader.Logger.LogInfo("bepinexrenamed renamed to bepinex");
                    }
                }
            }
        }

        private static void AddMod(string dir)
        {
            // TODO: Maybe add support for MonoModLoader as well?
            test = dir;
            UpdateFiles();
            var pluginsDir = Path.Combine(dir, "");

            var pluginsExists = Directory.Exists(pluginsDir);

            if (!pluginsExists)
                return;

            Mods.Add(new Mod
            {
                PluginsPath = pluginsExists ? pluginsDir : null,
                ModDir = dir
            });
        }
    }
}