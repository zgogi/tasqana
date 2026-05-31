using Microsoft.EntityFrameworkCore;
using System;
using WebApi.Clients;
using WebApi.Extensions;
using WebApi.Models;
using WebApi.Repositories;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TaskanaDb>();

builder.Services.AddScoped<TelegramClient>();

builder.Services.AddScoped<UsersRepository>();
builder.Services.AddScoped<SessionsRepository>();
builder.Services.AddScoped<CategoriesRepository>();
builder.Services.AddScoped<TodosRepository>();
builder.Services.AddScoped<TelegramRepository>();

builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<SessionsService>();
builder.Services.AddScoped<CategoriesService>();
builder.Services.AddScoped<TodosService>();
builder.Services.AddScoped<TelegramService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Добавляем наш кастомный конвертер в общие настройки сериализации
        options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
    });

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Use to create migrations
        // dotnet ef migrations add <MigrationName>
        var context = services.GetRequiredService<TaskanaDb>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error on migration.");
        throw; 
    }
}

app.Run();
