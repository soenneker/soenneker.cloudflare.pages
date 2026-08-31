[![](https://img.shields.io/nuget/v/soenneker.cloudflare.pages.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.cloudflare.pages/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.cloudflare.pages/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.cloudflare.pages/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.cloudflare.pages.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.cloudflare.pages/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.cloudflare.pages/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.cloudflare.pages/actions/workflows/codeql.yml)

# Soenneker.Cloudflare.Pages

Manages Cloudflare Pages projects, GitHub source configuration, deployments, and custom-domain DNS records.

## Installation

```bash
dotnet add package Soenneker.Cloudflare.Pages
```

## Configuration

```json
{
  "Cloudflare": {
    "ApiKey": "your-api-token"
  }
}
```

The token needs Pages permissions for the account and DNS/zone-read permissions when custom-domain helpers are used.

## Registration

```csharp
using Soenneker.Cloudflare.Pages.Registrars;

services.AddCloudflarePagesUtilAsScoped();
```

Registration also adds the shared Cloudflare client, DNS-record utility, and zone utility. Singleton registration is available.

## Projects

```csharp
PagesProject project = await pages.CreateWithGitHub(
    accountId,
    projectName: "docs",
    repoOwner: "example",
    repoName: "docs-site",
    productionBranch: "main",
    buildCommand: "npm run build",
    buildOutputDir: "dist",
    cancellationToken: cancellationToken);
```

`List` follows every Cloudflare result page. `Get`, `Update`, `UpdateGitHubConfig`, and `GetGitHubConfig` operate on one named project. `CreateDeployment` requests a deployment for a branch and returns Cloudflare's generated deployment model.

## Custom domains

```csharp
PagesDomain domain = await pages.AddCustomDomain(
    accountId,
    projectName: "docs",
    zoneDomain: "example.com",
    customDomain: "docs.example.com",
    cancellationToken);
```

`AddCustomDomain` first attaches the domain to Pages, resolves the Cloudflare zone, and then creates a proxied CNAME to `<project>.pages.dev`. `RemoveCustomDomain` removes the Pages association and then removes the matching CNAME. The `zoneDomain` argument is the managed zone used to resolve the zone ID; it is not necessarily identical to the custom hostname.

These are multi-step remote operations, not transactions. If a later Pages or DNS call fails, an earlier change may remain applied. Catch propagated API exceptions and reconcile both the Pages domain and DNS record before retrying.

## Deletion

`Delete(accountId, projectName, zoneDomain)` removes every returned custom domain and its DNS record before deleting the project. This is destructive and can leave partial state if any remote step fails; use it only when project and DNS removal are both intended.
