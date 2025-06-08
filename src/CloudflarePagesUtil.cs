using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Cloudflare.OpenApiClient;
using Soenneker.Cloudflare.OpenApiClient.Models;
using Soenneker.Cloudflare.Pages.Abstract;
using Soenneker.Cloudflare.Utils.Client.Abstract;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Cloudflare.Pages;

///<inheritdoc cref="ICloudflarePagesUtil"/>
public sealed class CloudflarePagesUtil : ICloudflarePagesUtil
{
    private readonly ICloudflareClientUtil _client;
    private readonly ILogger<CloudflarePagesUtil> _logger;

    public CloudflarePagesUtil(ICloudflareClientUtil client, ILogger<CloudflarePagesUtil> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async ValueTask<Pages_projectResponse> Create(string accountId, string name, string productionBranch, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating Pages project {Name} in account {AccountId}", name, accountId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();

        try
        {
            var project = new Pages_projectObject
            {
                Name = name,
                ProductionBranch = productionBranch
            };

            Pages_projectResponse? result = await client.Accounts[accountId].Pages.Projects.PostAsync(project, null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully created Pages project {Name}", name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Pages project {Name}", name);
            throw;
        }
    }

    public async ValueTask<Pages_projectResponse> Get(string accountId, string name, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting Pages project {Name} from account {AccountId}", name, accountId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();

        try
        {
            Pages_projectResponse? result = await client.Accounts[accountId].Pages.Projects[name].GetAsync(null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully retrieved Pages project {Name}", name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Pages project {Name}", name);
            throw;
        }
    }

    public async ValueTask<Pages_projectResponse> Update(string accountId, string name, string productionBranch, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating Pages project {Name} in account {AccountId}", name, accountId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var project = new Pages_projectPatch
            {
                ProductionBranch = productionBranch
            };

            Pages_projectResponse? result = await client.Accounts[accountId].Pages.Projects[name].PatchAsync(project, null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully updated Pages project {Name}", name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Pages project {Name}", name);
            throw;
        }
    }

    public async ValueTask Delete(string accountId, string name, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting Pages project {Name} from account {AccountId}", name, accountId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            await client.Accounts[accountId].Pages.Projects[name].DeleteAsync(null, null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully deleted Pages project {Name}", name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Pages project {Name}", name);
            throw;
        }
    }

    public async ValueTask<List<Pages_deployments>> List(string accountId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listing Pages projects in account {AccountId}", accountId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            Pages_projectsResponse? result = await client.Accounts[accountId].Pages.Projects.GetAsync(null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully listed Pages projects");
            return result.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list Pages projects");
            throw;
        }
    }

    public async ValueTask<Pages_domainResponseSingle> AddCustomDomain(string accountId, string projectName, string domainName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding custom domain {DomainName} to Pages project {ProjectName}", domainName, projectName);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var domain = new Pages_domainsPost
            {
                Name = domainName
            };

            Pages_domainResponseSingle? result =
                await client.Accounts[accountId].Pages.Projects[projectName].Domains.PostAsync(domain, null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully added custom domain {DomainName} to Pages project {ProjectName}", domainName, projectName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add custom domain {DomainName} to Pages project {ProjectName}", domainName, projectName);
            throw;
        }
    }

    public async ValueTask RemoveCustomDomain(string accountId, string projectName, string domainName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing custom domain {DomainName} from Pages project {ProjectName}", domainName, projectName);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var body = new Pages_domains_delete_domain_RequestBody_application_json();
            await client.Accounts[accountId].Pages.Projects[projectName].Domains[domainName].DeleteAsync(body, null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully removed custom domain {DomainName} from Pages project {ProjectName}", domainName, projectName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove custom domain {DomainName} from Pages project {ProjectName}", domainName, projectName);
            throw;
        }
    }

    public async ValueTask<IEnumerable<Pages_domainObject>> ListCustomDomains(string accountId, string projectName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listing custom domains for Pages project {ProjectName}", projectName);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var body = new object(); // Simplified request body since the specific type is not available
            Pages_domainResponseCollection? result =
                await client.Accounts[accountId].Pages.Projects[projectName].Domains.GetAsync(null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully listed custom domains for Pages project {ProjectName}", projectName);
            return result.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list custom domains for Pages project {ProjectName}", projectName);
            throw;
        }
    }

    public async ValueTask<Pages_projectResponse> ConnectToGitHub(string accountId, string projectName, string repoOwner, string repoName,
        string productionBranch, string? buildCommand = null, string? buildOutputDir = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Connecting Pages project {ProjectName} to GitHub repository {RepoOwner}/{RepoName}", projectName, repoOwner, repoName);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var project = new Pages_projectObject
            {
                Name = projectName,
                ProductionBranch = productionBranch,
                Source = new Pages_source
                {
                    Type = "github",
                    Config = new Pages_source_config
                    {
                        Owner = repoOwner,
                        RepoName = repoName,
                        ProductionBranch = productionBranch,
                        DeploymentsEnabled = true,
                        ProductionDeploymentsEnabled = true
                    }
                }
            };

            if (!string.IsNullOrEmpty(buildCommand) || !string.IsNullOrEmpty(buildOutputDir))
            {
                project.BuildConfig = new Pages_build_config
                {
                    BuildCommand = buildCommand,
                    DestinationDir = buildOutputDir
                };
            }

            Pages_projectResponse? result = await client.Accounts[accountId].Pages.Projects.PostAsync(project, null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully connected Pages project {ProjectName} to GitHub repository {RepoOwner}/{RepoName}", projectName, repoOwner,
                repoName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect Pages project {ProjectName} to GitHub repository {RepoOwner}/{RepoName}", projectName, repoOwner, repoName);
            throw;
        }
    }

    public async ValueTask<Pages_projectResponse> UpdateGitHubConfig(string accountId, string projectName, string repoOwner, string repoName,
        string productionBranch, string? buildCommand = null, string? buildOutputDir = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating GitHub configuration for Pages project {ProjectName}", projectName);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var project = new Pages_projectPatch
            {
                ProductionBranch = productionBranch,
                Source = new Pages_source
                {
                    Type = "github",
                    Config = new Pages_source_config
                    {
                        Owner = repoOwner,
                        RepoName = repoName,
                        ProductionBranch = productionBranch,
                        DeploymentsEnabled = true,
                        ProductionDeploymentsEnabled = true
                    }
                }
            };

            if (!string.IsNullOrEmpty(buildCommand) || !string.IsNullOrEmpty(buildOutputDir))
            {
                project.BuildConfig = new Pages_build_config
                {
                    BuildCommand = buildCommand,
                    DestinationDir = buildOutputDir
                };
            }

            Pages_projectResponse? result = await client.Accounts[accountId].Pages.Projects[projectName].PatchAsync(project, null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully updated GitHub configuration for Pages project {ProjectName}", projectName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update GitHub configuration for Pages project {ProjectName}", projectName);
            throw;
        }
    }

    public async ValueTask<Pages_source?> GetGitHubConfig(string accountId, string projectName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting GitHub configuration for Pages project {ProjectName}", projectName);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            Pages_projectResponse? result = await client.Accounts[accountId].Pages.Projects[projectName].GetAsync(null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully retrieved GitHub configuration for Pages project {ProjectName}", projectName);
            return result.Result?.Source;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get GitHub configuration for Pages project {ProjectName}", projectName);
            throw;
        }
    }
}