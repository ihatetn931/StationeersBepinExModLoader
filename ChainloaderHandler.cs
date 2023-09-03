using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
using Mono.Cecil;

namespace BepInEx.StationeerModLoader
{
    public static class ChainloaderHandler
    {
        private static bool shouldSaveCache = true;

        public static void Init()
        {
            TypeLoader.AssemblyResolve += TypeLoaderOnAssemblyResolve;
            var instance = new Harmony("org.bepinex.loaderprototype.chainloaderhandler");
            instance.Patch(
                AccessTools.Method(typeof(TypeLoader), nameof(TypeLoader.FindPluginTypes))
                    .MakeGenericMethod(typeof(PluginInfo)),
                new HarmonyMethod(AccessTools.Method(typeof(ChainloaderHandler), nameof(PreFindPluginTypes))),
                new HarmonyMethod(AccessTools.Method(typeof(ChainloaderHandler), nameof(PostFindPluginTypes))));

            instance.Patch(
                AccessTools.Method(typeof(TypeLoader), nameof(TypeLoader.SaveAssemblyCache))
                    .MakeGenericMethod(typeof(PluginInfo)),
                new HarmonyMethod(AccessTools.Method(typeof(ChainloaderHandler), nameof(OnSaveAssemblyCache))));
        }

        private static AssemblyDefinition TypeLoaderOnAssemblyResolve(object sender, AssemblyNameReference reference)
        {
            var name = new AssemblyName(reference.FullName);
            foreach (var pluginDir in ModLoader.GetPluginDirs())
                if (Utility.TryResolveDllAssembly(name, pluginDir, TypeLoader.ReaderParameters, out var assembly))
                    return assembly;
            return null;
        }

        private static void PreFindPluginTypes(string directory)
        {
            if (directory != Paths.PluginPath)
                return;
            // prevent saving cache in order to not overwrite it when loading all the mods 
            shouldSaveCache = false;
        }

        private static void PostFindPluginTypes(Dictionary<string, List<PluginInfo>> __result, string directory,
            Func<TypeDefinition, PluginInfo> typeSelector, Func<AssemblyDefinition, bool> assemblyFilter,
            string cacheName)
        {
            // Prevent recursion
            string path = null;
            if (directory != Paths.PluginPath)
                return;
            if (ConfigFile.modsloaded != null)
            {
                foreach (var mod in ConfigFile.moddata)
                {
                    if (!mod.IsCore)
                    {
                        if (!mod.IsEnabled)
                        {
                            path = mod.LocalPath;
                            StationeerModLoader.Logger.LogInfo($"Disabled Mod {ConfigFile.GetModAbout(mod.LocalPath,mod.AboutXmlPath).Name}");
                        }
                    }
                }
            }
            StationeerModLoader.Logger.LogInfo("Finding plugins from mods...");
            foreach (var pluginDir in ModLoader.GetPluginDirs())
            {
                //checks if any mods are set to load with StationeersMods and removes them from the BepinEx ChainLoader and allows StationeersMods to load them
                if (!File.Exists(pluginDir + "\\About\\bepinex"))
                {
                    if (pluginDir == path)
                        return;
                    var result = TypeLoader.FindPluginTypes(pluginDir, typeSelector, assemblyFilter, cacheName);
                    foreach (var kv in result)
                        __result[kv.Key] = kv.Value;
                }
            }
            shouldSaveCache = true;
            if (cacheName != null)
                TypeLoader.SaveAssemblyCache(cacheName, __result);
        }

        private static bool OnSaveAssemblyCache()
        {
            return shouldSaveCache;
        }
    }
}