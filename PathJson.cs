using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
namespace BepInEx.StationeerModLoader
{
    public class StationeersBepinExModLoader
    {
        public bool StationeersModsLoadsBepinExMod { get; set; }
    }
    //Created my Own ModConfig Class to get modconfig.xml data
    public class ModConfig
    {
        public List<ModData> Mods { get; set; } = new List<ModData>();
    }
    //Created my Own ModData Class to get modconfig.xml data
    public class ModData
    {
        public ulong Id { get; set; }
        public bool IsEnabled { get; set; }
        public string LocalPath { get; set; }
        public bool IsLocal
        {
            get
            {
                return this.Id == 0UL;
            }
        }
        public bool IsCore
        {
            get
            {
                return this.Id == 1UL;
            }
        }
        public bool IsWorkshop
        {
            get
            {
                return this.Id > 1UL;
            }
        }
        public string AboutXmlPath
        {
            get
            {
                return this.LocalPath + "\\About\\About.xml";
            }
        }
    }

    public class ConfigFile
    {
       //The ModConfig file where it reads all the mods folders
        private static readonly string ModPathConfig = "../../modconfig.xml";
        //ModLoader Settings that user can set by eding this xml file
        private static readonly string PathsConfig = "../../StationeersModLoader.xml";
        //Path Combines to get the folder the dll in then going to the game root
        public static string PathsConfigPath = Path.Combine(GetAssemblyDirectory, PathsConfig);
        public static string ModConfigPath = Path.Combine(GetAssemblyDirectory, ModPathConfig);

        //If true StationeersMods https://github.com/jixxed/StationeersMods will load BepinEx Mods Set in StationeersModLoader.xml
        public static  bool AllowStationeersMods = false;

        public static string GetAssemblyDirectory
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
            //Load modconfig.xml and sets it so modloader can get data from it
            if (File.Exists(ModConfigPath))
            { 
                ModConfig modconfig = ReadFromXmlFile<ModConfig>(ModConfigPath);
                ModLoader.mConfig = modconfig;
            }
            //Load StationeersModLoader.xml to get ModLoader Settings
            if (File.Exists(PathsConfigPath))
            {
                StationeersBepinExModLoader modLoaderConfig = ReadFromXmlFile<StationeersBepinExModLoader>(PathsConfigPath);
                AllowStationeersMods = modLoaderConfig.StationeersModsLoadsBepinExMod;
            }
            //if StationeersModLoader.xml does not exists this create the file (first time load)
            if (!File.Exists(PathsConfigPath))
                AttemptToCreate();

            return true;
        }

        //Creates StationeersModLoader.xml and set the default value to false
        public static bool AttemptToCreate()
        {
            StationeersBepinExModLoader modsInfo = new StationeersBepinExModLoader
            {
                StationeersModsLoadsBepinExMod = false

            };
            WriteToXmlFile<StationeersBepinExModLoader>(PathsConfigPath, modsInfo, true);
            AttemptToLoad();
            return true;
        }

        //Saves StationeersModLoader.xml but not used will be used in a later update
        public bool AttemptToSave()
        {
            if (File.Exists(PathsConfigPath))
            {
                StationeersBepinExModLoader modsInfo = new StationeersBepinExModLoader
                {
                    StationeersModsLoadsBepinExMod = AllowStationeersMods

                };
                WriteToXmlFile(PathsConfigPath, modsInfo, true);
                AttemptToLoad();
            }
            else
            {
                StationeerModLoader.Logger.LogError("[BepinEx Stationeers ModLoader] Cannot Find StationeersModPaths.xml in the game root Creating..");
                AttemptToCreate();
            }
            return true;
        }
    }
}