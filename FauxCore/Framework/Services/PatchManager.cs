namespace StardewMods.FauxCore.Framework.Services;

using HarmonyLib;
using StardewMods.FauxCore.Common.Enums;
using StardewMods.FauxCore.Common.Services;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;

/// <inheritdoc cref="IPatchManager" />
internal sealed class PatchManager : BaseService<PatchManager>, IPatchManager
{
    private readonly HashSet<string> appliedPatches = [];
    private readonly Harmony harmony;
    private readonly Dictionary<string, List<ISavedPatch>> savedPatches = new();

    /// <summary>Initializes a new instance of the <see cref="PatchManager" /> class.</summary>
    /// <param name="modInfo">Dependency used for accessing mod info.</param>
    public PatchManager(IModInfo modInfo) => this.harmony = new Harmony(modInfo.Manifest.UniqueID);

    /// <inheritdoc />
    public void Add(string id, params ISavedPatch[] patches)
    {
        if (!this.savedPatches.TryGetValue(id, out var list))
        {
            list = new List<ISavedPatch>();
            this.savedPatches.Add(id, list);
        }

        list.AddRange(patches);
    }

    /// <inheritdoc />
    public void Patch(string id)
    {
        if (this.appliedPatches.Contains(id) || !this.savedPatches.TryGetValue(id, out var patches))
        {
            return;
        }

        this.appliedPatches.Add(id);
        foreach (var patch in patches)
        {
            try
            {
                Log.Trace(
                    "Patching {0}.{1} with {2}.{3} {4}.",
                    patch.Original.DeclaringType!.Name,
                    patch.Original.Name,
                    patch.Patch.DeclaringType!.Name,
                    patch.Patch.Name,
                    patch.Type.ToStringFast());

                switch (patch.Type)
                {
                    case PatchType.Prefix:
                        this.harmony.Patch(patch.Original, new HarmonyMethod(patch.Patch));
                        continue;
                    case PatchType.Postfix:
                        this.harmony.Patch(patch.Original, postfix: new HarmonyMethod(patch.Patch));
                        continue;
                    case PatchType.Transpiler:
                        this.harmony.Patch(patch.Original, transpiler: new HarmonyMethod(patch.Patch));
                        continue;
                    case PatchType.Finalizer:
                        this.harmony.Patch(patch.Original, finalizer: new HarmonyMethod(patch.Patch));
                        continue;
                }
            }
            catch (Exception e)
            {
                Log.Warn(
                    "Patching {0} failed with.\nError: {1}",
                    patch.LogId ?? $"{patch.Original.DeclaringType!.Name}.{patch.Original.Name}",
                    e.Message);
            }
        }
    }

    /// <inheritdoc />
    public void Unpatch(string id)
    {
        if (!this.appliedPatches.Contains(id) || !this.savedPatches.TryGetValue(id, out var patches))
        {
            return;
        }

        this.appliedPatches.Remove(id);
        foreach (var patch in patches)
        {
            Log.Trace("Unpatching {0} with {1}.", patch.Original.Name, patch.Patch.Name);
            this.harmony.Unpatch(patch.Original, patch.Patch);
        }
    }
}