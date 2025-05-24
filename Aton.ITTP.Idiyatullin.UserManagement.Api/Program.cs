using Aton.ITTP.Idiyatullin.UserManagement.Api.Constants;
using Aton.ITTP.Idiyatullin.UserManagement.Api.Data;
using Aton.ITTP.Idiyatullin.UserManagement.Api.Entity;
using Aton.ITTP.Idiyatullin.UserManagement.Api.Helpers;
using Aton.ITTP.Idiyatullin.UserManagement.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<UserManagementDbContext>(options =>
    options.UseInMemoryDatabase("UserManagementDb"));

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "User Management API",
        Description = "API для управления пользователями (стажерское задание)",
        Contact = new OpenApiContact
        {
            Name = "Булат Идиятуллин",
            Email = "bulat.ya2003@gmail.com"
        }
    });

    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

    options.AddSecurityDefinition("ActorLoginHeader", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "X-Acting-User-Login",
        Type = SecuritySchemeType.ApiKey,
        Description = "Логин пользователя, от имени которого выполняется действие (введите admin)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ActorLoginHeader"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<UserManagementDbContext>();
    var config = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    context.Database.EnsureCreated();

    var adminLogin = config[AppConstants.DefaultAdminLoginConfigKey] ?? "admin";

    if (!await context.Users.AnyAsync(u => u.Login == adminLogin))
    {
        var adminPassword = config[AppConstants.DefaultAdminPasswordConfigKey] ?? "admin123";
        var adminName = config[AppConstants.DefaultAdminNameConfigKey] ?? "Default Admin";

        context.Users.Add(new User
        {
            Guid = Guid.NewGuid(),
            Login = adminLogin,
            PasswordHash = PasswordHasher.HashPassword(adminPassword),
            Name = adminName,
            Gender = Gender.Unknown,
            Admin = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = AppConstants.SystemUserLogin
        });

        await context.SaveChangesAsync();

        logger.LogInformation("Initial admin user '{AdminLogin}' created.", adminLogin);
    }
    else
    {
        logger.LogInformation("Initial admin user '{AdminLogin}' already exists.", adminLogin);
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API V1");
        c.DefaultModelsExpandDepth(-1);
    });
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();