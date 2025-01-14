using Autofac;
using Autofac.Extensions.DependencyInjection;
using DotnetWorkshop.API.MiddleWares;
using DotnetWorkshop.Core.Repositories;
using DotnetWorkshop.Core.Services;
using DotnetWorkshop.Core.UnitOfWorks;
using DotnetWorkshop.Repository;
using DotnetWorkshop.Repository.Repositories;
using DotnetWorkshop.Repository.UnitOfWorks;
using DotnetWorkshop.Service.Authorization.Abstract;
using DotnetWorkshop.Service.Authorization.Concrete;
using DotnetWorkshop.Service.Mapping;
using DotnetWorkshop.Service.Services;
using DotnetWorkshop.Service.Validations;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

#region Swagger İşlemleri
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

#endregion


//Automapper kütüphanesinin tanımlanması
builder.Services.AddAutoMapper(typeof(MapProfile));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IJwtAuthenticationManager, JwtAuthenticationManager>();
//Fluent Validation Tanımlamasının gerçekleştirilmesi.
//1. Yöntem
//builder.Services.AddControllers()
//    .AddFluentValidation(x =>
//    {
//        x.RegisterValidatorsFromAssemblyContaining<TeamDtoValidator>();
//        x.RegisterValidatorsFromAssemblyContaining<UserDtoValidator>();
//        x.RegisterValidatorsFromAssemblyContaining<UserProfileDtoValidator>();
//    });

//2. yöntem
builder.Services.AddControllers().AddFluentValidation(x => { x.RegisterValidatorsFromAssemblyContaining<TeamDtoValidator>(); });



//AppDbContext ile ilgili işlemler
builder.Services.AddDbContext<AppDbContext>(x =>
  x.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"), option =>
  {
      option.MigrationsAssembly(Assembly.GetAssembly(typeof(AppDbContext)).GetName().Name);
  })
);


//Autofac kütüphanesini ekledik. Bu kütüphane aracılığıyla (Generic Repository,service vb) işlemleri otomatik olarak program.cs'de çalıştırsın.
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Buranın devamı (API KATMANINDA.)
//builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder => containerBuilder.RegisterModule(new RepoModuleService()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCustomException();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<JwtMiddleware>();
app.MapControllers();

app.Run();
