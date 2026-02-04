using EmployeeContactApi.Application.Commands.Handlers;
using EmployeeContactApi.Application.Queries.Handlers;
using EmployeeContactApi.Application.Services;
using EmployeeContactApi.Infrastructure.Persistence;
using EmployeeContactApi.Presentation.Middleware;
using EmployeeContactApi.Presentation.Swagger;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.InputFormatters.Insert(0, new EmployeeContactApi.Presentation.Middleware.TextPlainInputFormatter());
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Handle conflicting actions (multiple POST methods with same route)
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    // Merge multiple POST operations into one with multiple content types
    options.DocumentFilter<MergePostOperationsFilter>();
});

// Configure EF Core with In-Memory Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("EmployeeDb"));

// Register repositories (CQRS pattern)
builder.Services.AddScoped<IEmployeeCommandRepository, EmployeeCommandRepository>();
builder.Services.AddScoped<IEmployeeQueryRepository, EmployeeQueryRepository>();

// Register command/query handlers
builder.Services.AddScoped<ICreateEmployeeCommandHandler, CreateEmployeeCommandHandler>();
builder.Services.AddScoped<IEmployeeQueryHandler, EmployeeQueryHandler>();

// Register parsers
builder.Services.AddScoped<IEmployeeParser, CsvParser>();
builder.Services.AddScoped<IEmployeeParser, JsonParser>();
builder.Services.AddScoped<EmployeeParserService>();

// Register exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Contact API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseSerilogRequestLogging();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

Log.Information("Employee Contact API started");

app.Run();
