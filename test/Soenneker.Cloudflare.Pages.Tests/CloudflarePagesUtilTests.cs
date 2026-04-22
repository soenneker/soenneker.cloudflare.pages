using Microsoft.Extensions.Configuration;
using Soenneker.Cloudflare.Pages.Abstract;
using Soenneker.Tests.Attributes.Local;
using Soenneker.Tests.HostedUnit;
using System.Threading.Tasks;
using Soenneker.Facts.Manual;

namespace Soenneker.Cloudflare.Pages.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class CloudflarePagesUtilTests : HostedUnitTest
{
    private readonly ICloudflarePagesUtil _util;
    private readonly IConfiguration _config;

    private const string TestProjectName = "test-project-leadping-gh";

    public CloudflarePagesUtilTests(Host host) : base(host)
    {
        _util = Resolve<ICloudflarePagesUtil>(true);
        _config = Resolve<IConfiguration>();
    }

    [Test]
    public void Default()
    {
    }

    [ManualFact]
    // [LocalOnly]
    public async ValueTask Create()
    {
        string? accountId = _config["Cloudflare:AccountId"];
    }

    [ManualFact]
    // [LocalOnly]
    public async ValueTask ConnectToGitHub()
    {
        string? accountId = _config["Cloudflare:AccountId"];
    }

    [ManualFact]
    // [LocalOnly]
    public async ValueTask CreateDeployment()
    {
        string? accountId = _config["Cloudflare:AccountId"];

        await _util.CreateDeployment(accountId, TestProjectName, "main", CancellationToken);
    }

    [ManualFact]
    // [LocalOnly]
    public async ValueTask RemoveCustomDomain()
    {
        string? accountId = _config["Cloudflare:AccountId"];
    }
}