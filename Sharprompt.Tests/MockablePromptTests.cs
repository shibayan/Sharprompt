using System;
using System.Linq;
using System.Linq.Expressions;

using Moq;

using Sharprompt.Prompts;

using Xunit;

namespace Sharprompt.Tests;

public class MockablePromptTests
{
    [Theory]
    [InlineData("string 123")]
    [InlineData("𩸽𠈻𠮷")]
    [InlineData("")]
    [InlineData(null)]
    public void Mock_InputMethodOptions_WithString_IsSuccessful(string expectedValue)
    {
        InputOptions<string> param = null;
        SetupPromptMockRealisation(p => p.Input(param), expectedValue);

        var value = Prompt.Input(param);

        Assert.True(value == expectedValue);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Mock_InputMethodConfigure_WithBool_IsSuccessful(bool expectedValue)
    {
        Action<InputOptions<bool>> param = null;
        SetupPromptMockRealisation(p => p.Input(param), expectedValue);

        var value = Prompt.Input(param);

        Assert.True(value == expectedValue);
    }

    [Theory]
    [InlineData(123_456)]
    [InlineData(-100)]
    [InlineData(0)]
    public void Mock_InputMethodMessage_WithInt_IsSuccessful(int expectedValue)
    {
        string param = "Hello there!";
        SetupPromptMockRealisation(p => p.Input<int>(param,
            null, null, null), expectedValue);

        var value = Prompt.Input<int>(param);

        Assert.True(value == expectedValue);
    }

    [Fact]
    public void Mock_PasswordMethodOptions_IsSuccessful()
    {
        var expectedValue = "super secret password";

        PasswordOptions param = null;
        SetupPromptMockRealisation(p => p.Password(param), expectedValue);

        var value = Prompt.Password(param);

        Assert.True(value == expectedValue);
    }

    [Fact]
    public void Mock_PasswordMethodConfigure_IsSuccessful()
    {
        var expectedValue = "super secret password";

        Action<PasswordOptions> param = null;
        SetupPromptMockRealisation(p => p.Password(param), expectedValue);

        var value = Prompt.Password(param);

        Assert.True(value == expectedValue);
    }

    [Fact]
    public void Mock_PasswordMethodMessage_IsSuccessful()
    {
        var expectedValue = "super secret password";

        string param = null;
        SetupPromptMockRealisation(p => p.Password(param, null, null, null), expectedValue);

        var value = Prompt.Password(param, null);

        Assert.True(value == expectedValue);
    }

    [Fact]
    public void Mock_ConfirmMethodOptions_IsSuccessful()
    {
        var expectedValue = true;

        ConfirmOptions param = null;
        SetupPromptMockRealisation(p => p.Confirm(param), expectedValue);

        var value = Prompt.Confirm(param);

        Assert.True(value == expectedValue);
    }

    [Fact]
    public void Mock_ConfirmMethodConfigure_IsSuccessful()
    {
        var expectedValue = true;

        Action<ConfirmOptions> param = null;
        SetupPromptMockRealisation(p => p.Confirm(param), expectedValue);

        var value = Prompt.Confirm(param);

        Assert.True(value == expectedValue);
    }

    [Fact]
    public void Mock_ConfirmMethodMessage_IsSuccessful()
    {
        var expectedValue = true;

        string param = null;
        SetupPromptMockRealisation(p => p.Confirm(param, null), expectedValue);

        var value = Prompt.Confirm(param);

        Assert.True(value == expectedValue);
    }

    [Fact]
    public void Mock_SelectMethodOptions_WithCustomStruct_IsSuccessful()
    {
        CustomStruct expectedValue = new CustomStruct() { Foo = false, Bar = 12345 };

        SelectOptions<CustomStruct> param = null;
        SetupPromptMockRealisation(p => p.Select(param), expectedValue);

        var value = Prompt.Select(param);

        Assert.True(value.Equals(expectedValue));
    }

    [Fact]
    public void Mock_SelectMethodConfigure_WithCustomRecord_IsSuccessful()
    {
        CustomRecord expectedValue = new CustomRecord() { Foo = 98765, Bar = "hello"};

        Action<SelectOptions<CustomRecord>> param = null;
        SetupPromptMockRealisation(p => p.Select(param), expectedValue);

        var value = Prompt.Select(param);

        Assert.True(value == expectedValue);
    }

    [Fact]
    public void Mock_SelectMethodMessage_WithCustomClass_IsSuccessful()
    {
        CustomClass expectedValue = new CustomClass { Foo = 98765, Bar = "hello", Struct = new CustomStruct()};

        string param = null;
        SetupPromptMockRealisation(p => p.Select<CustomClass>(param, null, null, null, null), expectedValue);

        var value = Prompt.Select<CustomClass>(param);

        Assert.True(value == expectedValue);
    }

    [Fact]
    public void Mock_MultiSelectMethodOptions_IsSuccessful()
    {
        int[] expectedValues = { 1, 2, 3, 4, 5 };

        MultiSelectOptions<int> param = null;
        SetupPromptMockRealisation(p => p.MultiSelect(param), expectedValues);

        var value = Prompt.MultiSelect(param);

        Assert.True(value.SequenceEqual(expectedValues));
    }

    [Fact]
    public void Mock_MultiSelectMethodConfigure_IsSuccessful()
    {
        int[] expectedValues = { 1, 22, 333, 4444, 5555 };

        Action<MultiSelectOptions<int>> param = null;
        SetupPromptMockRealisation(p => p.MultiSelect(param), expectedValues);

        var value = Prompt.MultiSelect(param);

        Assert.True(value.SequenceEqual(expectedValues));
    }

    [Fact]
    public void Mock_MultiSelectMethodMessage_IsSuccessful()
    {
        int[] expectedValues = { -1, 22, -333, 4444, -5555, 0 };

        string param = null;
        SetupPromptMockRealisation(p => p.MultiSelect<int>(param, null, null,
            1, 2, null, null), expectedValues);

        var value = Prompt.MultiSelect<int>(param, null, null, 1, 2, null, null);

        Assert.True(value.SequenceEqual(expectedValues));
    }

    [Fact]
    public void Mock_ListMethodOptions_IsSuccessful()
    {
        int[] expectedValues = { 0, 1, 0, 1 };

        ListOptions<int> param = null;
        SetupPromptMockRealisation(p => p.List(param), expectedValues);

        var value = Prompt.List(param);

        Assert.True(value.SequenceEqual(expectedValues));
    }

    [Fact]
    public void Mock_ListMethodConfigure_IsSuccessful()
    {
        int[] expectedValues = { 88, 66, 22, 11 };

        Action<ListOptions<int>> param = null;
        SetupPromptMockRealisation(p => p.List(param), expectedValues);

        var value = Prompt.List(param);

        Assert.True(value.SequenceEqual(expectedValues));
    }

    [Fact]
    public void Mock_ListMethodMessage_IsSuccessful()
    {
        int[] expectedValues = { 0, 0, 1, 0, 0 };

        string param = null;
        SetupPromptMockRealisation(p => p.List<int>(param, 1, 2, null), expectedValues);

        var value = Prompt.List<int>(param, 1, 2);

        Assert.True(value.SequenceEqual(expectedValues));
    }

    [Fact]
    public void Mock_BindMethod_IsSuccessful()
    {
        int expectedValue = 12345;

        SetupPromptMockRealisation(p => p.Bind<int>(), expectedValue);

        var value = Prompt.Bind<int>();

        Assert.True(value == expectedValue);
    }

    [Fact]
    public void Mock_BindMethodModel_IsSuccessful()
    {
        int expectedValue = 12345;

        int param = 999;
        SetupPromptMockRealisation(p => p.Bind(param), expectedValue);

        var value = Prompt.Bind(param);

        Assert.True(value == expectedValue);
    }

    private static void SetupPromptMockRealisation<T>(Expression<Func<IPrompt, T>> expression, T returnedValue)
    {
        var mock = new Mock<IPrompt>();
        mock.Setup(expression).Returns(returnedValue);
        Prompt.PromptRealisation = mock.Object;
    }

    public record CustomRecord
    {
        public int Foo { get; set; }

        public string Bar { get; set; }
    }

    public struct CustomStruct
    {
        public bool Foo { get; set; }

        public long Bar { get; set; }
    }

    public class CustomClass
    {
        public CustomStruct Struct { get; set; }

        public int Foo { get; set; }

        public string Bar { get; set; }
    }
}
