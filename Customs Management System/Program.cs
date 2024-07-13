using Customs_Management_System.DbContexts;
using Customs_Management_System.DependencyContainer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// DbContext register
builder.Services.AddDbContext<CMSDbContext>();

// Register repository through Dependency Inversion
DependencyInversion.RegisterServices(builder.Services);

// Use CORS for all IPs
builder.Services.AddCors(options =>
{
    options.AddPolicy("Open", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Open");
app.UseStaticFiles(); // For serving static files such as images
app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization(); // Add authorization middleware

app.MapControllers();

app.Run();
