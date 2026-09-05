using backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevCorsPolicy = "AngularDev";


builder.Services.AddDbContext<CrochetFileShelfDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("CrochetFileShelfDb")));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseCors(AngularDevCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
