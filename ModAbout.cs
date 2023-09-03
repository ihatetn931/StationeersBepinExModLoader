using System.Collections.Generic;
using System.Xml.Serialization;

namespace BepInEx.StationeerModLoader
{
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
}
