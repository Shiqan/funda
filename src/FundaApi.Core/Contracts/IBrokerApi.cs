using FundaApi.Core.Models;

namespace FundaApi.Core.Contracts;

public interface IBrokerApi
{
    Task<IReadOnlyList<RealEstateAgentWithCount>> GetRealEstateAgents(string location, string? outdoorspace = null, CancellationToken cancellationToken = default);
}
