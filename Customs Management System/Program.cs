using Customs_Management_System.DbContexts;
using Customs_Management_System.DependencyContainer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



// DbContext register

builder.Services.AddDbContext<CMSDbContext>();

// Register repository through Dependency Inversion 

DependencyInversion.RegisterServices(builder.Services);



//service for Frontend connection 
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://127.0.0.1:5501", "http://localhost:5501")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
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

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();

app.UseStaticFiles(); // for image file 
app.UseAuthentication(); // add authentication middleware 
app.UseAuthorization();
app.MapControllers();

app.Run();
