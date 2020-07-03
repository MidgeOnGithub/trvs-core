using System.Collections.Generic;

namespace TRVSCore
{
    /// <summary>
    ///     Contains information about packaged files' names and MD5 hashes.
    /// </summary>
    public interface IFileAudit
    {
        /// <summary>
        ///     The bare minimum files required for game/version recognition.
        /// </summary>
        public IEnumerable<string> GameFiles { get; }
    }
}
