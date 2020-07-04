using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace TRVS.Core
{
    /// <summary>
    ///     TR version swapping functionality.
    /// </summary>
    /// <typeparam name="TD"><see cref="IDirectories"/> implementation</typeparam>
    public abstract class VersionSwapperBase<TD>
        where TD : IDirectories
    {
        protected abstract TRVSProgramData ProgramData { get; }
        protected abstract TRVSProgramManager ProgramManager { get; }
        protected abstract TD Directories { get; }

        /// <summary>
        ///     Runs the version swap functionality.
        /// </summary>
        public abstract void SwapVersions();

        /// <summary>
        ///     Try to prevent but closing program if errors occur while copying.
        /// </summary>
        /// <param name="srcDir">The directory to copy from</param>
        /// <param name="destDir">The directory to copy to</param>
        protected void TryCopyingDirectory(string srcDir, string destDir)
        {
            // If the EXE is in use, it will cause issues when trying to overwrite it.
            EnsureNoTrGameRunningFromGameDir(Directories.Game);
            // Try to perform the copy.
            try
            {
                ProgramData.NLogger.Debug($"Attempting a copy from \"{srcDir}\" to \"{destDir}\"");
                FileIO.CopyDirectory(srcDir, destDir, true);
            }
            catch (Exception e)
            {
                ProgramManager.GiveErrorMessageAndExit("Failed to copy files!", e, 3);
            }
        }

        /// <summary>
        ///     Ensures any TR <see cref="Process"/> from the target directory is killed.
        /// </summary>
        private void EnsureNoTrGameRunningFromGameDir(string gameDirectory)
        {
            try
            {
                Process trProcess = FindTrGameRunningFromGameDir(gameDirectory);
                if (trProcess == null)
                {
                    ProgramData.NLogger.Debug($"No {ProgramData.GameAbbreviation} process of concern found; looks safe to copy files.");
                }
                else
                {
                    ProgramData.NLogger.Info($"Found {ProgramData.GameAbbreviation} process of concern.");
                    KillRunningTrGame(trProcess);
                    ProgramData.NLogger.Info($"Handled {ProgramData.GameAbbreviation} process of concern.");
                }
            }
            catch (Exception e)
            {
                ProgramData.NLogger.Error($"An unexpected error occurred while trying to find running {ProgramData.GameAbbreviation} processes. {e.Message}\n{e.StackTrace}");
                ConsoleIO.PrintWithColor($"I was unable to finish searching for running {ProgramData.GameAbbreviation} processes.", ConsoleColor.Yellow);
                Console.WriteLine($"Please note that a {ProgramData.GameAbbreviation} game or background task running from the target folder");
                Console.WriteLine("could cause the program to crash due to errors.");
                Console.WriteLine($"Double-check and make sure no {ProgramData.GameAbbreviation} game or background task is running.");
            }
        }

        /// <summary>
        ///     Finds a TR <see cref="Process"/> from the target directory if it exists.
        /// </summary>
        /// <returns>
        ///     The running <see cref="Process"/> or <see langword="null"/> if none was found.
        /// </returns>
        private Process FindTrGameRunningFromGameDir(string gameDirectory)
        {
            ProgramData.NLogger.Debug($"Checking for a {ProgramData.GameAbbreviation} process running in the target folder...");
            Process[] processes = Process.GetProcesses();
            return processes.FirstOrDefault(p =>
                p.ProcessName.ToLower() == ProgramData.GameExe &&
                Directory.GetParent(p.MainModule?.FileName).FullName == gameDirectory
            );
        }

        /// <summary>
        ///     Asks the user how to kill <paramref name="p"/>, then acts accordingly.
        /// </summary>
        /// <param name="p"><see cref="Process"/> of concern</param>
        private void KillRunningTrGame(Process p)
        {
            string processInfo = $"Name: {p.ProcessName} | ID: {p.Id} | Start time: {p.StartTime.TimeOfDay}";
            ProgramData.NLogger.Debug($"Found a {ProgramData.GameAbbreviation} process running from target folder. {processInfo}");
            ConsoleIO.PrintWithColor($"{ProgramData.GameAbbreviation} is running from the target folder.", ConsoleColor.Yellow);
            ConsoleIO.PrintWithColor(processInfo, ConsoleColor.Yellow);
            Console.WriteLine("Would you like me to end the task for you? If not, I will give a message");
            Console.Write("describing how to find and close it. ");
            if (ConsoleIO.UserPromptYesNo())
            {
                ProgramData.NLogger.Debug($"User wants the program to kill the running {ProgramData.GameAbbreviation} task.");
                try
                {
                    p.Kill();
                }
                catch (Exception e)
                {
                    ProgramData.NLogger.Error(e, $"An unexpected error occurred while trying to kill the {ProgramData.GameAbbreviation} process.");
                    ConsoleIO.PrintWithColor($"I was unable to kill the {ProgramData.GameAbbreviation} process. You will have to do it yourself.", ConsoleColor.Yellow);
                    ProgramData.NLogger.Debug("Going into the user prompt loop due to a failure in killing the process.");
                    LetUserKillTask(p);
                }
            }
            else
            {
                ProgramData.NLogger.Debug($"User opted to kill the running {ProgramData.GameAbbreviation} process on their own.");
                LetUserKillTask(p);
            }

            // During testing, it was found that the program went from process killing to file copying too fast,
            // and the files had not yet been freed for access, causing exceptions. Briefly pausing seems to fix this.
            Thread.Sleep(100);
        }

        /// <summary>
        ///     Puts the user in a prompt loop until they kill <paramref name="p"/>.
        /// </summary>
        /// <param name="p">TR process of concern</param>
        private void LetUserKillTask(Process p)
        {
            bool stillRunning = !p.HasExited;
            if (!stillRunning)
            {
                ProgramData.NLogger.Debug("Process ended before the user prompt loop started.");
                Console.WriteLine("Process ended before I could prompt you. Skipping prompt loop.");
                Console.WriteLine();
            }

            while (stillRunning)
            {
                Console.WriteLine($"Be sure that all {ProgramData.GameAbbreviation} game windows are closed. Then, if you are still");
                Console.WriteLine("getting this message, check Task Manager for any phantom processes.");
                Console.WriteLine("Press a key to continue. Or press CTRL + C to exit this program.");
                ProgramData.NLogger.Debug("Waiting for user to close the running task, running ReadKey.");
                Console.ReadKey(true);
                stillRunning = !p.HasExited;
                if (stillRunning)
                {
                    ProgramData.NLogger.Debug($"User tried to continue but the {ProgramData.GameAbbreviation} process is still running, looping.");
                    Console.WriteLine("Process still running, prompting again.");
                }
                else
                {
                    ProgramData.NLogger.Debug($"User continued the program after the {ProgramData.GameAbbreviation} process had exited.");
                    Console.WriteLine();
                }
            }
        }
    }
}