using System;
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
        ///     Get the latest <see cref="Release"/> version from Github using <see cref="Octokit"/>.
        /// </summary>
        /// <returns>
        ///     The latest Github release's version (based on <see cref="Release.TagName"/>).
        /// </returns>
        public static async Task<Version> GetLatestVersion(RepoInformation repoInfo, UserAgentInformation agentInfo)
        {
            var github = new GitHubClient(new ProductHeaderValue(agentInfo.Name, agentInfo.Version));
            Release latest = await github.Repository.Release.GetLatest(repoInfo.Owner, repoInfo.Name);
            return latest.TagName[0] == 'v'
                ? new Version(latest.TagName.Substring(1))
                : new Version(latest.TagName);
        }
    }
}