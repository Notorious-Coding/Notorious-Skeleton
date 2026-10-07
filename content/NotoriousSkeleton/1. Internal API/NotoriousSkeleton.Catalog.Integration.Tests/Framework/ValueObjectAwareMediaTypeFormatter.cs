using System.Net.Http.Formatting;
using NotoriousSkeleton.Libraries.Domain.Tools.ValueObjects;
using Newtonsoft.Json;

namespace NotoriousSkeleton.Catalog.Integration.Tests.Framework;

public sealed class ValueObjectAwareJsonFormatter : JsonMediaTypeFormatter
{
    public ValueObjectAwareJsonFormatter()
    {
        SerializerSettings.Converters.Add(new ValueObjectJsonConverter());
    }
}

internal sealed class ValueObjectJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
        => objectType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValueObject<>));

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var valueType = objectType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValueObject<>))
            .GetGenericArguments()[0];

        var value = serializer.Deserialize(reader, valueType);
        return Activator.CreateInstance(objectType, value);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var valueProperty = value!.GetType().GetProperty(nameof(IValueObject<int>.Value));
        serializer.Serialize(writer, valueProperty!.GetValue(value));
    }
}
