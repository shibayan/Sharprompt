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
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.UpdateFilter("0");

        var currentItems = paginator.CurrentItems;

        Assert.Equal(2, currentItems.Length);
        Assert.Equal(new[] { 0, 10 }, currentItems.ToArray());
    }

    [Fact]
    public void Filter_Empty()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.UpdateFilter("x");

        var subset = paginator.CurrentItems;

        Assert.True(subset.IsEmpty);
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

    [Fact]
    public void PreviousItem()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.NextItem();
        paginator.NextItem();
        paginator.PreviousItem();

        var selected = paginator.TryGetSelectedItem(out var selectedItem);

        Assert.True(selected);
        Assert.Equal(0, selectedItem);
    }

    [Fact]
    public void PreviousPage()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.NextPage();
        paginator.NextPage();
        paginator.PreviousPage();

        var currentItems = paginator.CurrentItems;

        Assert.Equal(5, currentItems.Length);
        Assert.Equal(new[] { 5, 6, 7, 8, 9 }, currentItems.ToArray());
    }

    [Fact]
    public void PreviousPage_WrapsAround()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.PreviousPage();

        var currentItems = paginator.CurrentItems;

        Assert.Equal(5, currentItems.Length);
        Assert.Equal(new[] { 15, 16, 17, 18, 19 }, currentItems.ToArray());
    }

    [Fact]
    public void NextPage_WrapsAround()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.NextPage();
        paginator.NextPage();
        paginator.NextPage();
        paginator.NextPage();

        var currentItems = paginator.CurrentItems;

        Assert.Equal(5, currentItems.Length);
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, currentItems.ToArray());
    }

    [Fact]
    public void PageCount()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        Assert.Equal(4, paginator.PageCount);
    }

    [Fact]
    public void TotalCount()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        Assert.Equal(20, paginator.TotalCount);
    }

    [Fact]
    public void TotalCount_WithFilter()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.UpdateFilter("1");

        Assert.Equal(11, paginator.TotalCount);
    }

    [Fact]
    public void FilterKeyword()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.UpdateFilter("test");

        Assert.Equal("test", paginator.FilterKeyword);
    }

    [Fact]
    public void DefaultValue_SetsSelectedItem()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, new Optional<int>(7), x => x.ToString());

        var selected = paginator.TryGetSelectedItem(out var selectedItem);

        Assert.True(selected);
        Assert.Equal(7, selectedItem);
        Assert.Equal(1, paginator.CurrentPage);
    }

    [Fact]
    public void LoopingSelection_NextItem_WrapsWithinPage()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());
        paginator.LoopingSelection = true;

        // 6 calls: -1 -> 0 -> 1 -> 2 -> 3 -> 4 -> 0 (wrap)
        for (var i = 0; i < 6; i++)
        {
            paginator.NextItem();
        }

        var selected = paginator.TryGetSelectedItem(out var selectedItem);

        Assert.True(selected);
        Assert.Equal(0, selectedItem);
        Assert.Equal(0, paginator.CurrentPage);
    }

    [Fact]
    public void UpdatePageSize()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.UpdatePageSize(10);

        var currentItems = paginator.CurrentItems;

        Assert.Equal(10, currentItems.Length);
        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, currentItems.ToArray());
    }

    [Fact]
    public void UpdatePageSize_SameSize_NoChange()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.NextPage();
        paginator.UpdatePageSize(5);

        Assert.Equal(1, paginator.CurrentPage);
    }

    [Fact]
    public void GetEnumerator()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 5), 5, Optional<int>.Empty, x => x.ToString());

        var items = paginator.ToList();

        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, items);
    }

    [Fact]
    public void SinglePage_NextPage_NoChange()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 3), 5, Optional<int>.Empty, x => x.ToString());

        paginator.NextPage();

        Assert.Equal(0, paginator.CurrentPage);
    }

    [Fact]
    public void SinglePage_PreviousPage_NoChange()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 3), 5, Optional<int>.Empty, x => x.ToString());

        paginator.PreviousPage();

        Assert.Equal(0, paginator.CurrentPage);
    }

    [Fact]
    public void SingleFilteredItem_AutoSelects()
    {
        var paginator = new Paginator<int>(Enumerable.Range(0, 20), 5, Optional<int>.Empty, x => x.ToString());

        paginator.UpdateFilter("19");

        var selected = paginator.TryGetSelectedItem(out var selectedItem);

        Assert.True(selected);
        Assert.Equal(19, selectedItem);
    }
}
