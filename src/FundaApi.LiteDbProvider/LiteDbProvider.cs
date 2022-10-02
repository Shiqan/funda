using FundaApi.Core.Contracts;
using FundaApi.Core.Models;
using FundaApi.LiteDb.Models;
using FundaApi.LiteDb.Options;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FundaApi.LiteDb;

public class LiteDbProvider : IBrokerDataProvider
{
    private readonly ILogger<LiteDbProvider> _logger;
    private readonly LiteDbOptions _options;
    private readonly IBrokerApi _api;
    private readonly ILiteDatabase _liteDatabase;

    public LiteDbProvider(ILiteDatabase liteDatabase, IBrokerApi api, ILogger<LiteDbProvider> logger, IOptions<LiteDbOptions> options)
    {
        _liteDatabase = liteDatabase;
        _api = api;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<RealEstateAgentWithCount>> GetRealEstateAgents(string location, string? outdoorspace, int? size, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{location}.{outdoorspace}".ToLower();
        _logger.LogDebug("Retrieving top {n} real estate agents for {location}", size, cacheKey);

        var collection = _liteDatabase.GetCollection<AggregateResult<IReadOnlyList<RealEstateAgentWithCount>>>(_options.Collection);

        var result = collection.FindOne(doc => doc.Id == cacheKey)?.Data;

        if (result is null)
        {
            _logger.LogInformation("{location} not found in cache, retrieving data from downstream.", cacheKey);

            result = await _api.GetRealEstateAgents(location, outdoorspace, cancellationToken);
            collection.Insert(cacheKey, new AggregateResult<IReadOnlyList<RealEstateAgentWithCount>> { Id = cacheKey, Data = result });
        }
        else
        {
            _logger.LogInformation("{location} was found in cache.", cacheKey);
        }

        return result
            .Take(size ?? 10)
            .ToList()
            .AsReadOnly();
    }
}
