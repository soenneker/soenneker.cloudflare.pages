using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using Soenneker.Cloudflare.OpenApiClient;
using Soenneker.Cloudflare.OpenApiClient.Models;
using Soenneker.Cloudflare.Pages.Abstract;
using Soenneker.Cloudflare.Utils.Client.Abstract;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Cloudflare.DnsRecords.Abstract;
using Soenneker.Cloudflare.Zones.Abstract;
using Soenneker.Extensions.String;

namespace Soenneker.Cloudflare.Pages;

///<inheritdoc cref="ICloudflarePagesUtil"/>
public sealed class CloudflarePagesUtil : ICloudflarePagesUtil
{
    private readonly ICloudflareClientUtil _client;
    private readonly ICloudflareDnsRecordsUtil _dnsRecordsUtil;
    private readonly ICloudflareZonesUtil _zonesUtil;
    private readonly ILogger<CloudflarePagesUtil> _logger;

    public CloudflarePagesUtil(ICloudflareClientUtil client, ILogger<CloudflarePagesUtil> logger, ICloudflareDnsRecordsUtil dnsRecordsUtil,
        ICloudflareZonesUtil zonesUtil)
    {
        _client = client;
        _logger = logger;
        _dnsRecordsUtil = dnsRecordsUtil;
        _zonesUtil = zonesUtil;
    }

    public async ValueTask<Pages_project> Create(string accountId, string projectName, string productionBranch, string? buildCommand,
        string? buildOutputDir, bool deployNow = false, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating Pages project {Name} in account {AccountId}", projectName, accountId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();

        try
        {
            var project = new Pages_project_create_project
            {
                Name = projectName,
                ProductionBranch = productionBranch
            };

            if (buildCommand.HasContent() || buildOutputDir.HasContent())
            {
                project.BuildConfig = new Pages_project_create_project_build_config
                {
                    BuildCommand = buildCommand,
                    DestinationDir = buildOutputDir
                };
            }

            var result = await client.Accounts[accountId].Pages.Projects.PostAsync(project, null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully created Pages project {Name}", projectName);

            if (deployNow)
            {
                await CreateDeployment(accountId, projectName, productionBranch, cancellationToken).NoSync();
            }

            return result?.Result ?? throw new InvalidOperationException("Failed to create project: result is null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Pages project {Name}", projectName);
            throw;
        }
    }

    public async ValueTask<Pages_project> CreateWithGitHub(string accountId, string projectName, string repoOwner, string repoName,
        string productionBranch, bool deployNow = false, string? buildCommand = null, string? buildOutputDir = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating Pages project {ProjectName} with GitHub repository {RepoOwner}/{RepoName}", projectName, repoOwner, repoName);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var project = new Pages_project_create_project
            {
                Name = projectName,
                ProductionBranch = productionBranch,
                Source = new Pages_project_create_project_source
                {
                    Type = Pages_project_create_project_source_type.Github,
                    Config = new Pages_project_create_project_source_config
                    {
                        Owner = repoOwner,
                        RepoName = repoName,
                        ProductionBranch = productionBranch,
                        DeploymentsEnabled = true,
                        ProductionDeploymentsEnabled = true
                    }
                }
            };

            if (buildCommand.HasContent() || buildOutputDir.HasContent())
            {
                project.BuildConfig = new Pages_project_create_project_build_config
                {
                    BuildCommand = buildCommand,
                    DestinationDir = buildOutputDir
                };
            }

            Pages_project_create_project_200? result = await client.Accounts[accountId].Pages.Projects.PostAsync(project, null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully created Pages project {ProjectName} with GitHub repository {RepoOwner}/{RepoName}", projectName, repoOwner,
                repoName);

            if (deployNow)
            {
                await CreateDeployment(accountId, projectName, productionBranch, cancellationToken).NoSync();
            }

            return result?.Result ?? throw new InvalidOperationException("Failed to create project: result is null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Pages project {ProjectName} with GitHub repository {RepoOwner}/{RepoName}", projectName, repoOwner,
                repoName);
            throw;
        }
    }

    public async ValueTask<Pages_project> Get(string accountId, string name, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting Pages project {Name} from account {AccountId}", name, accountId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();

        try
        {
            Pages_project_get_project_200? result = await client.Accounts[accountId].Pages.Projects[name].GetAsync(null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully retrieved Pages project {Name}", name);
            return result?.Result ?? throw new InvalidOperationException("Failed to get project: result is null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Pages project {Name}", name);
            throw;
        }
    }

    public async ValueTask<Pages_project> Update(string accountId, string name, string productionBranch, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating Pages project {Name} in account {AccountId}", name, accountId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var project = new Pages_project_update_project
            {
                ProductionBranch = productionBranch
            };

            Pages_project_update_project_200? result = await client.Accounts[accountId].Pages.Projects[name].PatchAsync(project, null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully updated Pages project {Name}", name);
            return result?.Result ?? throw new InvalidOperationException("Failed to update project: result is null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Pages project {Name}", name);
            throw;
        }
    }

    public async ValueTask Delete(string accountId, string name, string zoneDomain, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting Pages project {Name} from account {AccountId}", name, accountId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            // First, get all custom domains for the project
            List<Pages_domain> domains = await ListCustomDomains(accountId, name, cancellationToken).NoSync();

            // Delete each custom domain if any exist
            if (domains.Count > 0)
            {
                foreach (Pages_domain domain in domains)
                {
                    await RemoveCustomDomain(accountId, name, zoneDomain, domain.Name, cancellationToken).NoSync();
                }
            }

            // Now delete the Pages project
            await client.Accounts[accountId].Pages.Projects[name].DeleteAsync(null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully deleted Pages project {Name}", name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Pages project {Name}", name);
            throw;
        }
    }

