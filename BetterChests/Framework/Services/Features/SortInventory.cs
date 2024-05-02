namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Customize how an inventory gets sorted.</summary>
internal sealed class SortInventory : BaseFeature<SortInventory>
{
    private readonly ContainerHandler containerHandler;
    private readonly IInputHelper inputHelper;
    private readonly MenuManager menuManager;
    private readonly PerScreen<ClickableTextureComponent?> organizeButton = new();

    /// <summary>Initializes a new instance of the <see cref="SortInventory" /> class.</summary>
    /// <param name="containerHandler">Dependency used for handling operations between containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="menuManager">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public SortInventory(
        ContainerHandler containerHandler,
        IEventManager eventManager,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        MenuManager menuManager,
        IModConfig modConfig)
        : base(eventManager, log, manifest, modConfig)
    {
        this.containerHandler = containerHandler;
        this.inputHelper = inputHelper;
        this.menuManager = menuManager;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.SortInventory != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ContainerSortingEventArgs>(SortInventory.OnContainerSorting);
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        this.Events.Unsubscribe<ContainerSortingEventArgs>(SortInventory.OnContainerSorting);
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
    }

    private static void OnContainerSorting(ContainerSortingEventArgs e)
    {
        if (e.Container.Options.SortInventory is not FeatureOption.Enabled
            || string.IsNullOrWhiteSpace(e.Container.Options.SortInventoryBy))
        {
            return;
        }

        var itemSorter = new ItemSorter(e.Container.Options.SortInventoryBy);
        var copy = e.Container.Items.ToList();
        copy.Sort(itemSorter);
        e.Container.Items.OverwriteWith(copy);
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (!this.menuManager.CanFocus(this))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        var container = this.menuManager.CurrentMenu switch
        {
            ItemGrabMenu itemGrabMenu when itemGrabMenu.organizeButton?.containsPoint(mouseX, mouseY) == true =>
                this.menuManager.Top.Container,
            InventoryPage inventoryPage when inventoryPage.organizeButton?.containsPoint(mouseX, mouseY) == true =>
                this.menuManager.Bottom.Container,
            not null when this.organizeButton.Value?.containsPoint(mouseX, mouseY) == true => this.menuManager
                .Bottom.Container,
            _ => null,
        };

        if (container is null)
        {
            return;
        }

        switch (e.Button)
        {
            case SButton.MouseLeft or SButton.ControllerA:
                if (container.Options.SortInventory is not FeatureOption.Enabled)
                {
                    return;
                }

                this.inputHelper.Suppress(e.Button);
                Game1.playSound("Ship");
                this.containerHandler.Sort(container);
                return;

            case SButton.MouseRight or SButton.ControllerB:
                if (container.Options.SortInventory is not FeatureOption.Enabled)
                {
                    return;
                }

                this.inputHelper.Suppress(e.Button);
                Game1.playSound("Ship");
                this.containerHandler.Sort(container, true);
                return;
        }
    }

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        var container = this.menuManager.Bottom.Container;
        var bottom = this.menuManager.Bottom;

        if (this.menuManager.CurrentMenu is not ItemGrabMenu itemGrabMenu
            || bottom.Menu is null
            || container?.Options.SortInventory is not FeatureOption.Enabled)
        {
            this.organizeButton.Value = null;
            return;
        }

        // Add new organize button to the bottom inventory menu
        var x = itemGrabMenu.okButton.bounds.X;
        var y = itemGrabMenu.okButton.bounds.Y - Game1.tileSize - 16;
        this.organizeButton.Value = new ClickableTextureComponent(
            string.Empty,
            new Rectangle(x, y, Game1.tileSize, Game1.tileSize),
            string.Empty,
            Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"),
            Game1.mouseCursors,
            new Rectangle(162, 440, 16, 16),
            4f)
        {
            myID = (int)Math.Pow(y, 2) + x,
            upNeighborID = 5948,
            downNeighborID = 4857,
            leftNeighborID = 12,
        };

        itemGrabMenu.trashCan.bounds.Y -= Game1.tileSize;
        itemGrabMenu.okButton.upNeighborID = this.organizeButton.Value.myID;
        itemGrabMenu.trashCan.downNeighborID = this.organizeButton.Value.myID;
        itemGrabMenu.allClickableComponents.Add(this.organizeButton.Value);
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        if (this.organizeButton.Value is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        this.organizeButton.Value.tryHover(mouseX, mouseY);
        this.organizeButton.Value.draw(e.SpriteBatch);
        if (!this.organizeButton.Value.containsPoint(mouseX, mouseY))
        {
            return;
        }

        switch (this.menuManager.CurrentMenu)
        {
            case ItemGrabMenu itemGrabMenu:
                itemGrabMenu.hoverText = this.organizeButton.Value.hoverText;
                return;

            case InventoryPage inventoryPage:
                inventoryPage.hoverText = this.organizeButton.Value.hoverText;
                return;
        }
    }
}