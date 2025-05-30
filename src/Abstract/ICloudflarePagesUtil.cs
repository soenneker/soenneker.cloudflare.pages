using Soenneker.Cloudflare.OpenApiClient.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Cloudflare.Pages.Abstract;

/// <summary>
/// Provides utility methods for managing Cloudflare Pages projects
/// </summary>
public interface ICloudflarePagesUtil
{
    /// <summary>
    /// Creates a new Pages project in the specified Cloudflare account.
    /// </summary>
    /// <param name="accountId">The Cloudflare account ID.</param>
    /// <param name="name">The name of the new project.</param>
    /// <param name="productionBranch">The name of the production branch.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The created Pages project response.</returns>
    ValueTask<Pages_projectResponse> Create(string accountId, string name, string productionBranch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Pages project by name.
    /// </summary>
    /// <param name="accountId">The Cloudflare account ID.</param>
    /// <param name="name">The name of the project to retrieve.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The retrieved Pages project response.</returns>
    ValueTask<Pages_projectResponse> Get(string accountId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a Pages project with a new production branch.
    /// </summary>
    /// <param name="accountId">The Cloudflare account ID.</param>
    /// <param name="name">The name of the project to update.</param>
    /// <param name="productionBranch">The new production branch name.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The updated Pages project response.</returns>
    ValueTask<Pages_projectResponse> Update(string accountId, string name, string productionBranch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a Pages project.
    /// </summary>
    /// <param name="accountId">The Cloudflare account ID.</param>
    /// <param name="name">The name of the project to delete.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    ValueTask Delete(string accountId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all Pages projects in a Cloudflare account.
    /// </summary>
    /// <param name="accountId">The Cloudflare account ID.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of Pages deployments.</returns>
    ValueTask<List<Pages_deployments>> List(string accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a custom domain to a Pages project.
    /// </summary>
    /// <param name="accountId">The Cloudflare account ID.</param>
    /// <param name="projectName">The name of the Pages project.</param>
    /// <param name="domainName">The custom domain name to add.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The response for the added domain.</returns>
    ValueTask<Pages_domainResponseSingle> AddCustomDomain(string accountId, string projectName, string domainName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a custom domain from a Pages project.
    /// </summary>
    /// <param name="accountId">The Cloudflare account ID.</param>
    /// <param name="projectName">The name of the Pages project.</param>
    /// <param name="domainName">The custom domain name to remove.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    ValueTask RemoveCustomDomain(string accountId, string projectName, string domainName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all custom domains associated with a Pages project.
    /// </summary>
    /// <param name="accountId">The Cloudflare account ID.</param>
    /// <param name="projectName">The name of the Pages project.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A collection of custom domain objects.</returns>
    ValueTask<IEnumerable<Pages_domainObject>> ListCustomDomains(string accountId, string projectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Connects a Pages project to a GitHub repository and optionally sets build configuration.
    /// </summary>
    /// <param name="accountId">The Cloudflare account ID.</param>
    /// <param name="projectName">The name of the Pages project to create and connect.</param>
    /// <param name="repoOwner">The GitHub repository owner (user or org).</param>
    /// <param name="repoName">The GitHub repository name.</param>
    /// <param name="productionBranch">The production branch to track for deployments.</param>
    /// <param name="buildCommand">Optional build command to run during deployment.</param>
    /// <param name="buildOutputDir">Optional directory containing build output.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The created Pages project response.</returns>
    ValueTask<Pages_projectResponse> ConnectToGitHub(string accountId, string projectName, string repoOwner, string repoName,
        string productionBranch, string? buildCommand = null, string? buildOutputDir = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the GitHub connection and build configuration for an existing Pages project.
    /// </summary>
    /// <param name="accountId">The Cloudflare account ID.</param>
    /// <param name="projectName">The name of the existing Pages project.</param>
    /// <param name="repoOwner">The GitHub repository owner (user or org).</param>
    /// <param name="repoName">The GitHub repository name.</param>
    /// <param name="productionBranch">The production branch to track for deployments.</param>
    /// <param name="buildCommand">Optional updated build command.</param>
    /// <param name="buildOutputDir">Optional updated build output directory.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The updated Pages project response.</returns>
    ValueTask<Pages_projectResponse> UpdateGitHubConfig(string accountId, string projectName, string repoOwner, string repoName,
        string productionBranch, string? buildCommand = null, string? buildOutputDir = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the GitHub source configuration for a Pages project.
    /// </summary>
    /// <param name="accountId">The Cloudflare account ID.</param>
    /// <param name="projectName">The name of the Pages project.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The Pages source configuration, or null if not set.</returns>
    ValueTask<Pages_source?> GetGitHubConfig(string accountId, string projectName, CancellationToken cancellationToken = default);

}