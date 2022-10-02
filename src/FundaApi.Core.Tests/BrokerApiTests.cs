using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FundaApi.Core.Options;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace FundaApi.Core.Tests;
public class BrokerApiTests
{
    private readonly Mock<ILogger<BrokerApi>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly BrokerApi _service;

    public BrokerApiTests()
    {
        _loggerMock = new Mock<ILogger<BrokerApi>>();
        _handlerMock = new Mock<HttpMessageHandler>();
        var options = Microsoft.Extensions.Options.Options.Create(new FundaApiOptions()
        {
            ApiKey = "abc",
            PageSize = 100,
        });

        var httpClient = new HttpClient(_handlerMock.Object);
        _service = new BrokerApi(httpClient, options, _loggerMock.Object);
    }

    [Theory]
    [InlineData("amsterdam", null, "/amsterdam/")]
    [InlineData("amsterdam", "balkon", "/amsterdam/balkon/")]
    [InlineData("amsterdam", "tuin", "/amsterdam/tuin/")]
    [InlineData("amsterdam", "dakterras", "/amsterdam/dakterras/")]
    [InlineData("den-haag", null, "/den-haag/")]
    [InlineData("den-haag", "balkon", "/den-haag/balkon/")]
    public async Task ShouldCallApi_WithCorrectUrl(string location, string? outdoorspace, string expectedSearchQuery)
    {
        // Arrange
        _handlerMock.Protected()
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.Is<HttpRequestMessage>(e => e.RequestUri!.Query.Contains(expectedSearchQuery)),
              ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(new HttpResponseMessage()
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(@"
                    {
	                    ""TotaalAantalObjecten"": 1,
                        ""Paging"": {
                            ""AantalPaginas"": 1,
                            ""HuidigePagina"": 1
                        },
                        ""Objects"": [{
                            ""MakelaarId"": 1,
                            ""MakelaarNaam"": ""test""
                        }]
                    }")
           })
           .Verifiable();

        // Act
        var result = await _service.GetRealEstateAgents(location, outdoorspace);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result[0].Name.Should().Be("test");

        _handlerMock.Verify();
    }

    [Theory]
    [InlineData("amsterdam", null)]
    [InlineData("amsterdam", "balkon")]
    [InlineData("amsterdam", "tuin")]
    [InlineData("amsterdam", "dakterras")]
    [InlineData("den-haag", null)]
    [InlineData("den-haag", "balkon")]
    public async Task ShouldCallApi_WithMultiplePages(string location, string? outdoorspace)
    {
        // Arrange
        _handlerMock.Protected()
           .SetupSequence<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(new HttpResponseMessage()
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(@"
                    {
	                    ""TotaalAantalObjecten"": 2,
                        ""Paging"": {
                            ""AantalPaginas"": 2,
                            ""HuidigePagina"": 1
                        },
                        ""Objects"": [{
                            ""MakelaarId"": 1,
                            ""MakelaarNaam"": ""test""
                        }]
                    }")
           })
           .ReturnsAsync(new HttpResponseMessage()
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(@"
                    {
	                    ""TotaalAantalObjecten"": 2,
                        ""Paging"": {
                            ""AantalPaginas"": 2,
                            ""HuidigePagina"": 2
                        },
                        ""Objects"": [{
                            ""MakelaarId"": 2,
                            ""MakelaarNaam"": ""test-too""
                        }]
                    }")
           });

        // Act
        var result = await _service.GetRealEstateAgents(location, outdoorspace);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result[0].Name.Should().Be("test");
        result[1].Name.Should().Be("test-too");

        _handlerMock.Protected().Verify("SendAsync", Times.Exactly(2),
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>());
    }
}
