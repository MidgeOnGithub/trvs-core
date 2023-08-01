using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using CommandLine;
using CommandLine.Text;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace TRVS.Core;

/// <summary>
///     Class used by <see cref="Parser"/>.
/// </summary>
// ReSharper doesn't detect CommandLine's Bind
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal class ProgramArguments
{
    [Option('v', Default = false, HelpText = "Enable console logging.")]
    public bool Verbose { get; set; }
}

/// <summary>
///     A more custom, thorough version of <see cref="ParserResult{T}.Tag"/>.
/// </summary>
internal enum ArgumentParseResult
{
    ParsedAndShouldContinue,
    HelpOrVersionArgGiven,
    FailedToParse
}

/// <summary>
///     Represents the deserialized user settings object.
/// </summary>
/// <remarks>
///     ReSharper doesn't recognize the <see langword="public"/> <see langword="set"/>  
///     is used and required for full JSON deserialization, thus the warning is suppressed.
/// </remarks>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public struct TRVSUserSettings
{
    public int LogFileLimit { get; set; }
}

/// <summary>
///     Provides functionality for program initialization and maintenance.
/// </summary>
public class TRVSProgramManager
{
    private TRVSProgramData _programData;

    public TRVSProgramManager(TRVSProgramData programData)
    {
        _programData = programData;
    }

    /// <summary>
    ///     <inheritdoc cref="InitializeProgram"/> <inheritdoc cref="DeleteExcessLogFiles"/>
    /// </summary>
    /// <param name="args">Program arguments</param>
    public void ManageProgram(IEnumerable<string> args)
    {
        InitializeProgram(args);
        DeleteExcessLogFiles();
    }
        
    /// <summary>
    ///     Readies console, handles <paramref name="args"/> and <see cref="TRVSUserSettings"/>, hooks SIGINT.
    /// </summary>
    /// <param name="args">Program arguments</param>
    private void InitializeProgram(IEnumerable<string> args)
    {
        SetStageAndPrintSplash();

        switch (HandleProgramArgs(args))
        {
            case ArgumentParseResult.ParsedAndShouldContinue:
                break;
            case ArgumentParseResult.HelpOrVersionArgGiven:
                Environment.Exit(0); // No need to pause since they definitely used CMD/PS.
                break;
            case ArgumentParseResult.FailedToParse:
                EarlyPauseAndExit(1);
                break;
            default:
                var e = new ArgumentOutOfRangeException();
                GiveErrorMessageAndExit("An unexpected error occurred after parsing arguments.", e, -1);
                break;
        }

        SetSigIntHook();
        HandleUserSettings();
    }

    /// <summary>
    ///     Sets up console and prints the intro splash.
    /// </summary>
    private void SetStageAndPrintSplash()
    {
        Console.Title = $"{_programData.GameAbbreviation} Version Swapper";
        if (Console.WindowWidth < 81)
            Console.WindowWidth = 81;

        foreach (string s in _programData.MiscInfo.AsciiArt)
            ConsoleIO.PrintCentered(s, ConsoleColor.DarkCyan);

        ConsoleIO.PrintCentered("Made with love by Midge", ConsoleColor.DarkCyan);
        ConsoleIO.PrintCentered($"Source code: {_programData.MiscInfo.RepoLink}");
    }

    /// <summary>
    ///     Parses and propagates <paramref name="args"/>.
    /// </summary>
    /// <param name="args">Program arguments</param>
    /// <returns>
    ///     The appropriate <see cref="ArgumentParseResult"/> value
    /// </returns>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private ArgumentParseResult HandleProgramArgs(IEnumerable<string> args)
    {
        var result = ArgumentParseResult.ParsedAndShouldContinue;
        var parser = new Parser(with => with.HelpWriter = null);
        var parserResult = parser.ParseArguments<ProgramArguments>(args);
        parserResult
            .WithParsed(ConfigLogger)
            .WithNotParsed(errs =>
            {
                if (errs.IsHelp() || errs.IsVersion())
                    result = ArgumentParseResult.HelpOrVersionArgGiven;
                else
                    result = ArgumentParseResult.FailedToParse;

                DisplayHelp(parserResult, errs);
            });

        return result;
    }

    /// <summary>
    ///     Configures the logger according to <paramref name="args"/>. 
    /// </summary>
    /// <param name="args"><see cref="ProgramArguments"/></param>
    private void ConfigLogger(ProgramArguments args)
    {
        var consoleLogLevel = args.Verbose ? LogLevel.Info : LogLevel.Off;
        var config = new LoggingConfiguration();

        // Configure the targets and rules.
        var consoleTarget = new ConsoleTarget
        {
            Layout = "${uppercase:${level}}: ${message} ${exception}"
        };
        config.AddRule(consoleLogLevel, LogLevel.Error, consoleTarget);

        Directory.CreateDirectory("logs");
        var fileTarget = new FileTarget
        {
            FileName = Path.Combine("logs", $"{_programData.GameAbbreviation}_Version_Swapper.{DateTime.Now:s}.log"),
            Layout = "${longdate} | ${stacktrace} | ${uppercase:${level}} | ${message} ${exception}"
        };
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

        // Set and load the configuration.
        LogManager.Configuration = config;
        LogManager.ReconfigExistingLoggers();
        _programData.NLogger.Info("Verbose mode activated.");
    }

