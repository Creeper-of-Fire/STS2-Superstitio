using System.Reflection;
using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.ModSetting;
using FileAccess = Godot.FileAccess;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace Superstitio.Main.Features.SafeForWork;

public static class LocVariantManager
{
    public static bool IsInitLocVariantAfterModelDbInit { get; set; }
    
    public enum LocVariant
    {
        Sfw,
        Nsfw,
        Guro
    }

    private static readonly FieldInfo LocDictionaryField =
        AccessTools.Field(typeof(LocTable), "_translations");

    private static readonly MethodInfo TriggerLocaleChangeMethod =
        AccessTools.Method(typeof(LocManager), "TriggerLocaleChange");

    public static void ApplyVariant(LocVariant variant, string modName)
    {
        string language = LocManager.Instance.Language;

        string variantDir = $"res://{modName}/localization/{language}/{variant.ToString().ToLowerInvariant()}";

        if (!DirAccess.DirExistsAbsolute(variantDir))
        {
            Log.Info($"[LocVariant] Variant directory not found: {variantDir}");
            return;
        }

        using var dir = DirAccess.Open(variantDir);
        if (dir is null)
        {
            Log.Error($"[LocVariant] Failed to open directory: {variantDir}");
            return;
        }

        foreach (string file in dir.GetFiles())
        {
            if (!file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                continue;

            string tableName = Path.GetFileNameWithoutExtension(file);

            LocTable locTable;
            try
            {
                locTable = LocManager.Instance.GetTable(tableName);
            }
            catch (LocException exception)
            {
                Log.Error(exception.Message);
                continue;
            }

            var dict = LocDictionaryField.GetValue(locTable) as Dictionary<string, string>
                       ?? throw new Exception("Failed to get localization dictionary.");

            string filePath = $"{variantDir}/{file}";
            if (!LoadAndMergeJson(filePath, dict))
                continue;

            Log.Info($"[LocVariant] Applied overrides to '{tableName}' from {filePath}");
        }

        TriggerLocaleChangeMethod.Invoke(LocManager.Instance, null);
    }

    private static bool LoadAndMergeJson(string path, Dictionary<string, string> targetDict)
    {
        if (!FileAccess.FileExists(path))
            return false;

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file is null)
            return false;

        try
        {
            string json = file.GetAsText();
            var overrideDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (overrideDict is null)
                return false;

            foreach (var entry in overrideDict)
            {
                targetDict[entry.Key] = entry.Value;
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"[LocVariant] Failed to parse {path}: {ex.Message}");
            return false;
        }
    }
}

[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.Init))]
static class ModelDbInit_LocVariantPatch
{
    [HarmonyPostfix]
    static void Postfix()
    {
        LocVariantManager.IsInitLocVariantAfterModelDbInit = true;
        SuperstitioModConfig.ApplyLocVariant();
    }
}