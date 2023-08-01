using System;
using NLog;

namespace TRVS.Core;

public struct TRVSProgramData
{
    public string GameAbbreviation;
    public string GameExe;
    public Logger NLogger;
    public MiscInfoBase MiscInfo;
    public TRVSUserSettings Settings;
    public Version Version;
}