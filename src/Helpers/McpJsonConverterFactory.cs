using McpFramework.McpTypes;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpFramework;

public class McpJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeof(McpValue).IsAssignableFrom(typeToConvert)) return true;
        if (IsMcpCollection(typeToConvert)) return true;
        return false;
    }

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        // Handle collections
        if (IsMcpCollection(type, out var elementType))
        {
            return (JsonConverter)Activator.CreateInstance(
                typeof(CollectionConverter<,>).MakeGenericType(type, elementType))!;
        }

        // Handle primitives
        var mcpBase = FindMcpTypedValueBase(type);
        if (mcpBase != null)
        {
            var innerType = mcpBase.GetGenericArguments()[0];

            if (innerType == typeof(Guid))
                return (JsonConverter)Activator.CreateInstance(typeof(GuidConverter<>).MakeGenericType(type))!;
            if (innerType == typeof(int))
                return (JsonConverter)Activator.CreateInstance(typeof(IntConverter<>).MakeGenericType(type))!;
            if (innerType == typeof(double))
                return (JsonConverter)Activator.CreateInstance(typeof(DoubleConverter<>).MakeGenericType(type))!;
            if (innerType == typeof(decimal))
                return (JsonConverter)Activator.CreateInstance(typeof(DecimalConverter<>).MakeGenericType(type))!;
            if (innerType == typeof(float))
                return (JsonConverter)Activator.CreateInstance(typeof(FloatConverter<>).MakeGenericType(type))!;
            if (innerType == typeof(DateTime?))
                return (JsonConverter)Activator.CreateInstance(typeof(DateTimeConverter<>).MakeGenericType(type))!;
            if (innerType == typeof(string))
                return (JsonConverter)Activator.CreateInstance(typeof(StringConverter<>).MakeGenericType(type))!;
            if (innerType == typeof(bool))
                return (JsonConverter)Activator.CreateInstance(typeof(BoolConverter<>).MakeGenericType(type))!;
        }

        throw new NotSupportedException($"No converter for {type}");
    }

    // === Helpers ===
    private static Type? FindMcpTypedValueBase(Type type)
    {
        var current = type;
        while (current != null && current != typeof(object))
        {
            if (current.IsGenericType &&
                current.GetGenericTypeDefinition() == typeof(McpTypedValue<>))
            {
                return current;
            }
            current = current.BaseType;
        }
        return null;
    }

    private static bool IsMcpCollection(Type type) =>
        IsMcpCollection(type, out _);

    private static bool IsMcpCollection(Type type, out Type elementType)
    {
        var current = type;
        while (current != null && current != typeof(object))
        {
            if (current.IsGenericType &&
                current.GetGenericTypeDefinition() == typeof(McpCollection<>))
            {
                elementType = current.GetGenericArguments()[0];
                return true;
            }
            current = current.BaseType;
        }
        elementType = null!;
        return false;
    }

    // === Primitive converters ===
    private class GuidConverter<T> : JsonConverter<T> where T : McpTypedValue<Guid>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            (T)Activator.CreateInstance(typeof(T), reader.GetGuid())!;

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.Value);
    }

    private class IntConverter<T> : JsonConverter<T> where T : McpTypedValue<int>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            (T)Activator.CreateInstance(typeof(T), reader.GetInt32())!;

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(value.Value);
    }

    private class DoubleConverter<T> : JsonConverter<T> where T : McpTypedValue<double>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            (T)Activator.CreateInstance(typeof(T), reader.GetDouble())!;

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(value.Value);
    }

    private class DecimalConverter<T> : JsonConverter<T> where T : McpTypedValue<decimal>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            (T)Activator.CreateInstance(typeof(T), reader.GetDecimal())!;

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(value.Value);
    }

    private class FloatConverter<T> : JsonConverter<T> where T : McpTypedValue<float>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            (T)Activator.CreateInstance(typeof(T), reader.GetSingle())!;

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(value.Value);
    }

    private class DateTimeConverter<T> : JsonConverter<T> where T : McpTypedValue<DateTime?>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return (T)Activator.CreateInstance(typeof(T), (DateTime?)null)!;

            return (T)Activator.CreateInstance(typeof(T), reader.GetDateTime())!;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value?.Value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.Value.Value);
        }
    }

    private class StringConverter<T> : JsonConverter<T> where T : McpTypedValue<string>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            (T)Activator.CreateInstance(typeof(T), reader.GetString() ?? string.Empty)!;

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.Value);
    }

    private class BoolConverter<T> : JsonConverter<T> where T : McpTypedValue<bool>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            (T)Activator.CreateInstance(typeof(T), reader.GetBoolean())!;

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteBooleanValue(value.Value);
    }

    // === Collection converter ===
    private class CollectionConverter<TCollection, TElement> : JsonConverter<TCollection>
        where TCollection : McpCollection<TElement>
        where TElement : class
    {
        public override TCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var items = JsonSerializer.Deserialize<List<TElement>>(ref reader, options) ?? new List<TElement>();
            return (TCollection)Activator.CreateInstance(typeToConvert, items)!;
        }

        public override void Write(Utf8JsonWriter writer, TCollection value, JsonSerializerOptions options)
        {
            var items = value.AsEnumerable();
            JsonSerializer.Serialize(writer, items, options);
        }
    }
}
