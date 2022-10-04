using FundaApi.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services
    .AddHttpClient<IFundaApiClient, FundaApiClient>()
    .ConfigureHttpClient((httpClient) =>
    {
        httpClient.BaseAddress = new Uri(builder.Configuration["FundaApi:BaseUrl"]);
    });

await builder.Build().RunAsync();
