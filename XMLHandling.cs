using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace BepInEx.StationeerModLoader
{
    public class ConfigFile
    {
        private static readonly string ModPathConfig = "../../modconfig.xml";
        private static readonly string PathsConfig = "../../ModLoaderSettings.xml";
        public static string PathsConfigPath = Path.Combine(ConfigFile.GetAssemblyDirectory, ConfigFile.PathsConfig);
        public static string ModConfigPath = Path.Combine(ConfigFile.GetAssemblyDirectory, ConfigFile.ModPathConfig);
        public static List<ModData> moddata = new List<ModData>();
        public static List<Mod> modsloaded = new List<Mod>();

        public static string GetAssemblyDirectory
        {
            get
            {
                return Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
            }
        }

        public static void WriteToXmlFile<T>(string filePath, T objectToWrite, XmlSerializerNamespaces nameSpace, bool append = false) where T : new()
        {
            TextWriter textWriter = null;
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                textWriter = new StreamWriter(filePath, append);
                xmlSerializer.Serialize(textWriter, objectToWrite, nameSpace);
            }
            finally
            {
                if (textWriter != null)
                {
                    textWriter.Close();
                }
            }
        }


        public static T ReadFromXmlFile<T>(string filePath) where T : new()
        {
            TextReader textReader = null;
            T result;
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                textReader = new StreamReader(filePath);
                result = (T)((object)xmlSerializer.Deserialize(textReader));
            }
            finally
            {
                if (textReader != null)
                {
                    textReader.Close();
                }
            }
            return result;
        }

        public static bool AttemptToLoadModConfig()
        {
            if (File.Exists(ConfigFile.ModConfigPath))
            {
                moddata.Clear();
                //var modConfig = ConfigFile.ReadFromXmlFile<ModConfig>(ModConfigPath);
                var modConfig = RemoveNonBepinExMods(XmlSerialization.Deserialize<ModConfig>(ModConfigPath, "ModConfig"));
                foreach (var t in modConfig.Mods)
                    if (!t.IsCore)
                        moddata.Add(t);
            }
            if (File.Exists(ConfigFile.PathsConfigPath))
            {
                ConfigFile.AttemptToLoadSettings();
            }
            else
            {
                ConfigFile.AttemptToCreate();
            }

            return true;
        }

        public static bool CheckForRemovedMods(BepinExMods modData)
        {
            foreach (var loaded in modData.Mod.ToList())
            {
                if (!File.Exists(Path.Combine(loaded.ModPath, loaded.ModPath + "\\About\\About.xml")))
                {
                    modsloaded.Remove(loaded);
                    StationeerModLoader.Logger.LogInfo($"Removed Mod {loaded.ModName} from ModLoaderSettings.xml");
                }

            }
            return true;
        }


        public static bool AttemptToLoadSettings()
        {
            if (File.Exists(ConfigFile.PathsConfigPath))
            {
                modsloaded.Clear();
                //var modData = XmlSerialization.Deserialize<BepinExMods>(PathsConfigPath, "BepinExMods");
                BepinExMods modData = ConfigFile.ReadFromXmlFile<BepinExMods>(PathsConfigPath);
                if (CheckForRemovedMods(modData))
                {
                    foreach (var data in moddata)
                    {
                        var match = modData.Mod.FirstOrDefault(stringToCheck => stringToCheck.ModPath.Contains(data.LocalPath));
                        if (match != null)
                        {
                            Mod modadded = new Mod
                            {
                                ModName = match.ModName,
                                ModPath = match.ModPath,
                                LoadWithStationeersMod = match.LoadWithStationeersMod,
                                IsEnabled = data.IsEnabled,
                                WorkshopId = GetModAbout(data.LocalPath, data.AboutXmlPath).WorkshopHandle
                            };

                            modsloaded.Add(modadded);

                            BepinExMods bepinmod = new BepinExMods
                            {
                                Mod = modsloaded
                            };
                            if(!modsloaded.Contains(modadded))
                                StationeerModLoader.Logger.LogInfo($"Added Mod {modadded.ModName} from ModLoaderSettings.xml");
                            ModLoader.UpdateFiles(moddata, modsloaded);
                            //removing any xml namespaces that gets added cause for some people it may confuse them
                            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                            ns.Add("", "");
                            WriteToXmlFile<BepinExMods>(PathsConfigPath, bepinmod, ns, false);
                        }
                        else
                        {
                            Mod mod = new Mod
                            {
                                ModName = GetModAbout(data.LocalPath, data.AboutXmlPath).Name,
                                LoadWithStationeersMod = false,
                                WorkshopId = GetModAbout(data.LocalPath, data.AboutXmlPath).WorkshopHandle,
                                IsEnabled = data.IsEnabled,
                                ModPath = data.LocalPath
                            };
                            StationeerModLoader.Logger.LogInfo($"Added Mod {mod.ModName} modconfig.xml");
                            modsloaded.Add(mod);
                            BepinExMods bepinmod = new BepinExMods
                            {
                                Mod = modsloaded
                            };
                            //removing any xml namespaces that gets added cause for some people it may confuse them
                            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                            ns.Add("", "");
                            WriteToXmlFile<BepinExMods>(PathsConfigPath, bepinmod, ns, false);
                        }
                    }
                }
            }
            return true;
        }

        //Removes all mods from the list if they're not BepinExMods
        public static ModConfig RemoveNonBepinExMods(ModConfig mconfig)
        {
            if (mconfig != null)
            {
                foreach (ModData modData in mconfig.Mods.ToList<ModData>())
                {
                    if (modData != null)
                    {
                        if (!modData.IsCore)
                        {
                            var checkfordll = Directory.GetFiles(modData.LocalPath, "*.dll",SearchOption.AllDirectories).Length;
                            if (checkfordll == 0)
                                mconfig.Mods.Remove(modData);
                            if (File.Exists(modData.LocalPath + "\\About\\stationeersmods"))
                                mconfig.Mods.Remove(modData);
                        }
                    }
                }
            }
            return mconfig;
        }

        //Read The Mod About.xml
        public static ModAbout GetModAbout(string modpath, string aboutpath)
        {
            ModAbout modAbout = ConfigFile.ReadFromXmlFile<ModAbout>(Path.Combine(modpath, aboutpath));
            return modAbout;
        }

        //Create The ConfigFile
        public static bool AttemptToCreate()
        {
            if (moddata != null)
            {
                //removing any xml namespaces that gets added cause for some people it may confuse them
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                foreach (var data in moddata)
                {
                    Mod mod = new Mod
                    {
                        ModName = GetModAbout(data.LocalPath, data.AboutXmlPath).Name,
                        LoadWithStationeersMod = false,
                        WorkshopId = GetModAbout(data.LocalPath, data.AboutXmlPath).WorkshopHandle,
                        IsEnabled = data.IsEnabled,
                        ModPath = data.LocalPath
                    };
                    modsloaded.Add(mod);
                    BepinExMods bepinmod = new BepinExMods
                    {
                        Mod = modsloaded
                    };
                    WriteToXmlFile<BepinExMods>(PathsConfigPath, bepinmod, ns, false);
                }
            }
            return true;
        }
    }
}


