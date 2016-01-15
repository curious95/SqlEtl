using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SqlEtl.Helpers
{
    internal static class Serializer
    {
        /// <summary>
        ///     Serialize to xml string
        /// </summary>
        /// <typeparam name="T">Typed Class</typeparam>
        /// <param name="t">value</param>
        /// <returns>String</returns>
        public static string Serialize<T>(T t) where T : new()
        {
            var sb = new StringBuilder();
            try
            {
                var ser = new XmlSerializer(typeof (T));
                using (var swriter = new StringWriter(sb))
                {
                    ser.Serialize(swriter, t);
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Deserialize to Generic T
        /// </summary>
        /// <typeparam name="T">Typed class</typeparam>
        /// <param name="data">xml string</param>
        /// <returns>Generic T</returns>
        public static T Deserialize<T>(string data) where T : new()
        {
            var customType = new T();
            try
            {
                if (!string.IsNullOrEmpty(data))
                {
                    var serializer = new XmlSerializer(typeof (T));
                    using (var reader = new StringReader(data))
                    {
                        customType = (T) serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return customType;
        }
    }
}