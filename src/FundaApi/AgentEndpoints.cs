using FundaApi.Core.Contracts;
using FundaApi.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FundaApi;

public static class AgentEndpoints
{
    public static void AddEndpoints(this WebApplication endpoints)
    {
        endpoints.MapGet("/agents/{location}/", async (string location, int? size, [FromServices] IBrokerDataProvider api, CancellationToken cancellationToken) => await Request(location, null, size, api, cancellationToken))
            .WithName("GetAgents")
            .WithMetadata(new SwaggerOperationAttribute(summary: "Get the top count Real Estate Agents and the number of listings they have in the specified location."))
            .Produces<IReadOnlyList<RealEstateAgentWithCount>>(200)
            .ProducesProblem(404);

        endpoints.MapGet("/agents/{location}/{outdoorspace}", async (string location, string? outdoorspace, int? size, [FromServices] IBrokerDataProvider api, CancellationToken cancellationToken) => await Request(location, outdoorspace, size, api, cancellationToken))
            .WithName("GetAgentsWithOutdoorspace")
            .WithMetadata(new SwaggerOperationAttribute(summary: "Get the top count Real Estate Agents and the number of listings with a certain outdoorspace they have in the specified location."))
            .Produces<IReadOnlyList<RealEstateAgentWithCount>>(200)
            .ProducesProblem(404);
    }

    private static async Task<IResult> Request(string location, string? outdoorspace, int? size, [FromServices] IBrokerDataProvider api, CancellationToken cancellationToken)
    {
        var data = await api.GetRealEstateAgents(location, outdoorspace, size, cancellationToken: cancellationToken);
        if (data is null || !data.Any())
        {
            return Results.NotFound();
        }

        return Results.Ok(data);
    }
}
