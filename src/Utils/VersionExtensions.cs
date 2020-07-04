using System;

namespace TRVS.Core
{
    /// <summary>
    ///     Provides extended functionality to <see cref="Version"/>.
    /// </summary>
    public static class VersionExtensions
    {
        /// <summary>
        ///     Compares two <see cref="Version"/>s to the specified <paramref name="significantParts"/>.
        /// </summary>
        /// <remarks>
        ///     Credits: https://stackoverflow.com/a/28695949/10466817
        ///     Provides an alternative to <see cref="Version"/>'s compare, which sometimes yields undesirable results when comparing
        ///     a version with no <see cref="Version.Revision"/> number to a version with a <see cref="Version.Revision"/> number, etc.
        ///     With this extension method, you specify up-front how many of the version's numbers to compare.
        /// </remarks>
        /// <returns>
        ///    -1 if <see langword="this"/> is less, 0 if equal, 1 if <see langword="this"/> is greater
        /// </returns>
        public static int CompareTo(this Version version, Version otherVersion, int significantParts)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));
            if (otherVersion == null)
                return 1;

            if (version.Major != otherVersion.Major && significantParts >= 1)
            {
                if (version.Major > otherVersion.Major)
                    return 1;
                return -1;
            }

            if (version.Minor != otherVersion.Minor && significantParts >= 2)
            {
                if (version.Minor > otherVersion.Minor)
                    return 1;
                return -1;
            }

            if (version.Build != otherVersion.Build && significantParts >= 3)
            {
                if (version.Build > otherVersion.Build)
                    return 1;
                return -1;
            }

            if (version.Revision != otherVersion.Revision && significantParts >= 4)
            {
                if (version.Revision > otherVersion.Revision)
                    return 1;
                return -1;
            }

            return 0;
        }
    }
}
