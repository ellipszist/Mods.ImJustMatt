#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Enums;
#else

namespace StardewMods.Common.Enums;
#endif

using NetEscapades.EnumGenerators;

/// <summary>The amount of debugging information that will be logged to the console.</summary>
[EnumExtensions]
public enum SimpleLogLevel
{
    /// <summary>No debugging information will be logged to the console.</summary>
    None = 0,

    /// <summary>Less debugging information will be logged to the console.</summary>
    Less = 1,

    /// <summary>More debugging information will be logged to the console.</summary>
    More = 2,
}