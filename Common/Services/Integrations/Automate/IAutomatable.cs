#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Services.Integrations.Automate;
#else

namespace StardewMods.Common.Services.Integrations.Automate;
#endif

using System.ComponentModel;
using Microsoft.Xna.Framework;

// ReSharper disable All
#pragma warning disable

/// <summary>
/// An automatable entity, which can implement a more specific type like <see cref="IMachine" /> or
/// <see cref="IContainer" />. If it doesn't implement a more specific type, it's treated as a connector with no additional
/// logic.
/// </summary>
public interface IAutomatable
{
    /*********
     ** Accessors
     *********/

    /// <summary>The location which contains the machine.</summary>
    GameLocation Location { get; }

    /// <summary>The tile area covered by the machine.</summary>
    Rectangle TileArea { get; }
}