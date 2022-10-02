using FundaApi.Core.Contracts;
using FundaApi.LiteDb.Options;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;

namespace FundaApi.LiteDb.Extensions;

public static class ServiceCollectionExtensions
{
    public static void UseLiteDb(this IServiceCollection services, Action<LiteDbOptions> options)
    {
        var liteDbOptions = new LiteDbOptions();
        options.Invoke(liteDbOptions);

        services.Configure(options);

        services.AddSingleton<ILiteDatabase>(new LiteDatabase(liteDbOptions.ConnectionString));
        services.AddTransient<IBrokerDataProvider, LiteDbProvider>();
    }
}
