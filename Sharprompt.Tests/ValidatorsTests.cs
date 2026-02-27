using System.ComponentModel.DataAnnotations;

using Xunit;

namespace Sharprompt.Tests;

public class ValidatorsTests
{
    [Fact]
    public void Required_Null_ReturnsError()
    {
        var validator = Validators.Required();

        var result = validator(null);

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void Required_EmptyString_ReturnsError()
    {
        var validator = Validators.Required();

        var result = validator("");

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void Required_NonEmptyString_ReturnsSuccess()
    {
        var validator = Validators.Required();

        var result = validator("hello");

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void Required_NonNullObject_ReturnsSuccess()
    {
        var validator = Validators.Required();

        var result = validator(42);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void Required_CustomErrorMessage()
    {
        var validator = Validators.Required("custom error");

        var result = validator(null);

        Assert.NotNull(result);
        Assert.Equal("custom error", result!.ErrorMessage);
    }

    [Fact]
    public void MinLength_ShortString_ReturnsError()
    {
        var validator = Validators.MinLength(5);

        var result = validator("abc");

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void MinLength_ExactLength_ReturnsSuccess()
    {
        var validator = Validators.MinLength(3);

        var result = validator("abc");

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void MinLength_LongerString_ReturnsSuccess()
    {
        var validator = Validators.MinLength(3);

        var result = validator("abcdef");

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void MinLength_NonString_ReturnsSuccess()
    {
        var validator = Validators.MinLength(5);

        var result = validator(42);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void MinLength_CustomErrorMessage()
    {
        var validator = Validators.MinLength(5, "too short");

        var result = validator("ab");

        Assert.NotNull(result);
        Assert.Equal("too short", result!.ErrorMessage);
    }

    [Fact]
    public void MaxLength_LongString_ReturnsError()
    {
        var validator = Validators.MaxLength(3);

        var result = validator("abcdef");

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void MaxLength_ExactLength_ReturnsSuccess()
    {
        var validator = Validators.MaxLength(3);

        var result = validator("abc");

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void MaxLength_ShorterString_ReturnsSuccess()
    {
        var validator = Validators.MaxLength(5);

        var result = validator("ab");

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void MaxLength_NonString_ReturnsSuccess()
    {
        var validator = Validators.MaxLength(3);

        var result = validator(42);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void MaxLength_CustomErrorMessage()
    {
        var validator = Validators.MaxLength(3, "too long");

        var result = validator("abcdef");

        Assert.NotNull(result);
        Assert.Equal("too long", result!.ErrorMessage);
    }

    [Fact]
    public void RegularExpression_Match_ReturnsSuccess()
    {
        var validator = Validators.RegularExpression(@"^\d+$");

        var result = validator("12345");

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void RegularExpression_NoMatch_ReturnsError()
    {
        var validator = Validators.RegularExpression(@"^\d+$");

        var result = validator("abc");

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void RegularExpression_NonString_ReturnsSuccess()
    {
        var validator = Validators.RegularExpression(@"^\d+$");

        var result = validator(42);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void RegularExpression_CustomErrorMessage()
    {
        var validator = Validators.RegularExpression(@"^\d+$", "digits only");

        var result = validator("abc");

        Assert.NotNull(result);
        Assert.Equal("digits only", result!.ErrorMessage);
    }
}
