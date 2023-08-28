using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace BepInEx.StationeerModLoader
{
    /// <summary>
    /// 
    /// </summary>
    public class StationeerModLoaderSettings
    {
        //public bool StationeersModsLoadsBepinExMod { get; set; }
        public List<ModsLoaded> LoadedMods { get; set; } = new List<ModsLoaded>();
    }
    public class ModsLoaded
    {
        public string ModName { get; set; }
        public bool LoadWithStationeersMod { get; set; }
        public string ModPath { get; set; }
    }
    //Created my Own ModConfig Class to get modconfig.xml data
    public class ModConfig
    {
        public List<ModData> Mods { get; set; } = new List<ModData>();
    }

    [XmlRoot("ModMetadata")]
    public class ModAbout
    {

        public const string ROOT_NAME = "ModMetadata";

        [XmlElement]
        public string Name;

        [XmlElement]
        public string Author;

        [XmlElement]
        public string Version;

        [XmlElement]
        public string Description;

        [XmlElement]
        public ulong WorkshopHandle;

        [XmlArray("Tags")]
        [XmlArrayItem("Tag")]
        public List<string> Tags;

        [XmlIgnore]
        public bool IsValid = true;
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
        //ModLoader Settings File use this toggle if mods load with StationersMod or this ModLoader
        private static readonly string PathsConfig = "../../ModLoaderSettings.xml";
        //Path combines for modconfig.xml and ModLoaderSettings.xml
        public static string PathsConfigPath = Path.Combine(GetAssemblyDirectory, PathsConfig);
        public static string ModConfigPath = Path.Combine(GetAssemblyDirectory, ModPathConfig);

        //Mods Loaded with bepinex files
        public static List<ModsLoaded> mloaded = new List<ModsLoaded>();

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

        //For writing ModLoaderSettings.xml on first load
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

        //For reading ModLoaderSettings.xml and modconfig.xml to get the current mod info
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

        //Loads all the xml files so they can be used
        public static bool AttemptToLoad()
        {
            ModConfig modconfig = null;
            //Load modconfig.xml and sets it so modloader can get data from it
            if (File.Exists(ModConfigPath))
            {
                //ModLoader.mConfig.Mods.Clear();
                modconfig = ReadFromXmlFile<ModConfig>(ModConfigPath);
                ModLoader.mConfig = modconfig;
            }
            //Load ModLoaderSettings.xml to get ModLoader Settings
            if (File.Exists(PathsConfigPath))
            {
                StationeerModLoaderSettings modLoaderConfig = ReadFromXmlFile<StationeerModLoaderSettings>(PathsConfigPath);
                mloaded = modLoaderConfig.LoadedMods;
                if (ModLoader.mConfig != null && modLoaderConfig.LoadedMods != null)
                {
                   ModLoader.UpdateFiles(modconfig.Mods,modLoaderConfig.LoadedMods);
                }
            }
            //if ModLoaderSettings.xml does not exists this create the file (first time load)
            if (!File.Exists(PathsConfigPath))
            {
                AttemptToCreate();
            }

            return true;
        }

        //Creates ModLoaderSettings.xml and sets the default value to false
        public static bool AttemptToCreate()
        {
            if (ModLoader.mConfig != null)
            {
                foreach (var config in ModLoader.mConfig.Mods)
                {
                    if (!config.IsCore)
                    {
                        var aboutfile = Path.Combine(config.LocalPath, config.AboutXmlPath);
                        var test = ConfigFile.ReadFromXmlFile<ModAbout>(aboutfile);
                        string[] filePaths = Directory.GetFiles(config.LocalPath, "*", SearchOption.AllDirectories);
                        foreach (var fp in filePaths)
                        {
                            if (fp.Contains("bepinexStationeersModsNoLoad") || fp.Contains("bepinex"))
                            {
                                ///Mods Loaded with BepinEx files
                                ModsLoaded modsInfo = new ModsLoaded
                                {
                                    LoadWithStationeersMod = false,
                                    ModName = test.Name,
                                    ModPath = config.LocalPath
                                };
                                mloaded.Add(modsInfo);
                                StationeerModLoaderSettings mloader = new StationeerModLoaderSettings
                                {
                                    LoadedMods = mloaded
                                };
                                WriteToXmlFile<StationeerModLoaderSettings>(PathsConfigPath, mloader, false);
                            }
                        }
                    }
                }
            }
            AttemptToLoad();
            return true;
        }

        //Saves StationeersModLoader.xml but not used will be used in a later update
        /* public static bool AttemptToSave( List<ModsLoaded> modsLoaded)
         {
             if (File.Exists(PathsConfigPath))
             {
                 StationeerModLoaderSettings modsInfo = new StationeerModLoaderSettings
                 {
                     LoadedMods = modsLoaded
                 };
                 mloaded = modsLoaded;
                 WriteToXmlFile<StationeerModLoaderSettings>(PathsConfigPath, modsInfo, false);
                 AttemptToLoad();
             }
             else
             {
                 StationeerModLoader.Logger.LogError("[BepinEx Stationeers ModLoader] Cannot Find ModLoaderSettings.xml in the game root Creating..");
                 AttemptToCreate();
             }
             return true;
         }*/
    }
}