using Customs_Management_System.DbContexts;
using Customs_Management_System.DependencyContainer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register DbContext
builder.Services.AddDbContext<CMSDbContext>();

// Register repositories and services through Dependency Injection
DependencyInversion.RegisterServices(builder.Services);

// Configure CORS to allow all origins, headers, and methods
builder.Services.AddCors(options =>
{
    options.AddPolicy("Open", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add logging
builder.Logging.AddConsole();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Open");

app.UseHttpsRedirection();
app.UseStaticFiles(); // For serving static files such as images

app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();

app.MapControllers();

app.Run();
