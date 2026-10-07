using System.Text.Json;
using System.Text.Json.Serialization;
using NotoriousSkeleton.Libraries.Domain.Tools.ValueObjects;

namespace NotoriousSkeleton.API.Serializer;

public class JsonValueObjectConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValueObject<>));

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValueObject<>))
            .GetGenericArguments()[0];

        var converterType = typeof(JsonValueObjectConverter<,>).MakeGenericType(typeToConvert, valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class JsonValueObjectConverter<T, TValue> : JsonConverter<T>
    where TValue: notnull
    where T : struct, IValueObject<TValue>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = JsonSerializer.Deserialize<TValue>(ref reader, options)!;
        return (T)Activator.CreateInstance(typeof(T), value)!;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value.Value, options);
}