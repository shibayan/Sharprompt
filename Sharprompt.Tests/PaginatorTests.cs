using System.Linq;

using Sharprompt.Internal;

using Xunit;

namespace Sharprompt.Tests;

public class PaginatorTests
{
    [Fact]
    public void Basic()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        var subset1 = paginator.ToSubset();

        Assert.Equal(5, subset1.Count);
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, subset1);

        paginator.NextPage();

        var subset2 = paginator.ToSubset();

        Assert.Equal(5, subset2.Count);
        Assert.Equal(new[] { 5, 6, 7, 8, 9 }, subset2);
    }

    [Fact]
    public void Filter_NotEmpty()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.UpdateFilter("0");

        var subset = paginator.ToSubset();

        Assert.Equal(2, subset.Count);
        Assert.Equal(new[] { 0, 10 }, subset);
    }

    [Fact]
    public void Filter_Empty()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.UpdateFilter("x");

        var subset = paginator.ToSubset();

        Assert.Empty(subset);
    }

    [Fact]
    public void SelectedItem()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.NextPage();
        paginator.NextItem();

        var selected = paginator.TryGetSelectedItem(out var selectedItem);

        Assert.True(selected);
        Assert.Equal(5, selectedItem);
    }

    [Fact]
    public void SelectedItem_NotSelected()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        var selected = paginator.TryGetSelectedItem(out _);

        Assert.False(selected);
    }

    [Fact]
    public void SelectedItem_EmptyList()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.UpdateFilter("x");
        paginator.NextItem();

        var selected = paginator.TryGetSelectedItem(out _);

        Assert.False(selected);
    }
}
