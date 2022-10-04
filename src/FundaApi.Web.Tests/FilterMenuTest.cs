using Bunit;
using FundaApi.Web.Models;
using FundaApi.Web.Shared;
using MudBlazor.Services;
using Xunit;

namespace FundaApi.Web.Tests;

public class FilterMenuTest
{
    [Fact]
    public void ShouldRender_Successfully()
    {
        // Arrange 
        using var ctx = new TestContext();
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var c = ctx.RenderComponent<FilterMenu>(parameters => parameters
            .Add(p => p.Filter, new Filter())
            .Add(p => p.OnFilterChanged, args => { /* handle callback */ })
        );

        // Assert
        Assert.Equal(1, c.RenderCount);
    }

    [Fact]
    public void ShouldRender_AndUpdateAfterFilterChange()
    {
        // Arrange 
        using var ctx = new TestContext();
        ctx.Services.AddMudServices();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var c = ctx.RenderComponent<FilterMenu>(parameters => parameters
            .Add(p => p.Filter, new Filter())
            .Add(p => p.OnFilterChanged, args => { /* handle callback */ })
        );

        c.Find(".mud-input-slot").Input("amsterdam");

        // Assert
        Assert.Equal(2, c.RenderCount);
    }
}
