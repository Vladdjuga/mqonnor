using mqonnor.Domain.Primitives;

namespace mqonnor.Tests.Domain;

public class ResultTests
{
    [Fact]
    public void Success_IsSuccess_True()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_IsFailure_True()
    {
        var result = Result.Failure("something went wrong");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("something went wrong", result.Error);
    }

    [Fact]
    public void SuccessT_Value_ReturnsValue()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void FailureT_Value_Throws()
    {
        var result = Result.Failure<int>("error");

        Assert.True(result.IsFailure);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void FailureT_Error_ReturnsMessage()
    {
        var result = Result.Failure<string>("not found");

        Assert.Equal("not found", result.Error);
    }
}
