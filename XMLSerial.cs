﻿using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System;
namespace BepInEx.StationeerModLoader
{
    public static class XmlSerialization
    {
        public static T LoadOrCreateNew<T>(string path, Func<T> onNewCreated) where T : class
        {
            T t;
            if (!File.Exists(path))
            {
                t = onNewCreated();
                t.SaveXml(path);
            }
            else
            {
                t = XmlSerialization.Deserialize<T>(path, "");
                if (t == null)
                {
                    t = onNewCreated();
                    t.SaveXml(path);
                }
            }
            return t;
        }

        public static T LoadOrNull<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default(T);
            }
            return XmlSerialization.Deserialize<T>(path, "");
        }

        public static bool SaveXml<T>(this T savable, string path) where T : class
        {
            return XmlSerialization.Serialize<T>(savable, path);
        }

        public static bool Serialize<T>(T obj, string path)
        {
            bool result;
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (StreamWriter streamWriter = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(streamWriter, obj);
                    result = true;
                }
            }
            catch (Exception exception)
            {
                StationeerModLoader.Logger.LogError(exception);
                result = false;
            }
            return result;
        }

        public static bool Serialization(XmlSerializer xmlSerializer, object obj, string path)
        {
            if (xmlSerializer == null || obj == null)
            {
                return false;
            }
            bool result;
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(path))
                {
                    result = XmlSerialization.Serialization(xmlSerializer, streamWriter, obj, path);
                }
            }
            catch (Exception ex)
            {
                string str = (ex.InnerException != null) ? ("\nFurther info: " + ex.InnerException.Message) : string.Empty;
                StationeerModLoader.Logger.LogError("An error occured serializing data!\n" + ex.Message + str);
                result = false;
            }
            return result;
        }

        public static bool Serialization(XmlSerializer xmlSerializer, StreamWriter streamWriter, object obj, string path = "")
        {
            if (streamWriter == null || obj == null)
            {
                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
                return false;
            }
            bool result;
            try
            {
                xmlSerializer.Serialize(streamWriter, obj);
                result = true;
            }
            catch (Exception ex)
            {
                StationeerModLoader.Logger.LogError("An error occured serializing data!: " + path + " - " + ex.Message);
                result = false;
            }
            finally
            {
                streamWriter.Close();
            }
            return result;
        }

        public static object Deserialize(XmlSerializer xmlSerializer, string path)
        {
            if (xmlSerializer == null || !File.Exists(path))
            {
                return null;
            }
            object result;
            try
            {
                using (StreamReader streamReader = new StreamReader(path, Encoding.GetEncoding("UTF-8")))
                {
                    result = XmlSerialization.Deserialize(xmlSerializer, streamReader, path);
                }
            }
            catch (Exception ex)
            {
                string str = (ex.InnerException != null) ? ("\nFurther info: " + ex.InnerException.Message) : string.Empty;
                StationeerModLoader.Logger.LogError("An error occured deserializing data!\n" + ex.Message + str);
                result = false;
            }
            return result;
        }

        public static T Deserialize<T>(string path, string root = "")
        {
            T result;
            try
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("File not found at path: " + path);
                }
                XmlSerializer xmlSerializer = string.IsNullOrEmpty(root) ? new XmlSerializer(typeof(T)) : new XmlSerializer(typeof(T), new XmlRootAttribute(root));
                using (StreamReader streamReader = new StreamReader(path))
                {
                    result = (T)((object)xmlSerializer.Deserialize(streamReader));
                }
            }
            catch (Exception exception)
            {
                StationeerModLoader.Logger.LogError(exception);
                result = default(T);
            }
            return result;
        }
        public static readonly XmlReaderSettings XmlReaderSettings = new XmlReaderSettings
        {
            CheckCharacters = false
        };


        public static object Deserialize(XmlSerializer xmlSerializer, StreamReader streamReader, string path = "")
        {
            if (xmlSerializer == null || streamReader == null)
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                }
                return null;
            }
            object result;
            try
            {
                using (XmlReader xmlReader = XmlReader.Create(streamReader, XmlReaderSettings))
                {
                    result = xmlSerializer.Deserialize(xmlReader);
                }
            }
            catch (Exception ex)
            {
                StationeerModLoader.Logger.LogError(ex);
                StationeerModLoader.Logger.LogError(string.Concat(new string[]
                {
                    "An error occurred while deserializing a file!: ",
                    path,
                    " - ",
                    ex.Message,
                    (ex.InnerException != null) ? (" : " + ex.InnerException.Message) : ""
                }));
                result = null;
            }
            finally
            {
                streamReader.Close();
            }
            return result;
        }
    }
}