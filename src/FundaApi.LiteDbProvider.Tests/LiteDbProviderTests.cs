using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FundaApi.Core.Contracts;
using FundaApi.Core.Models;
using FundaApi.LiteDb.Models;
using FundaApi.LiteDb.Options;
using LiteDB;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FundaApi.LiteDb.Tests;
public class LiteDbProviderTests
{
    private readonly Mock<IBrokerApi> _apiMock;
    private readonly Mock<ILiteDatabase> _databaseMock;
    private readonly Mock<ILiteCollection<AggregateResult<IReadOnlyList<RealEstateAgentWithCount>>>> _collectionMock;
    private readonly Mock<ILogger<LiteDbProvider>> _loggerMock;
    private readonly LiteDbProvider _provider;

    public LiteDbProviderTests()
    {
        _apiMock = new Mock<IBrokerApi>();
        _databaseMock = new Mock<ILiteDatabase>();
        _collectionMock = new Mock<ILiteCollection<AggregateResult<IReadOnlyList<RealEstateAgentWithCount>>>>();
        _loggerMock = new Mock<ILogger<LiteDbProvider>>();

        var options = Microsoft.Extensions.Options.Options.Create(new LiteDbOptions()
        {
            Collection = "test",
            ConnectionString = "test.db"
        });

        _provider = new LiteDbProvider(_databaseMock.Object, _apiMock.Object, _loggerMock.Object, options);
    }

    [Fact]
    public async Task ShouldReturnAgentsFromCache()
    {
        // Arrange
        var expected = new List<RealEstateAgentWithCount>() { new RealEstateAgentWithCount(1, "name", 1) };
        var aggregate = new AggregateResult<IReadOnlyList<RealEstateAgentWithCount>>
        {
            Id = "amsterdam.",
            Data = expected
        };

        _databaseMock
            .Setup(s => s.GetCollection<AggregateResult<IReadOnlyList<RealEstateAgentWithCount>>>(It.IsAny<string>(), It.IsAny<BsonAutoId>()))
            .Returns(_collectionMock.Object);

        _collectionMock
            .Setup(s => s.FindOne(s => s.Id == "amsterdam."))
            .Returns(aggregate);

        // Act
        var result = await _provider.GetRealEstateAgents("amsterdam", null, 10);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task ShouldReturnAgentsFromApi()
    {

        // Arrange
        var expected = new List<RealEstateAgentWithCount>() { new RealEstateAgentWithCount(1, "name", 1) };
        var aggregate = new AggregateResult<IReadOnlyList<RealEstateAgentWithCount>>
        {
            Id = "amsterdam.",
            Data = expected
        };

        _databaseMock
            .Setup(s => s.GetCollection<AggregateResult<IReadOnlyList<RealEstateAgentWithCount>>>(It.IsAny<string>(), It.IsAny<BsonAutoId>()))
            .Returns(_collectionMock.Object);

        _collectionMock
            .Setup(s => s.FindOne(s => s.Id == "amsterdam."))
            .Returns(() => null);

        _apiMock
            .Setup(s => s.GetRealEstateAgents(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _provider.GetRealEstateAgents("amsterdam", null, 10);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}
