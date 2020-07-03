namespace TRVSCore
{
    /// <summary>
    ///     Directories used by <see cref="InstallationManagerBase{TD,TF}"/> and <see cref="VersionSwapperBase{TD}"/>
    /// </summary>
    public interface IDirectories
    {
        /// <summary>
        ///     The location of the installed TR game being operated on.
        /// </summary>
        /// <remarks>
        ///     Should be the parent folder of the installed version swapper.
        /// </remarks>
        public string Game { get; }
    }
}