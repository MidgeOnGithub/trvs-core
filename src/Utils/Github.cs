using System;
using System.Net;
using System.Threading.Tasks;
using Octokit;

namespace TRVS.Core
{
    /// <summary>
    ///     Provides interaction with Github's REST API.
    /// </summary>
    public static class Github
    {
        public struct RepoInformation
        {
            public string Owner;
            public string Name;
        }

        public struct UserAgentInformation
        {
            public string Name;
            public string Version;
        }

        /// <summary>
        ///     Get the latest <see cref="Release"/> version (based on <see cref="Release.TagName"/>) from Github.
        /// </summary>
        /// <returns>
        ///     The latest release's <see cref="Version"/>, if one is found; otherwise, <see langword="null"></see>.
        /// </returns>
        /// <exception cref="ApiException"></exception>
        public static async Task<Version?> GetLatestVersion(RepoInformation repoInfo, UserAgentInformation agentInfo)
        {
            var github = new GitHubClient(new ProductHeaderValue(agentInfo.Name, agentInfo.Version));
            
            Release latest;
            try
            {
                latest = await github.Repository.Release.GetLatest(repoInfo.Owner, repoInfo.Name);
            }
            catch (ApiException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                    return null;

                throw;
            }

            return latest.TagName[0] == 'v'
                ? new Version(latest.TagName.Substring(1))
                : new Version(latest.TagName);
        }
    }
}