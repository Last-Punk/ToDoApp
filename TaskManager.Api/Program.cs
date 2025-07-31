using TaskManager.Data;
using TaskManager.Domain.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using TaskManager.API.Validators;
using TaskManager.Api.Middlewares;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using TaskManager.Data.Models;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<Database>();

builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<Database>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapIdentityApi<User>();

app.UseAuthorization();

app.MapControllers();

app.Run();
