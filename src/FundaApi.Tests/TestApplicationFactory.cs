using System;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FundaApi.Tests;

internal class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Action<IServiceCollection>? _configureServices;

    public TestApplicationFactory(Action<IServiceCollection>? configureServices = null)
    {
        _configureServices = configureServices;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        if (_configureServices is not null)
        {
            builder.ConfigureServices(_configureServices);
        }

        return base.CreateHost(builder);
    }
}
