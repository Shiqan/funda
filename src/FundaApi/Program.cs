using FundaApi;
using FundaApi.Core.Extensions;
using FundaApi.Core.Options;
using FundaApi.LiteDb.Extensions;
using FundaApi.LiteDb.Options;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFundaApi(builder.Configuration.GetSection(nameof(FundaApiOptions)).Bind);
builder.Services.UseLiteDb(builder.Configuration.GetSection(nameof(LiteDbOptions)).Bind);

builder.Services.AddCors(setup =>
{
    setup.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.AddEndpoints();

app.Run();
