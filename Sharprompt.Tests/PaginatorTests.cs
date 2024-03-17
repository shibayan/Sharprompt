using System;
using System.Linq;

using Sharprompt.Internal;

using Xunit;

namespace Sharprompt.Tests;

public class PaginatorTests
{
    private readonly Func<int, string, bool> _containsTextInputFilter = (item, keyword) => item.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase);

    [Fact]
    public void Basic()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString(), _containsTextInputFilter);

        var currentItems1 = paginator.CurrentItems;

        Assert.Equal(5, currentItems1.Length);
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, currentItems1.ToArray());

        paginator.NextPage();

        var currentItems2 = paginator.CurrentItems;

        Assert.Equal(5, currentItems2.Length);
        Assert.Equal(new[] { 5, 6, 7, 8, 9 }, currentItems2.ToArray());
    }

    [Fact]
    public void Filter_NotEmpty()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString(), _containsTextInputFilter);

        paginator.UpdateFilter("0");

        var currentItems = paginator.CurrentItems;

        Assert.Equal(2, currentItems.Length);
        Assert.Equal(new[] { 0, 10 }, currentItems.ToArray());
    }

    [Fact]
    public void Filter_Empty()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString(), _containsTextInputFilter);

        paginator.UpdateFilter("x");

        var subset = paginator.CurrentItems;

        Assert.True(subset.IsEmpty);
    }

    [Fact]
    public void SelectedItem()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString(), _containsTextInputFilter);

        paginator.NextPage();
        paginator.NextItem();

        var selected = paginator.TryGetSelectedItem(out var selectedItem);

        Assert.True(selected);
        Assert.Equal(5, selectedItem);
    }

    [Fact]
    public void SelectedItem_NotSelected()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString(), _containsTextInputFilter);

        var selected = paginator.TryGetSelectedItem(out _);

        Assert.False(selected);
    }

    [Fact]
    public void SelectedItem_EmptyList()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString(), _containsTextInputFilter);

        paginator.UpdateFilter("x");
        paginator.NextItem();

        var selected = paginator.TryGetSelectedItem(out _);

        Assert.False(selected);
    }
}
