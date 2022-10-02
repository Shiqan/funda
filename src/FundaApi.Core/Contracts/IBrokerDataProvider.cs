using FundaApi.Core.Models;

namespace FundaApi.Core.Contracts;

public interface IBrokerDataProvider
{
    Task<IReadOnlyList<RealEstateAgentWithCount>> GetRealEstateAgents(string location, string? outdoorspace = null, int? size = 10, CancellationToken cancellationToken = default);
}
