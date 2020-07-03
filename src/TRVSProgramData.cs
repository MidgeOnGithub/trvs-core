using System;
using NLog;

namespace TRVSCore
{
    public struct TRVSProgramData
    {
        public string GameAbbreviation;
        public string GameExe;
        public Logger NLogger;
        public MiscInfoBase MiscInfo;
        public TRVSUserSettings Settings;
        public Version Version;
    }
}
