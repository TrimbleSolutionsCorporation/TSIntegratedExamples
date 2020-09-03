namespace SpreadsheetReinforcement.Tools.Serialize
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// Basic serialization extensions
    /// </summary>
    public static class ClassXmlSerializer
    {
        /// <summary>
        /// Serializes to xml any class
        /// </summary>
        /// <typeparam name="T">Class Type to serialize</typeparam>
        /// <param name="database">Class to serialize</param>
        /// <param name="file">File info, file does not need to exist, but full path must be right</param>
        /// <returns>False if deserialization exception occurs</returns>
        public static bool SerializeToXml<T>(T database, FileInfo file)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var textWriter = new StreamWriter(file.FullName)) serializer.Serialize(textWriter, database);
                Debug.WriteLine("XmlSerializerService list of {0} type serialized to xml file {1}", database.GetType(),
                    file.FullName);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("XmlSerializerService:SerializeToXml failed with exception: " +
                                ex.Message + ex.InnerException + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Serializes from xml any class
        /// </summary>
        /// <typeparam name="T">Type of class to deserialize</typeparam>
        /// <param name="file">Class to deserialize, must exist</param>
        /// <returns>Strong type list of class objects</returns>
        public static T DeserializeFromXml<T>(FileInfo file)
        {
            var result = default(T);
            if (!file.Exists) return result;

            try
            {
                var deserializer = new XmlSerializer(typeof(T));
                using (var textReader = new StreamReader(file.FullName))
                    result = (T) deserializer.Deserialize(textReader);
                Debug.WriteLine("XmlSerializerService list of {0} deserialized from xml file {1}", result.GetType(),
                    file.FullName);
                return result;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("XmlSerializerService:DeserializeFromXml failed with exception: " + ex.Message +
                                ex.InnerException + ex.StackTrace);
                return result;
            }
        }
    }
}