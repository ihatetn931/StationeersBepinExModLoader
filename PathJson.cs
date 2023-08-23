using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace BepInEx.StationeerModLoader
{
    [Serializable]
    public class ModInfo
    {
        public bool StationeersModsLoadsBepinExMod { get; set; }
        public ModPaths ModsPath { get; set; }
    }
    [Serializable]
    public class ModPaths
    {
        public string WorkShopPath { get; set; }
        public string LocalModPath { get; set; }
    }

    public class OldModPaths
    {
        public string WorkShopPath { get; set; }
        public string LocalModPath { get; set; }
        public bool StationeersModsLoadsBepinExMod { get; set; }
    }

    public static class ConfigFile
    {
        private static readonly string OldConfig = "../../StationeersModLoader.json";
        private static readonly string PathsConfig = "../../StationeersModLoader.xml";
        private static readonly string WorkShopPath = @"workshop\content\544550";
        private static readonly string LocalModPath = @"My Games\Stationeers\mods";

        public static string PathsConfigPath = Path.Combine(GetAssemblyDirectory, PathsConfig);
        public static string OldConfigPath = Path.Combine(GetAssemblyDirectory, OldConfig);
        public static string workshopPath = Path.Combine(Directory.GetParent(GetAssemblyDirectory).Parent.Parent.Parent.ToString(), WorkShopPath);
        public static string localModPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), LocalModPath);
        public static bool AlloStationeersMods = false;

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

        public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                writer = new StreamWriter(filePath, append);
                serializer.Serialize(writer, objectToWrite);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public static T ReadFromXmlFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                reader = new StreamReader(filePath);
                return (T)serializer.Deserialize(reader);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public static bool AttemptToLoad()
        {

            if (File.Exists(PathsConfigPath))
            {
                var xml = ReadFromXmlFile<ModInfo>(PathsConfigPath);
                workshopPath = xml.ModsPath.WorkShopPath;
                localModPath = xml.ModsPath.LocalModPath;
                AlloStationeersMods = xml.StationeersModsLoadsBepinExMod;
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
                ModPaths mods = new ModPaths
                {
                    WorkShopPath = workshopPath,
                    LocalModPath = localModPath,
                };
                ModInfo modsInfo = new ModInfo
                {
                    ModsPath = mods,
                    StationeersModsLoadsBepinExMod = false

                };
                WriteToXmlFile<ModInfo>(PathsConfigPath, modsInfo, true);
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
                ModPaths mods = new ModPaths
                {
                    WorkShopPath = workshopPath,
                    LocalModPath = localModPath,
                };
                ModInfo modsInfo = new ModInfo
                {
                    ModsPath = mods,
                    StationeersModsLoadsBepinExMod = AlloStationeersMods

                };
                WriteToXmlFile(PathsConfigPath, modsInfo, true);
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