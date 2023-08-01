using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TRVS.Core
{
    /// <summary>
    ///     Serves as the base for Version Swapper program entries.  
    /// </summary>
    /// <typeparam name="TD"><see cref="IDirectories"/> implementation</typeparam>
    /// <typeparam name="TF"><see cref="IFileAudit"/> implementation</typeparam>
    /// <typeparam name="TI"><see cref="InstallationManagerBase{TD,TF}"/> implementation</typeparam>
    /// <typeparam name="TV"><see cref="VersionSwapperBase{TD}"/> implementation</typeparam>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class ProgramBase<TD, TF, TI, TV>
        where TD : IDirectories
        where TF : IFileAudit
        where TI : InstallationManagerBase<TD, TF>
        where TV : VersionSwapperBase<TD>
    {
        protected abstract TRVSProgramData ProgramData { get; }
        protected abstract TD Directories { get; }
        protected abstract TF FileAudit { get; }

        /// <summary>
        ///     Runs the TR version swapper.
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>
        ///     OS exit code
        /// </returns>
        public int Main(IEnumerable<string> args)
        {
            var programManager = new TRVSProgramManager(ProgramData);
            programManager.ManageProgram(args);
            
            var installationManager = 
                (TI)Activator.CreateInstance(typeof(TI), ProgramData, programManager, FileAudit, Directories);
            installationManager.VersionCheck();
            installationManager.ValidateInstallation();

            var versionSwapper = 
                (TV)Activator.CreateInstance(typeof(TV), ProgramData, programManager, Directories);
            versionSwapper.SwapVersions();

            ConsoleIO.PrintHeader("Version swap complete!", "Press any key to exit...", ConsoleColor.White);
            Console.ReadKey(true);
            return 0;
        }
    }
}
