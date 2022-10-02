using FundaApi.Core.Contracts;
using FundaApi.Core.Extensions;
using FundaApi.Core.Models;
using FundaApi.Core.Options;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFundaApi(builder.Configuration.GetSection(nameof(FundaApiOptions)).Bind);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/agents/{location}/", async (string location, [FromServices] IBrokerApi api, CancellationToken cancellationToken) =>
{
    var data = await api.GetRealEstateAgents(location, null, cancellationToken: cancellationToken);
    if (data is null || !data.Any())
    {
        return Results.NotFound();
    }

    return Results.Ok(data);
})
.WithName("GetAgents")
.WithMetadata(new SwaggerOperationAttribute(summary: "Get the top count Real Estate Agents and the number of listings they have in the specified location."))
.Produces<IReadOnlyList<RealEstateAgentWithCount>>(200)
.ProducesProblem(404);

app.MapGet("/agents/{location}/{outdoorspace}", async (string location, string? outdoorspace, [FromServices] IBrokerApi api, CancellationToken cancellationToken) =>
{
    var data = await api.GetRealEstateAgents(location, outdoorspace, cancellationToken: cancellationToken);
    if (data is null || !data.Any())
    {
        return Results.NotFound();
    }

    return Results.Ok(data);
})
.WithName("GetAgentsWithOutdoorspace")
.WithMetadata(new SwaggerOperationAttribute(summary: "Get the top count Real Estate Agents and the number of listings with a certain outdoorspace they have in the specified location."))
.Produces<IReadOnlyList<RealEstateAgentWithCount>>(200)
.ProducesProblem(404);

app.Run();
