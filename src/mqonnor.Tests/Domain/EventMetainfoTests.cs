using mqonnor.Domain.Entities;

namespace mqonnor.Tests.Domain;

public class EventMetainfoTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var metainfo = new EventMetainfo("UTF-8", 128, "test-service");

        Assert.Equal("UTF-8", metainfo.Encoding);
        Assert.Equal(128, metainfo.Length);
        Assert.Equal("test-service", metainfo.Source);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new EventMetainfo("UTF-8", 64, "svc");
        var b = new EventMetainfo("UTF-8", 64, "svc");

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = new EventMetainfo("UTF-8", 64, "svc-a");
        var b = new EventMetainfo("UTF-8", 64, "svc-b");

        Assert.NotEqual(a, b);
    }
}
