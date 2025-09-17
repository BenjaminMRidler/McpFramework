using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using McpFramework.McpTypes;

namespace McpFramework
{
    public class McpJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            // Primitives
            if (typeof(McpGuid).IsAssignableFrom(typeToConvert)) return true;
            if (typeof(McpString).IsAssignableFrom(typeToConvert)) return true;
            if (typeof(McpInt).IsAssignableFrom(typeToConvert)) return true;
            if (typeof(McpDouble).IsAssignableFrom(typeToConvert)) return true;
            if (typeof(McpDecimal).IsAssignableFrom(typeToConvert)) return true;
            if (typeof(McpFloat).IsAssignableFrom(typeToConvert)) return true;
            if (typeof(McpBool).IsAssignableFrom(typeToConvert)) return true;
            if (typeof(McpDateTime).IsAssignableFrom(typeToConvert)) return true;

            // Collections
            if (typeToConvert.IsGenericType &&
                typeToConvert.GetGenericTypeDefinition() == typeof(McpCollection<>))
                return true;

            return false;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            // Handle primitives
            if (typeof(McpGuid).IsAssignableFrom(typeToConvert))
                return (JsonConverter)Activator.CreateInstance(typeof(McpGuidConverter<>).MakeGenericType(typeToConvert))!;
            if (typeof(McpString).IsAssignableFrom(typeToConvert))
                return (JsonConverter)Activator.CreateInstance(typeof(McpStringConverter<>).MakeGenericType(typeToConvert))!;
            if (typeof(McpInt).IsAssignableFrom(typeToConvert))
                return (JsonConverter)Activator.CreateInstance(typeof(McpIntConverter<>).MakeGenericType(typeToConvert))!;
            if (typeof(McpDouble).IsAssignableFrom(typeToConvert))
                return (JsonConverter)Activator.CreateInstance(typeof(McpDoubleConverter<>).MakeGenericType(typeToConvert))!;
            if (typeof(McpDecimal).IsAssignableFrom(typeToConvert))
                return (JsonConverter)Activator.CreateInstance(typeof(McpDecimalConverter<>).MakeGenericType(typeToConvert))!;
            if (typeof(McpFloat).IsAssignableFrom(typeToConvert))
                return (JsonConverter)Activator.CreateInstance(typeof(McpFloatConverter<>).MakeGenericType(typeToConvert))!;
            if (typeof(McpBool).IsAssignableFrom(typeToConvert))
                return (JsonConverter)Activator.CreateInstance(typeof(McpBoolConverter<>).MakeGenericType(typeToConvert))!;
            if (typeof(McpDateTime).IsAssignableFrom(typeToConvert))
                return (JsonConverter)Activator.CreateInstance(typeof(McpDateTimeConverter<>).MakeGenericType(typeToConvert))!;

            // Handle collections
            if (typeToConvert.IsGenericType &&
                typeToConvert.GetGenericTypeDefinition() == typeof(McpCollection<>))
            {
                var itemType = typeToConvert.GetGenericArguments()[0];
                return (JsonConverter)Activator.CreateInstance(
                    typeof(McpCollectionConverter<,>).MakeGenericType(typeToConvert, itemType))!;
            }

            throw new NotSupportedException($"No converter for {typeToConvert}");
        }

        // --- Converters ---
        private class McpGuidConverter<T> : JsonConverter<T> where T : McpGuid, new()
        {
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => new T { Value = reader.GetGuid() };

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (value?.Value != Guid.Empty)
                    writer.WriteStringValue(value.Value);
                else
                    writer.WriteNullValue();
            }
        }

        private class McpStringConverter<T> : JsonConverter<T> where T : McpString, new()
        {
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => new T { Value = reader.GetString() ?? string.Empty };

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (!string.IsNullOrEmpty(value?.Value))
                    writer.WriteStringValue(value.Value);
                else
                    writer.WriteNullValue();
            }
        }

        private class McpIntConverter<T> : JsonConverter<T> where T : McpInt, new()
        {
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => new T { Value = reader.GetInt32() };

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (value != null)
                    writer.WriteNumberValue(value.Value);
                else
                    writer.WriteNullValue();
            }
        }


        private class McpDoubleConverter<T> : JsonConverter<T> where T : McpDouble, new()
        {
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => new T { Value = reader.GetDouble() };

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (value != null)
                    writer.WriteNumberValue(value.Value);
                else
                    writer.WriteNullValue();
            }
        }

        private class McpDecimalConverter<T> : JsonConverter<T> where T : McpDecimal, new()
        {
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => new T { Value = reader.GetDecimal() };

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (value != null)
                    writer.WriteNumberValue(value.Value);
                else
                    writer.WriteNullValue();
            }
        }

        private class McpFloatConverter<T> : JsonConverter<T> where T : McpFloat, new()
        {
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => new T { Value = (float)reader.GetDouble() };

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (value != null)
                    writer.WriteNumberValue(value.Value);
                else
                    writer.WriteNullValue();
            }
        }


        private class McpBoolConverter<T> : JsonConverter<T> where T : McpBool, new()
        {
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => new T { Value = reader.GetBoolean() };

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (value != null)
                    writer.WriteBooleanValue(value.Value);
                else
                    writer.WriteNullValue();
            }
        }


        private class McpDateTimeConverter<T> : JsonConverter<T> where T : McpDateTime, new()
        {
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => new T { Value = reader.GetDateTime() };

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (value?.Value is DateTime dt)
                {
                    // Use ISO-8601 round-trip format
                    writer.WriteStringValue(dt.ToString("o"));
                }
                else
                {
                    writer.WriteNullValue();
                }
            }
        }

        private class McpCollectionConverter<TCollection, TItem> : JsonConverter<TCollection>
            where TCollection : McpCollection<TItem>, new()
            where TItem : class
        {
            public override TCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var list = JsonSerializer.Deserialize<List<TItem>>(ref reader, options) ?? new List<TItem>();
                var collection = new TCollection();
                collection.AddRange(list);
                return collection;
            }

            public override void Write(Utf8JsonWriter writer, TCollection value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value.AsEnumerable(), options);
            }
        }
    }
}
