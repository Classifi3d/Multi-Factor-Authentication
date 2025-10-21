using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace MFAWebApplication.Configurations;

public static class RateLimiterConfiguration
{
    public static void AddCustomRateLimiters( this IServiceCollection services )
    {
        services.AddRateLimiter(options =>
        {
            // Registration limiter
            options.AddFixedWindowLimiter("registerLimiter", opt =>
            {
                opt.PermitLimit = 5; // Max 5 registrations per minute
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2; // Allows 2 extra attempts to queue
            });

            // Login limiter
            options.AddTokenBucketLimiter("loginLimiter", opt =>
            {
                opt.TokenLimit = 10;  // Max 10 login attempts at any time
                opt.TokensPerPeriod = 2; // Refill 2 tokens every 10 seconds
                opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
                opt.AutoReplenishment = true;
                opt.QueueLimit = 3;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // MFA limiter
            options.AddSlidingWindowLimiter("mfaLimiter", opt =>
            {
                opt.PermitLimit = 3; // Max 3 MFA attempts within 30 seconds
                opt.Window = TimeSpan.FromSeconds(30);
                opt.SegmentsPerWindow = 3; // MFA attempts spread over 3 segments (10 sec each)
                opt.QueueLimit = 1; // Only 1 additional attempt in queue
            });

            // Global limiter
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter("globalLimiter", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100, // Max 100 requests per minute per user
                    Window = TimeSpan.FromMinutes(1)
                })
            );
        });
    }
}
