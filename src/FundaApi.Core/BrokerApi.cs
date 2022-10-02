using System.Net.Http.Json;
using FundaApi.Core.Contracts;
using FundaApi.Core.Models;
using FundaApi.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FundaApi.Core;

public class BrokerApi : IBrokerApi
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BrokerApi> _logger;
    private readonly FundaApiOptions _options;

    private const string _baseUrl = "https://partnerapi.funda.nl/feeds/Aanbod.svc/search/json/{0}/?website=funda&type=koop&zo=/{1}/&page={2}&pagesize={3}";

    public BrokerApi(HttpClient httpClient, IOptions<FundaApiOptions> options, ILogger<BrokerApi> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<RealEstateAgentWithCount>> GetRealEstateAgents(string location, string? outdoorspace, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving real estate agents for {location}", location);
        var result = new List<RealEstateAgent>();

        var searchQuery = string.IsNullOrEmpty(outdoorspace) ? location : $"{location}/{outdoorspace}";

        var response = await GetPage(searchQuery, 1, cancellationToken);
        if (response is null || response.TotaalAantalObjecten == 0)
        {
            return Enumerable.Empty<RealEstateAgentWithCount>().ToList();
        }

        result.AddRange(response.Objects);

        _logger.LogDebug("Found {total} and {count} pages to fetch", response.TotaalAantalObjecten, response.Paging.AantalPaginas);

        var requests = Enumerable.Range(2, response.Paging.AantalPaginas - 1)
            .Select(page => GetPage(searchQuery, page, cancellationToken));

        var responses = await Task.WhenAll(requests);
        result.AddRange(responses
            .SelectMany(response => response?.Objects ?? Enumerable.Empty<RealEstateAgent>()));

        return result
            .GroupBy(agent => agent)
            .Select(group => new RealEstateAgentWithCount(group.Key.MakelaarId, group.Key.MakelaarNaam, group.Count()))
            .OrderByDescending(agent => agent.Count)
            .ToList();
    }

    private async Task<ApiResponse<RealEstateAgent>?> GetPage(string query, int page, CancellationToken cancellationToken)
    {
        var url = string.Format(_baseUrl, _options.ApiKey, query, page, _options.PageSize);
        _logger.LogDebug("Getting {page} from {url}", page, url);

        return await _httpClient.GetFromJsonAsync<ApiResponse<RealEstateAgent>>(url, cancellationToken);
    }
}
