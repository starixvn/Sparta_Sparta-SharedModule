using System.IO;
using Newtonsoft.Json;

namespace ONEBook.UM.Extensions
{
    internal static class Serializer
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
        };

        internal static string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    var serializer = JsonSerializer.Create(_serializerSettings);
                    serializer.Serialize(jsonWriter, obj);
                    return stringWriter.ToString();
                }
            }
        }

        internal static T Deserialize<T>(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return default(T);
            }

            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    var serializer = JsonSerializer.Create(_serializerSettings);
                    return serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }
    }
}