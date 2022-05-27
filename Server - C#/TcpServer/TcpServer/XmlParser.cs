using System.Xml.Serialization;
using System.IO;

namespace TcpServer
{
    public static class XmlParser
    {
        public static T Parse<T>(string xml)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var stringReader = new StringReader(xml))
                return (T)xmlSerializer.Deserialize(stringReader);
        }
    }
}