    public async ValueTask<List<Pages_project>> List(string accountId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listing Pages projects in account {AccountId}", accountId);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            Pages_project_get_projects_200? result = await client.Accounts[accountId].Pages.Projects.GetAsync(null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully listed Pages projects");
            return result?.Result ?? new List<Pages_project>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list Pages projects");
            throw;
        }
    }

    public async ValueTask<Pages_domain> AddCustomDomain(string accountId, string projectName, string zoneDomain, string customDomain,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding custom domain {DomainName} to Pages project {ProjectName}", customDomain, projectName);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var domain = new Pages_domains_add_domain
            {
                Name = customDomain
            };

            Pages_domains_add_domain_200? result =
                await client.Accounts[accountId].Pages.Projects[projectName].Domains.PostAsync(domain, null, cancellationToken).NoSync();

            string? zoneId = await _zonesUtil.GetId(zoneDomain, cancellationToken).NoSync();

            await _dnsRecordsUtil.AddCnameRecord(zoneId, customDomain, $"{projectName}.pages.dev", 1, true, cancellationToken).NoSync();

            _logger.LogInformation("Successfully added custom domain {DomainName} to Pages project {ProjectName}", customDomain, projectName);
            return result?.Result ?? throw new InvalidOperationException("Failed to add domain: result is null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add custom domain {DomainName} to Pages project {ProjectName}", customDomain, projectName);
            throw;
        }
    }

    public async ValueTask RemoveCustomDomain(string accountId, string projectName, string zoneDomain, string customDomain,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing custom domain {DomainName} from Pages project {ProjectName}", customDomain, projectName);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            await client.Accounts[accountId].Pages.Projects[projectName].Domains[customDomain].DeleteAsync(null, cancellationToken).NoSync();

            string zoneId = await _zonesUtil.GetId(zoneDomain, cancellationToken).NoSync();

            await _dnsRecordsUtil.RemoveCnameRecord(zoneId, customDomain, cancellationToken).NoSync();

            _logger.LogInformation("Successfully removed custom domain {DomainName} from Pages project {ProjectName}", customDomain, projectName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove custom domain {DomainName} from Pages project {ProjectName}", customDomain, projectName);
            throw;
        }
    }

    public async ValueTask<List<Pages_domain>> ListCustomDomains(string accountId, string projectName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listing custom domains for Pages project {ProjectName}", projectName);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            Pages_domains_get_domains_200? result =
                await client.Accounts[accountId].Pages.Projects[projectName].Domains.GetAsync(null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully listed custom domains for Pages project {ProjectName}", projectName);
            return result?.Result ?? new List<Pages_domain>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list custom domains for Pages project {ProjectName}", projectName);
            throw;
        }
    }


    public async ValueTask<Pages_project> UpdateGitHubConfig(string accountId, string projectName, string repoOwner, string repoName,
        string productionBranch, string? buildCommand = null, string? buildOutputDir = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating GitHub configuration for Pages project {ProjectName}", projectName);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var project = new Pages_project_update_project
            {
                ProductionBranch = productionBranch,
                Source = new Pages_project_update_project_source
                {
                    Type = Pages_project_update_project_source_type.Github,
                    Config = new Pages_project_update_project_source_config
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
                project.BuildConfig = new Pages_project_update_project_build_config
                {
                    BuildCommand = buildCommand,
                    DestinationDir = buildOutputDir
                };
            }

            Pages_project_update_project_200? result = await client.Accounts[accountId].Pages.Projects[projectName].PatchAsync(project, null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully updated GitHub configuration for Pages project {ProjectName}", projectName);
            return result?.Result ?? throw new InvalidOperationException("Failed to update project: result is null");
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
            var result = await client.Accounts[accountId].Pages.Projects[projectName].GetAsync(null, cancellationToken).NoSync();
            _logger.LogInformation("Successfully retrieved GitHub configuration for Pages project {ProjectName}", projectName);
            return result.Result?.Source;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get GitHub configuration for Pages project {ProjectName}", projectName);
            throw;
        }
    }

    public async ValueTask<Pages_deployment> CreateDeployment(string accountId, string projectName, string branch,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating deployment for Pages project {ProjectName} from branch {Branch}", projectName, branch);
        CloudflareOpenApiClient client = await _client.Get(cancellationToken).NoSync();
        try
        {
            var deployment = new MultipartBody();
            deployment.AddOrReplacePart("branch", "text/plain", branch);

            Pages_deployment_create_deployment_200? result = await client.Accounts[accountId]
                                                                  .Pages.Projects[projectName]
                                                                  .Deployments.PostAsync(deployment, null, cancellationToken)
                                                                  .NoSync();
            _logger.LogInformation("Successfully created deployment for Pages project {ProjectName} from branch {Branch}", projectName, branch);
            return result?.Result ?? throw new InvalidOperationException("Failed to create deployment: result is null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create deployment for Pages project {ProjectName} from branch {Branch}", projectName, branch);
            throw;
        }
    }
}