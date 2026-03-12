using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Cloudflare.DnsRecords.Registrars;
using Soenneker.Cloudflare.Pages.Abstract;
using Soenneker.Cloudflare.Utils.Client.Registrars;
using Soenneker.Cloudflare.Zones.Registrars;

namespace Soenneker.Cloudflare.Pages.Registrars;

/// <summary>
/// A utility for managing Cloudflare Pages
/// </summary>
public static class CloudflarePagesUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="ICloudflarePagesUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddCloudflarePagesUtilAsSingleton(this IServiceCollection services)
    {
        services.AddCloudflareClientUtilAsSingleton().AddCloudflareDnsRecordsUtilAsSingleton().AddCloudflareZonesUtilAsSingleton().TryAddSingleton<ICloudflarePagesUtil, CloudflarePagesUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="ICloudflarePagesUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddCloudflarePagesUtilAsScoped(this IServiceCollection services)
    {
        services.AddCloudflareClientUtilAsSingleton().AddCloudflareDnsRecordsUtilAsScoped().AddCloudflareZonesUtilAsScoped().TryAddScoped<ICloudflarePagesUtil, CloudflarePagesUtil>();

        return services;
    }
}