using System.Collections.Generic;

namespace TRVSCore
{
    /// <summary>
    ///     Contains misc info for console output and settings file creation.
    /// </summary>
    public abstract class MiscInfoBase
    {
        /// <summary>
        ///     Text art for the intro splash.
        /// </summary>
        public abstract IEnumerable<string> AsciiArt { get; }
        
        /// <summary>
        ///     The project's Github repo link.
        /// </summary>
        public abstract string RepoLink { get; }
        
        /// <summary>
        ///     The project's Github release link.
        /// </summary>
        public abstract string LatestReleaseLink { get; }

        /// <summary>
        ///     The raw text needed to create a default settings file.
        /// </summary>
        public static readonly string[] DefaultSettingsFile =
        {
            @"{",
            @"  // The number of log files the program will allow before deleting the oldest one(s).",
            @"  // Set to 0 to allow infinite log file generation.",
            @"  // Default: 15",
            @"  ""LogFileLimit"": 15",
            @"}",
        };
    }
}
