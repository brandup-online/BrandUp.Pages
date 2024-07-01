using System.Text.Json;

namespace BrandUp.Pages.Content.Serialization
{
    public static class JsonContentSerializer
    {
        public static string Serialize(IDictionary<string, object> data)
        {
            ArgumentNullException.ThrowIfNull(data);

            return JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        public static IDictionary<string, object> Deserialize(string jsonData)
        {
            ArgumentNullException.ThrowIfNull(jsonData);

            using var stringReader = new StringReader(jsonData);
            ReadOnlySpan<byte> jsonUtf8 = System.Text.Encoding.UTF8.GetBytes(jsonData);
            var jsonReader = new Utf8JsonReader(jsonUtf8);

            if (!jsonReader.Read())
                return null;

            return ReadDictionary(ref jsonReader);
        }

        public static IDictionary<string, object> Deserialize(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            using var stringReader = new StreamReader(stream);
            return Deserialize(stringReader.ReadToEnd());
        }

        #region Helpers

        static void WriteDictionary(Utf8JsonWriter writer, IDictionary<string, object> data)
        {
            writer.WriteStartObject();

            foreach (var kv in data)
            {
                writer.WritePropertyName(kv.Key);

                var value = kv.Value;
                if (value == null)
                    writer.WriteNullValue();
                else if (value is IDictionary<string, object> valueDict)
                    WriteDictionary(writer, valueDict);
                else if (value is IList<IDictionary<string, object>> valueList)
                    WriteList(writer, valueList);
                else if (value is string)
                    writer.WriteStringValue((string)kv.Value);
                else if (value is bool)
                    writer.WriteBooleanValue((bool)kv.Value);
                else if (value is short)
                    writer.WriteNumberValue((short)kv.Value);
                else if (value is int)
                    writer.WriteNumberValue((int)kv.Value);
                else if (value is long)
                    writer.WriteNumberValue((long)kv.Value);
                else if (value is float)
                    writer.WriteNumberValue((float)kv.Value);
                else if (value is decimal)
                    writer.WriteNumberValue((decimal)kv.Value);
                else if (value is double)
                    writer.WriteNumberValue((double)kv.Value);
                else if (value is ulong)
                    writer.WriteNumberValue((ulong)kv.Value);
                else if (value is uint)
                    writer.WriteNumberValue((uint)kv.Value);
                else if (value is DateTime)
                    writer.WriteStringValue((DateTime)kv.Value);
                else if (value is DateTimeOffset)
                    writer.WriteStringValue((DateTimeOffset)kv.Value);
                else if (value is Guid)
                    writer.WriteStringValue((Guid)kv.Value);
                else
                    writer.WriteStringValue(kv.Value.ToString());
            }

            writer.WriteEndObject();
        }

        static void WriteList(Utf8JsonWriter writer, IList<IDictionary<string, object>> data)
        {
            writer.WriteStartArray();

            foreach (var listItem in data)
                WriteDictionary(writer, listItem);

            writer.WriteEndArray();
        }

        static IDictionary<string, object> ReadDictionary(ref Utf8JsonReader reader)
        {
            var dictionary = new SortedDictionary<string, object>();

            string fieldName = null;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        {
                            fieldName = reader.GetString();
                            break;
                        }
                    case JsonTokenType.Null:
                        {
                            if (fieldName == null)
                                throw new InvalidOperationException();

                            dictionary.Add(fieldName, null);

                            break;
                        }
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        {
                            if (fieldName == null)
                                throw new InvalidOperationException();

                            dictionary.Add(fieldName, reader.GetBoolean());

                            break;
                        }
                    case JsonTokenType.Number:
                    case JsonTokenType.String:
                        {
                            if (fieldName == null)
                                throw new InvalidOperationException();

                            dictionary.Add(fieldName, reader.GetString());

                            break;
                        }
                    case JsonTokenType.StartObject:
                        {
                            if (fieldName == null)
                                throw new InvalidOperationException();

                            var dictValue = ReadDictionary(ref reader);
                            dictionary.Add(fieldName, dictValue);

                            break;
                        }
                    case JsonTokenType.StartArray:
                        {
                            if (fieldName == null)
                                throw new InvalidOperationException();

                            var listValue = ReadList(ref reader);
                            dictionary.Add(fieldName, listValue);

                            break;
                        }
                    case JsonTokenType.EndObject:
                        return dictionary;
                    case JsonTokenType.EndArray:
                        throw new InvalidOperationException();
                    case JsonTokenType.None:
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return dictionary;
        }

        static IList<IDictionary<string, object>> ReadList(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new InvalidOperationException();

            var list = new List<IDictionary<string, object>>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                var item = ReadDictionary(ref reader);
                list.Add(item);
            }

            return list;
        }

        #endregion
    }
}