    /// <summary>
    ///     Generates and prints customized messages for help and version.
    /// </summary>
    /// <param name="result">Results from CommandLine's <see cref="Parser"/></param>
    /// <param name="errs">Errors from CommandLine's <see cref="Parser"/></param>
    private static void DisplayHelp(ParserResult<ProgramArguments> result, IEnumerable<Error> errs)
    {
        HelpText helpText;
        if (errs.IsVersion())
        {
            helpText = HelpText.AutoBuild(result);
        }
        else
        {
            helpText = HelpText.AutoBuild(result, h =>
            {
                h.AddNewLineBetweenHelpSections = false;
                h.AdditionalNewLineAfterOption = false;
                h.MaximumDisplayWidth = 80;
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
        }
        Console.WriteLine(helpText);
    }

    /// <summary>
    ///     Creates a hook to handle SIGINT (CTRL + C).
    /// </summary>
    private void SetSigIntHook()
    {
        Console.CancelKeyPress += delegate {
            _programData.NLogger.Debug("User gave SIGINT. Ending Program.");
            ConsoleIO.PrintWithColor("Received SIGINT. It's up to you to know the current state of your game!", ConsoleColor.Yellow);
        };
    }

    /// <summary>
    ///     Tries to get user settings; creates default if file doesn't exist.
    /// </summary>
    private void HandleUserSettings()
    {
        const string fileName = "appsettings.json";
        if (!File.Exists(fileName))
            CreateDefaultUserSettingsFile(fileName);

        try
        {
            _programData.Settings = GetUserSettingsFromFile(fileName);
        }
        catch (JsonException e)
        {
            const string statement = "An error was encountered while reading the user settings file.";
            GiveErrorMessageAndExit(statement, e, 1);
        }
    }

    /// <summary>
    ///     Writes, then alerts the user of a default user settings file.
    /// </summary>
    /// <param name="fileName">User settings file name</param>
    private void CreateDefaultUserSettingsFile(string fileName)
    {
        using var stream = File.CreateText(fileName);
        foreach (string line in MiscInfoBase.DefaultSettingsFile)
            stream.WriteLine(line);

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        _programData.NLogger.Debug($"Created a default user settings file at {filePath}.");
        Console.WriteLine("I created a default user settings files at");
        Console.WriteLine(filePath);
        Console.WriteLine("You can edit the settings in this file to your desired amounts.");
        Console.WriteLine();
    }

    /// <summary>
    ///     Parses and deserializes a file into a <see cref="TRVSUserSettings"/> object.
    /// </summary>
    /// <param name="fileName">User settings file name</param>
    private static TRVSUserSettings GetUserSettingsFromFile(string fileName)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        var parsedSettings = JsonSerializer.Deserialize<TRVSUserSettings>(File.ReadAllText(fileName), jsonOptions);
        return parsedSettings;
    }

    /// <summary>
    ///     Deletes the oldest log file(s) according to user's set limit.
    /// </summary>
    private void DeleteExcessLogFiles()
    {
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        var files = new List<string>(Directory.GetFiles(dir));
        files.Sort();

        int limit = _programData.Settings.LogFileLimit;
        if (limit == 0)
            return;

        if (files.Count > limit)
        {
            _programData.NLogger.Debug($"Excessive log file count: {files.Count} vs {limit}");
            ConsoleIO.PrintWithColor($"Log file limit of {limit} exceeded (total: {files.Count})", ConsoleColor.Yellow);
            Console.WriteLine("Files will be deleted accordingly.");
            Console.WriteLine();
        }
        else if (files.Count + 3 > limit)
        {
            _programData.NLogger.Debug($"Log file count approaching excessive: {files.Count} vs {limit}");
            ConsoleIO.PrintWithColor($"You are approaching your set log file limit ({files.Count} of {limit})", ConsoleColor.Yellow);
            Console.WriteLine("Be sure to edit appsettings.json to adjust the limit to your tastes.");
            Console.WriteLine();
        }

        while (files.Count > limit)
        {
            try
            {
                File.Delete(files[0]);
                _programData.NLogger.Info($"Deleted excess log file {files[0]}.");
            }
            catch (Exception e)
            {
                _programData.NLogger.Error(e, "Could not delete at least one excess log file.");
                ConsoleIO.PrintWithColor($"You have more than your setting of {limit} log files in the logs folder.", ConsoleColor.Yellow);
                Console.WriteLine("Normally I'd take care of this for you but I had an unexpected error.");
                Console.WriteLine("I've put some additional information in this session's log file.");
                Console.WriteLine();
                break;
            }

            files.RemoveAt(0);
        }
    }

    /// <summary>
    ///     Provides a standardized format to display an error and exit. 
    /// </summary>
    /// <param name="statement">String to log and print to console</param>
    /// <param name="e">Exception causing the early exit</param>
    /// <param name="exitCode"><inheritdoc cref="EarlyPauseAndExit"/></param>
    public void GiveErrorMessageAndExit(string statement, Exception e, int exitCode)
    {
        _programData.NLogger.Fatal($"{statement} {e.Message}\n{e.StackTrace}");
        ConsoleIO.PrintWithColor(statement, ConsoleColor.Red);
        Console.WriteLine("I've put some additional information in this session's log file.");
        EarlyPauseAndExit(exitCode);
    }

    /// <summary>
    ///     Ends program after pausing to prevent immediate CMD window exits.
    /// </summary>
    /// <param name="exitCode">Return code to give the OS</param>
    public static void EarlyPauseAndExit(int exitCode)
    {
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey(true);
        Environment.Exit(exitCode);
    }
}