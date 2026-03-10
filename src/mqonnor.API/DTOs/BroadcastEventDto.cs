using System.Text.Json;

namespace mqonnor.API.DTOs;

public sealed record BroadcastEventDto(
    Guid Id,
    JsonElement Payload,
    string Encoding,
    string Source
);
