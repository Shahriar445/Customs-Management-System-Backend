using Customs_Management_System.DbContexts;
using Customs_Management_System.DependencyContainer;
using Customs_Management_System.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register DbContext
builder.Services.AddDbContext<CMSDbContext>();
builder.Services.AddScoped<EmailService>();

// Configure JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // Use "Jwt" to match appsettings.json
            ValidAudience = builder.Configuration["Jwt:Audience"], // Use "Jwt" to match appsettings.json
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])), // Use "Jwt" to match appsettings.json
            ClockSkew = TimeSpan.Zero // Optional: reduce token expiration tolerance
        };
    });

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Admin"));
});


var app = builder.Build();


// Add logging
builder.Logging.AddConsole();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseDeveloperExceptionPage();  // Add this for development purposes

app.UseCors("Open");

app.UseHttpsRedirection();
app.UseStaticFiles(); // For serving static files such as images

app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();

app.MapControllers();

app.Run();
