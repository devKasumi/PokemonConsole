using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MoveType
{
    Physical,
    Special,
    Status
}