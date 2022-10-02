using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FundaApi.Core.Contracts;
using FundaApi.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FundaApi.Tests;

public class AgentEndpointsTests
{

    private readonly Mock<IBrokerDataProvider> _providerMock;

    public AgentEndpointsTests()
    {
        _providerMock = new Mock<IBrokerDataProvider>();
    }

    [Theory]
    [InlineData("amsterdam")]
    [InlineData("rotterdam")]
    [InlineData("den-haag")]
    public async Task GetAgentsFromLocation_ShouldReturn200_WhenLocationExists(string location)
    {
        // Arrange
        var expected = new List<RealEstateAgentWithCount>() { new RealEstateAgentWithCount(1, "name", 1) };

        _providerMock
            .Setup(s => s.GetRealEstateAgents(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        using var app = new TestApplicationFactory((services) =>
        {
            services.AddSingleton(_providerMock.Object);
        });
        var httpClient = app.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"/agents/{location}/");
        var responseText = await response.Content.ReadAsStringAsync();
        var customerResult = JsonSerializer.Deserialize<IReadOnlyList<RealEstateAgentWithCount>>(responseText, new JsonSerializerOptions { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        customerResult.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAgentsFromLocation_ShouldReturn404_WhenLocationDoesNotExists()
    {
        // Arrange
        _providerMock
            .Setup(s => s.GetRealEstateAgents(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        using var app = new TestApplicationFactory((services) =>
        {
            services.AddSingleton(_providerMock.Object);
        });
        var httpClient = app.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"/agents/hello-world/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("amsterdam", "tuin")]
    [InlineData("amsterdam", "balkon")]
    [InlineData("amsterdam", null)]
    public async Task GetAgentsFromLocationWithOutdoorspace_ShouldReturn200_WhenOutdoorspaceExists(string location, string? outdoorspace)
    {
        // Arrange
        var expected = new List<RealEstateAgentWithCount>() { new RealEstateAgentWithCount(1, "name", 1) };

        _providerMock
            .Setup(s => s.GetRealEstateAgents(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        using var app = new TestApplicationFactory((services) =>
        {
            services.AddSingleton(_providerMock.Object);
        });
        var httpClient = app.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"/agents/{location}/{outdoorspace}");
        var responseText = await response.Content.ReadAsStringAsync();
        var customerResult = JsonSerializer.Deserialize<IReadOnlyList<RealEstateAgentWithCount>>(responseText, new JsonSerializerOptions { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        customerResult.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAgentsFromLocation_ShouldReturn404_WhenOutdoorspaceDoesNotExists()
    {
        // Arrange
        _providerMock
            .Setup(s => s.GetRealEstateAgents(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        using var app = new TestApplicationFactory((services) =>
        {
            services.AddSingleton(_providerMock.Object);
        });
        var httpClient = app.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"/agents/amsterdam/hello-world/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAgentsFromLocation_ShouldReturn500_WhenExceptionIsThrown()
    {
        // Arrange
        _providerMock
            .Setup(s => s.GetRealEstateAgents(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception("BOOM!"));

        using var app = new TestApplicationFactory((services) =>
        {
            services.AddSingleton(_providerMock.Object);
        });
        var httpClient = app.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"/agents/amsterdam/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
