using NumericalMethods.Core.Services;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "FrontendPolicy";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
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
app.MapControllers();

app.Run();
