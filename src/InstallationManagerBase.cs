using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using Octokit;

namespace TRVS.Core;

/// <summary>
///     Validates install location and packaged files.
/// </summary>
/// <typeparam name="TF"><see cref="IFileAudit"/> implementation</typeparam>
/// <typeparam name="TD"><see cref="IDirectories"/> implementation</typeparam>
public abstract class InstallationManagerBase<TD, TF>
    where TD : IDirectories
    where TF : IFileAudit
{
    protected abstract TRVSProgramData ProgramData { get; }
    protected abstract TRVSProgramManager ProgramManager { get; }
    protected abstract TF FileAudit { get; }
    protected abstract TD Directories { get; }

    /// <summary>
    ///     Notifies the user if their program is outdated.
    /// </summary>
    public void VersionCheck()
    {
        ProgramData.NLogger.Debug("Running Github Version checks...");
        var repoInfo = new Github.RepoInformation
        {
            Owner = "TombRunners",
            Name = $"{ProgramData.GameAbbreviation.ToLower()}-version-swapper"
        };
        var agentInfo = new Github.UserAgentInformation
        {
            Name = "TRVS",
            Version = ProgramData.Version.ToString()
        };

        try
        {
            var latest = Github.GetLatestVersion(repoInfo, agentInfo).GetAwaiter().GetResult();
            if (latest is null)
            {
                ProgramData.NLogger.Debug("No releases found.");
                Console.WriteLine("I didn't find any latest release information.");
                Console.WriteLine("Perhaps no releases exist or the URL was bad.");
                Console.WriteLine("If release information was expected, please bring up the issue!");
                Console.WriteLine("Otherwise... Let me know how testing goes! :D");                
            }
            else
            {
                int result = ProgramData.Version.CompareTo(latest, 3);
                switch (result)
                {
                    case -1:
                        ProgramData.NLogger.Debug($"Latest Github release ({latest}) is newer than the running version ({result}).");
                        ConsoleIO.PrintHeader("A new release is available!", ProgramData.MiscInfo.LatestReleaseLink, ConsoleColor.Yellow);
                        Console.WriteLine("You are strongly advised to update to ensure leaderboard compatibility.");
                        break;
                    case 0:
                        ProgramData.NLogger.Debug($"Version is up-to-date ({latest}).");
                        break;
                    default: // result == 1
                        ProgramData.NLogger.Debug($"Running version ({ProgramData.Version}) has not yet been released on Github ({latest}).");
                        Console.WriteLine("You seem to be running a pre-release version.");
                        Console.WriteLine("Let me know how testing goes! :D");
                        break;
                }
            }
        }
        catch (Exception e)
        {
            ProgramData.NLogger.Error(e is ApiException or HttpRequestException
                ? $"Github request failed due to an API/HTTP failure. {e.Message}\n{e.StackTrace}"
                : $"Version check failed with an unforeseen error. {e.Message}\n{e.StackTrace}");

            ConsoleIO.PrintWithColor("Unable to check for the latest version. Consider manually checking:", ConsoleColor.Yellow);
            Console.WriteLine(ProgramData.MiscInfo.LatestReleaseLink);
        }
        finally
        {
            Console.WriteLine();
        }
    }

    /// <summary>
    ///     Validates packaged files, ensures target directory looks like a TR game.
    /// </summary>
    public void ValidateInstallation()
    {
        try
        {
            ValidatePackagedFiles();
            ProgramData.NLogger.Info("Successfully validated packaged files using MD5 hashes.");
            CheckGameDirLooksLikeATrInstall();
            ProgramData.NLogger.Info($"Parent directory seems like a {ProgramData.GameAbbreviation} game installation.");
        }
        catch (Exception e)
        {
            if (e is BadInstallationLocationException or RequiredFileMissingException or InvalidGameFileException)
            {
                ProgramData.NLogger.Fatal($"Installation failed to validate. {e.Message}\n{e.StackTrace}");
                ConsoleIO.PrintWithColor(e.Message, ConsoleColor.Red);
                Console.WriteLine("You are advised to re-install the latest release to fix the issue:");
                Console.WriteLine(ProgramData.MiscInfo.LatestReleaseLink);
                TRVSProgramManager.EarlyPauseAndExit(2);
            }

            const string statement = "An unhandled exception occurred while validating your installation.";
            ProgramManager.GiveErrorMessageAndExit(statement, e, 1);
        }
    }

    /// <summary>
    ///     Ensures files packaged in releases are untampered.
    /// </summary>
    protected abstract void ValidatePackagedFiles();

    /// <summary>
    ///     Ensures target directory contains affected game files and folders.
    /// </summary>
    /// <exception cref="BadInstallationLocationException">Targeted directory is missing a file or folder</exception>
    private void CheckGameDirLooksLikeATrInstall()
    {
        string? missingFile = FileIO.FindMissingFile(FileAudit.GameFiles, Directories.Game);
        if (!string.IsNullOrEmpty(missingFile))
            throw new BadInstallationLocationException($"Parent folder is missing game file {missingFile}, cannot be a {ProgramData.GameAbbreviation} installation.");
    }

    /// <summary>
    ///     Checks that files in <paramref name="dir"/> match their required MD5 hashes.
    /// </summary>
    /// <param name="fileAudit">Mapping of file names to readable, lowercased MD5 hashes</param>
    /// <param name="dir">Directory to operate within</param>
    /// <exception cref="RequiredFileMissingException">A file was not found</exception>
    /// <exception cref="InvalidGameFileException">A file's MD5 hash did not match expected value</exception>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    protected static void ValidateMd5Hashes(Dictionary<string, string> fileAudit, string dir)
    {
        foreach (var item in fileAudit)
        {
            try
            {
                string hash = FileIO.ComputeMd5Hash(Path.Combine(dir, item.Key));
                if (hash != item.Value)
                    throw new InvalidGameFileException($"File {item.Key} was modified.\nGot {hash}, expected {item.Value}");
            }
            catch (FileNotFoundException e)
            {
                throw new RequiredFileMissingException(e.Message);
            }
        }
    }
}