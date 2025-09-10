using System.Security.Claims;
using Core.Application.Constants;
using Core.Application.Options;
using Core.Domain.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Core.API.Extensions.DI;

public static class IdentityConfigureExtension
{
    public static IServiceCollection AddAuthJwt(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {

        // serviceCollection.Configure<IdentityAuthOptions>(configAuth);

        serviceCollection
            .AddOptions<IdentityAuthOptions>()
            .Bind(configuration.GetSection(nameof(IdentityAuthOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var configAuth = configuration.GetSection(nameof(IdentityAuthOptions));
        var authOptions = configAuth.Get<IdentityAuthOptions>();

        // serviceCollection
        //     .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //     .AddJwtBearer();

       // serviceCollection.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
       serviceCollection
           .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer((options) =>
            {

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,

                        ValidateAudience = true,

                        ValidateLifetime = true,
                        //https://stackoverflow.com/questions/52379848/asp-net-core-jwt-authentication-allows-expired-tokens
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,

                        //https://stackoverflow.com/questions/55285811/jwt-auth-works-with-authorize-attribute-but-not-with-authorize-policy-admin
                        //  RoleClaimType = AuthConstant.RoleClaim
                    };

            });

       // serviceCollection.AddTransient<IConfigureOptions<JwtBearerOptions>, JwtConfigureOptions>();
        
        serviceCollection.AddAuthorization(options =>
        {

            options.AddPolicy(IdentityAuthConstants.AuthPolicyModerator,
                policy =>
                {
                    policy.RequireClaim(ClaimTypes.Role, new[] {RoleConstants.Moderator});
                 });

            //
            //
            // options.AddPolicy(IdentityAuthConstants.AuthPolicyCompany,
            //     policy =>
            //     {
            //         policy.RequireClaim(ClaimTypes.Role, new[] {RoleConstants.Company});
            //     });
            //
        });

        var origins = configuration.GetSection("Cors:Origins")
            .Get<string[]>()??throw new ArgumentNullException("Cors:Origins");
        serviceCollection.AddCors(options =>
        {

            options.AddPolicy(IdentityAuthConstants.CorsPolicyName,
                builder =>
                {
                    // builder.AllowAnyHeader();
                    // builder.AllowAnyMethod();
                    // builder.SetIsOriginAllowed(host => true);
                    // builder.AllowCredentials();
                    builder.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });
       

        return serviceCollection;
    }

    public static IApplicationBuilder UseAuthJwt(this WebApplication app)
    {
        app.UseCors(IdentityAuthConstants.CorsPolicyName);
        app.UseAuthentication();
        app.UseAuthorization();
        //  app.UseAntiforgery();
        return app;
    }
}