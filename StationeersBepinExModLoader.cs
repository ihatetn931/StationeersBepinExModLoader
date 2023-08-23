using BepInEx.Logging;
using Mono.Cecil;
using System.Collections.Generic;

namespace BepInEx.StationeerModLoader
{
    public class StationeerModLoader
    {

        public static readonly ManualLogSource Logger = Logging.Logger.CreateLogSource(nameof(StationeerModLoader));
        public static OldModPaths paths;
        public const string Version = "1.0.1";
        public const string WebUrl = "https://github.com/ihatetn931/StationeersBepinExModLoader";
        public const string PatcherName = "BepinEx.StationeersModLoader";

        // Add dummy property to fulfil the preloader patcher contract
        public static IEnumerable<string> TargetDLls => new string[0];

        public static void Initialize()
        {
            ModLoader.Init();
        }

        public static void Finish()
        {
            // Hook chainloader only after preloader to not cause resolving on UnityEngine too soon
            ChainloaderHandler.Init();
        }

        public static void Patch(AssemblyDefinition ass)
        {
            // Not used, exists so that this works as preloader patch
        }
    }
}
