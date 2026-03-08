namespace mqonnor.Application.DTOs;

public sealed record PublishEventDto(
    byte[] Payload,
    string Encoding,
    string Source
);
