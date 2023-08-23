using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace BepInEx.StationeerModLoader
{
    [Serializable]
    public class ModPaths
    {
        public string WorkShopPath { get; set; }
        public string LocalModPath { get; set; }
        public bool StationeersModsLoadsBepinExMod {get; set;}
    }

    public static class ConfigFile
    {
        private static readonly string PathsConfig = "../../StationeersModLoader.json";
        private static readonly string WorkShopPath = @"workshop\content\544550";
        private static readonly string LocalModPath = @"My Games\Stationeers\mods";

    private static string GetAssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        public static string PathsConfigPath = Path.Combine(GetAssemblyDirectory, PathsConfig);

        public static string workshopPath = Path.Combine(Directory.GetParent(GetAssemblyDirectory).Parent.Parent.Parent.ToString(), WorkShopPath);
        public static string localModPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), LocalModPath);
        public static bool AlloStationeersMods = false;

        public static bool AttemptToLoad()
        {

            if (File.Exists(PathsConfigPath))
            {
                string settingsJson = File.ReadAllText(PathsConfigPath);
                //StationeerModLoader.Logger.LogError("settingsJson: " + settingsJson);
                ModPaths settingFromFile = JsonConvert.DeserializeObject<ModPaths>(settingsJson);
                StationeerModLoader.paths = settingFromFile;
                AlloStationeersMods = settingFromFile.StationeersModsLoadsBepinExMod;
                workshopPath = settingFromFile.WorkShopPath;
                localModPath = settingFromFile.LocalModPath;
            }
            else
            {
                AttemptToCreate();
            }
            return true;
        }

        public static bool AttemptToCreate()
        {

            if (!File.Exists(PathsConfigPath))
            {
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.Formatting = Formatting.Indented;
                var mods = new ModPaths
                {
                    WorkShopPath = workshopPath,
                    LocalModPath = localModPath,
                    StationeersModsLoadsBepinExMod = false
                };
                string json = JsonConvert.SerializeObject(mods,serializerSettings);
                File.WriteAllText(PathsConfigPath, json);
                AttemptToLoad();


            }
            else
            {
                AttemptToLoad();
            }
            return true;
        }

        public static bool AttemptToSave()
        {
            if (File.Exists(PathsConfigPath))
            {
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.Formatting = Formatting.Indented;
                ModPaths mod = new ModPaths
                {
                    WorkShopPath = workshopPath,
                    LocalModPath = localModPath,
                    StationeersModsLoadsBepinExMod = AlloStationeersMods
                };
                string toJson = JsonConvert.SerializeObject(mod,serializerSettings);
                File.WriteAllText(PathsConfigPath, toJson);
                AttemptToLoad();
            }
            else
            {
                StationeerModLoader.Logger.LogError("[BepinEx Stationeers ModLoader] Cannot Find StationeersModPaths.json in the game root");
            }
            return true;
        }
    }
}