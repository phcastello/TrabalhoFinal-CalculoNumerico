using NumericalMethods.Core.Services;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "FrontendPolicy";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    var allowedOrigins = new[]
    {
        "http://localhost:5173",
        "http://localhost:4173"
    };

    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<ILinearSystemSolverService, LinearSystemSolverService>();
builder.Services.AddScoped<IRootFindingService, RootFindingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors(CorsPolicyName);

app.Use(async (context, next) =>
{
    await next();

    if (string.IsNullOrEmpty(context.Response.ContentType))
    {
        return;
    }

    if (context.Response.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) &&
        !context.Response.ContentType.Contains("charset=", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.ContentType = "application/json; charset=utf-8";
    }
});

app.MapControllers();

app.Run();
