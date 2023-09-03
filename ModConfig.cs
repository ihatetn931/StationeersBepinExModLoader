using System.Collections.Generic;
using System.Xml.Serialization;

namespace BepInEx.StationeerModLoader
{
    //so I can read the Game ModConfig
    [XmlRoot(ElementName = "ModConfig")]
    public class ModConfig
    {
        public List<ModData> Mods { get; set; } = new List<ModData>();
    }
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
}
