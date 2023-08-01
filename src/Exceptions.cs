// ReSharper disable UnusedMember.Global
using System;

namespace TRVS.Core;

/// <summary>
///     When the installed program is not in a TR game installation.
/// </summary>
public class BadInstallationLocationException : Exception
{
    /// <inheritdoc/>
    public BadInstallationLocationException() { }

    /// <inheritdoc/>
    public BadInstallationLocationException(string message)
        : base(message) { }

    /// <inheritdoc/>
    public BadInstallationLocationException(string message, Exception inner)
        : base(message, inner) { }
}

/// <summary>
///     When a packaged game file has been tampered.
/// </summary>
public class InvalidGameFileException : Exception
{
    /// <inheritdoc/>
    public InvalidGameFileException() { }

    /// <inheritdoc/>
    public InvalidGameFileException(string message)
        : base(message) { }

    /// <inheritdoc/>
    public InvalidGameFileException(string message, Exception inner)
        : base(message, inner) { }
}

/// <summary>
///     When a packaged game file is missing.
/// </summary>
public class RequiredFileMissingException : Exception
{
    /// <inheritdoc/>
    public RequiredFileMissingException() { }

    /// <inheritdoc/>
    public RequiredFileMissingException(string message)
        : base(message) { }

    /// <inheritdoc/>
    public RequiredFileMissingException(string message, Exception inner)
        : base(message, inner) { }
}