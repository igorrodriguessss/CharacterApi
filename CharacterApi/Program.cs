using CharacterApi.Data;
using CharacterApi.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<ICharacterService, CharacterService>();
var app = builder.Build();


// Configure the HTTP request pipeline.

app.MapOpenApi();
app.MapScalarApiReference();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
