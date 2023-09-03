using System.Xml.Serialization;
using System.Collections.Generic;

//The config file created by this mod loader
namespace BepInEx.StationeerModLoader
{
    [XmlRoot(ElementName = "Mod")]
    public class Mod
    {
        [XmlElement(ElementName = "ModName")]
        public string ModName { get; set; }
        [XmlElement(ElementName = "WorkshopId")]
        public ulong WorkshopId { get; set; }
        [XmlElement(ElementName = "IsEnabled")]
        public bool IsEnabled { get; set; }
        [XmlElement(ElementName = "ModPath")]
        public string ModPath { get; set; }
        [XmlAttribute(AttributeName = "LoadWithStationeersMod")]
        public bool LoadWithStationeersMod { get; set; }
        [XmlIgnore]
        public bool IsCore
        {
            get
            {
                return this.WorkshopId == 1UL;
            }
        }
    }

    [XmlRoot(ElementName = "BepinExMods", Namespace = "")]
    public class BepinExMods
    {
        [XmlElement(ElementName = "Mod" , Type = typeof(Mod))]
        public List<Mod> Mod { get; set; }
    }

}


