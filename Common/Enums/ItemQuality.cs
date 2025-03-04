#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Enums;
#else

namespace StardewMods.Common.Enums;
#endif

using NetEscapades.EnumGenerators;

/// <summary>Represents the quality of an item.</summary>
[EnumExtensions]
public enum ItemQuality
{
    /// <summary>No quality.</summary>
    None = SObject.lowQuality,

    /// <summary>Silver quality.</summary>
    Silver = SObject.medQuality,

    /// <summary>Gold quality.</summary>
    Gold = SObject.highQuality,

    /// <summary>Iridium quality.</summary>
    Iridium = SObject.bestQuality,
}