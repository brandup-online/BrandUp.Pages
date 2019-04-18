using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BrandUp.Pages.Content.Serialization
{
    public static class JsonContentDataSerializer
    {
        public static string SerializeToString(IDictionary<string, object> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using (var stringWriter = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    WriteDictionary(jsonWriter, data);
                }

                stringWriter.Flush();

                return stringWriter.ToString();
            }
        }
        private static void WriteDictionary(JsonTextWriter writer, IDictionary<string, object> data)
        {
            writer.WriteStartObject();

            foreach (var kv in data)
            {
                writer.WritePropertyName(kv.Key);

                var value = kv.Value;
                if (value is IDictionary<string, object> valueDict)
                    WriteDictionary(writer, valueDict);
                else if (value is IList<IDictionary<string, object>> valueList)
                    WriteList(writer, valueList);
                else
                    writer.WriteValue(kv.Value);
            }

            writer.WriteEndObject();
        }
        private static void WriteList(JsonTextWriter writer, IList<IDictionary<string, object>> data)
        {
            writer.WriteStartArray();

            foreach (var listItem in data)
            {
                WriteDictionary(writer, listItem);
            }

            writer.WriteEndArray();
        }

        public static IDictionary<string, object> DeserializeFromString(string jsonData)
        {
            if (jsonData == null)
                throw new ArgumentNullException(nameof(jsonData));

            using (var stringReader = new StringReader(jsonData))
            {
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    if (!jsonReader.Read())
                        throw new InvalidOperationException();

                    return ReadDictionary(jsonReader);
                }
            }
        }
        public static IDictionary<string, object> DeserializeFromStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var stringReader = new StreamReader(stream))
            {
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    if (!jsonReader.Read())
                        throw new InvalidOperationException();

                    return ReadDictionary(jsonReader);
                }
            }
        }
        private static IDictionary<string, object> ReadDictionary(JsonTextReader reader)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new InvalidOperationException();

            var dictionary = new SortedDictionary<string, object>();

            string fieldName = null;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        {
                            fieldName = (string)reader.Value;
                            break;
                        }
                    case JsonToken.String:
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.Date:
                    case JsonToken.Boolean:
                    case JsonToken.Bytes:
                    case JsonToken.Null:
                        {
                            if (fieldName == null)
                                throw new InvalidOperationException();

                            dictionary.Add(fieldName, reader.Value);

                            break;
                        }
                    case JsonToken.StartObject:
                        {
                            if (fieldName == null)
                                throw new InvalidOperationException();

                            var dictValue = ReadDictionary(reader);
                            dictionary.Add(fieldName, dictValue);

                            break;
                        }
                    case JsonToken.StartArray:
                        {
                            if (fieldName == null)
                                throw new InvalidOperationException();

                            var listValue = ReadList(reader);
                            dictionary.Add(fieldName, listValue);

                            break;
                        }
                    case JsonToken.EndObject:
                        return dictionary;
                    case JsonToken.EndArray:
                        throw new InvalidOperationException();
                    case JsonToken.None:
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return dictionary;
        }
        private static IList<IDictionary<string, object>> ReadList(JsonTextReader reader)
        {
            if (reader.TokenType != JsonToken.StartArray)
                throw new InvalidOperationException();

            var list = new List<IDictionary<string, object>>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndArray)
                    break;

                var item = ReadDictionary(reader);
                list.Add(item);
            }

            return list;
        }
    }
}