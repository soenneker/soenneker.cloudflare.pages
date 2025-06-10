using Microsoft.Extensions.Configuration;
using Soenneker.Cloudflare.Pages.Abstract;
using Soenneker.Facts.Local;
using Soenneker.Tests.FixturedUnit;
using System.Threading.Tasks;
using Xunit;

namespace Soenneker.Cloudflare.Pages.Tests;

[Collection("Collection")]
public sealed class CloudflarePagesUtilTests : FixturedUnitTest
{
    private readonly ICloudflarePagesUtil _util;
    private readonly IConfiguration _config;

    private const string TestProjectName = "test-project-leadping-gh";

    public CloudflarePagesUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<ICloudflarePagesUtil>(true);
        _config = Resolve<IConfiguration>();
    }

    [Fact]
    public void Default()
    {

    }

    [LocalFact]
    public async ValueTask Create()
    { 
        string? accountId = _config["Cloudflare:AccountId"];


    }

    [LocalFact]
    public async ValueTask ConnectToGitHub()
    {
        string? accountId = _config["Cloudflare:AccountId"];
       
    }

    [LocalFact]
    public async ValueTask CreateDeployment()
    {
        string? accountId = _config["Cloudflare:AccountId"];

        await _util.CreateDeployment(accountId, TestProjectName, "main", CancellationToken);
    }

    [LocalFact]
    public async ValueTask RemoveCustomDomain()
    {
        string? accountId = _config["Cloudflare:AccountId"];

    }
}
