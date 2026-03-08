namespace mqonnor.Domain;

public readonly struct EventMetainfo
{
    public string Encoding { get; init; }
    public int Length { get; init; }
    public string Source { get; init; }

    public EventMetainfo(string encoding, int length, string source)
    {
        Encoding = encoding;
        Length = length;
        Source = source;
    }
}
