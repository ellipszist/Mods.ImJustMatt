namespace StardewMods.GarbageDay.Framework.Services;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.GarbageDay.Framework.Interfaces;
using StardewMods.GarbageDay.Framework.Models;

/// <inheritdoc cref="StardewMods.GarbageDay.Framework.Interfaces.IModConfig" />
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="dataHelper">Dependency used for storing and retrieving data.</param>
    /// <param name="eventPublisher">Dependency used for publishing events.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(IDataHelper dataHelper, IEventPublisher eventPublisher, IModHelper modHelper)
        : base(dataHelper, eventPublisher, modHelper) { }

    /// <inheritdoc />
    public DayOfWeek GarbageDay => this.Config.GarbageDay;

    /// <inheritdoc />
    public bool OnByDefault => this.Config.OnByDefault;
}