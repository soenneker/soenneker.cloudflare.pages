using Soenneker.Cloudflare.Pages.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Cloudflare.Pages.Tests;

[Collection("Collection")]
public sealed class CloudflarePagesUtilTests : FixturedUnitTest
{
    private readonly ICloudflarePagesUtil _util;

    public CloudflarePagesUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<ICloudflarePagesUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
