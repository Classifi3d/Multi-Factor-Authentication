using AuthenticationWebApplication.Context;
using MFAWebApplication.Context;
using Microsoft.EntityFrameworkCore;

namespace MFAWebApplication.Extensions;

public static class ExternalConnectionsExtensions
{
    public static IHostApplicationBuilder AddExternalServices(this IHostApplicationBuilder builder)
    {
        // Write PostgreSQL DB
        builder.Services.AddDbContext<WriteDbContext>(
            options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("PostgreSQL_Write_Connection_String")
                )
        );

        // Read PostgreSQL DB
        builder.Services.AddDbContext<ReadDbContext>(
            options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("PostgreSQL_Read_Connection_String")
                )
        );

        return builder;

    }
}

