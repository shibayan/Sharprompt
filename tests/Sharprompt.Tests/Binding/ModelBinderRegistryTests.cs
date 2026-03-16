using System;

using Xunit;

namespace Sharprompt.Tests;

public class ModelBinderRegistryTests
{
    [Fact]
    public void Register_And_TryGetBinder_ReturnsRegisteredBinder()
    {
        Action<TestModel> binder = _ => { };

        ModelBinderRegistry.Register(binder);

        var found = ModelBinderRegistry.TryGetBinder<TestModel>(out var retrievedBinder);

        Assert.True(found);
        Assert.Same(binder, retrievedBinder);
    }

    [Fact]
    public void TryGetBinder_Unregistered_ReturnsFalse()
    {
        var found = ModelBinderRegistry.TryGetBinder<UnregisteredModel>(out var binder);

        Assert.False(found);
        Assert.Null(binder);
    }

    public class TestModel;

    public class UnregisteredModel;
}
