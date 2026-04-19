using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgendeX.WebAPI.Serialization;

public sealed class TimeOnlyMinutesJsonConverter : JsonConverter<TimeOnly>
{
    private static readonly string[] Formats = ["HH:mm", "HH:mm:ss"];

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
            throw new JsonException("Invalid time value.");

        if (TimeOnly.TryParseExact(value, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly time))
            return time;

        throw new JsonException("Invalid time value.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("HH:mm", CultureInfo.InvariantCulture));
    }
}