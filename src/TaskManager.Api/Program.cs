using TaskManager.Api;
using TaskManager.Api.Authentication;
using TaskManager.Application.Users;
using TaskManager.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<RegisterUser>();
builder.Services.AddScoped<LoginUser>();

var connectionString = builder.Configuration.GetConnectionString("TaskManager")
    ?? throw new InvalidOperationException("Connection string 'TaskManager' is required.");
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddTaskManagerJwt(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();
await DatabaseInitializer.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseCors("AngularDev");
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program
{
}
