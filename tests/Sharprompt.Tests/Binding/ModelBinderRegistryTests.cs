using System;
using System.Threading;

using Xunit;

namespace Sharprompt.Tests;

public class ModelBinderRegistryTests
{
    [Fact]
    public void Register_And_TryGetBinder_ReturnsRegisteredBinder()
    {
        TestModel? boundModel = null;

        ModelBinderRegistry.Register<TestModel>(model => boundModel = model);

        var found = ModelBinderRegistry.TryGetBinder<TestModel>(out var retrievedBinder);

        Assert.True(found);

        var model = new TestModel();

        retrievedBinder!(model, CancellationToken.None);

        Assert.Same(model, boundModel);
    }

    [Fact]
    public void Register_WithCancellationToken_ReturnsRegisteredBinder()
    {
        Action<CancelableTestModel, CancellationToken> binder = (_, _) => { };

        ModelBinderRegistry.Register(binder);

        var found = ModelBinderRegistry.TryGetBinder<CancelableTestModel>(out var retrievedBinder);

        Assert.True(found);
        Assert.Same(binder, retrievedBinder);
    }

    [Fact]
    public void Register_NullBinder_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ModelBinderRegistry.Register((Action<TestModel>)null!));
        Assert.Throws<ArgumentNullException>(() => ModelBinderRegistry.Register((Action<TestModel, CancellationToken>)null!));
    }

    [Fact]
    public void Bind_PassesCancellationTokenToBinder()
    {
        CancellationToken boundToken = default;

        ModelBinderRegistry.Register<TokenCapturingModel>((_, cancellationToken) => boundToken = cancellationToken);

        using var cts = new CancellationTokenSource();

        var model = new TokenCapturingModel();

        Assert.Same(model, Prompt.Bind(model, cts.Token));
        Assert.Equal(cts.Token, boundToken);
    }

    [Fact]
    public void TryGetBinder_Unregistered_ReturnsFalse()
    {
        var found = ModelBinderRegistry.TryGetBinder<UnregisteredModel>(out var binder);

        Assert.False(found);
        Assert.Null(binder);
    }

    public class TestModel;

    public class CancelableTestModel;

    public class TokenCapturingModel;

    public class UnregisteredModel;
}
