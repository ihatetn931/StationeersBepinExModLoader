using BepInEx.Logging;
using Mono.Cecil;
using System.Collections.Generic;

namespace BepInEx.StationeerModLoader
{
    public class StationeerModLoader
    {
        public static readonly ManualLogSource Logger = Logging.Logger.CreateLogSource(nameof(StationeerModLoader));
        public const string Version = "1.2.2";
        public const string WebUrl = "https://github.com/ihatetn931/StationeersBepinExModLoader";
        public const string PatcherName = "BepinEx.StationeersModLoader";

        // Add dummy property to fulfil the preloader patcher contract
        public static IEnumerable<string> TargetDLls => new string[0];
        //preloader patcher Initialize
        public static void Initialize()
        {
            if(ConfigFile.AttemptToLoadModConfig())
                ModLoader.Init();
        }
        //preloader patcher Finish
        public static void Finish()
        {
            ChainloaderHandler.Init();
        }

        public static void Patch(AssemblyDefinition ass)
        {
            // Not used, exists so that this works as preloader patcher
        }
    }
}
