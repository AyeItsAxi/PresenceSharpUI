using System;
using System.IO;
using Newtonsoft.Json;
using PresenceSharpUI.Models;

namespace PresenceSharpUI.Services;

public static class PresencePreferencesService
{
    private static readonly string AppDataRoot =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PresenceSharp",
            "UI");

    private static readonly string PreferencesPath =
        Path.Combine(AppDataRoot, "UserPreferences.json");

    public static void EnsureExists()
    {
        Directory.CreateDirectory(AppDataRoot);

        if (!File.Exists(PreferencesPath))
            Save(CreateDefault());
    }

    public static PsuiUserData Load()
    {
        var data = JsonConvert.DeserializeObject<PsuiUserData>(
            File.ReadAllText(PreferencesPath))!;

        return Sanitize(data);
    }

    public static void Save(PsuiUserData userData)
    {
        File.WriteAllText(
            PreferencesPath,
            JsonConvert.SerializeObject(userData));
    }

    private static PsuiUserData CreateDefault()
    {
        return new PsuiUserData
        {
            I64ApplicationId = 1061800604051189830,
            StrTitle = "This is an example title",
            StrSubtitle = "This is an example subtitle",
            StrLargeImageName = "appicon",
            StrLargeImageText = "Example text",
            StrSmallImageName = "appicon",
            StrSmallImageText = "Example text"
        };
    }
    
    private static PsuiUserData Sanitize(PsuiUserData data)
    {
        return new PsuiUserData
        {
            I64ApplicationId = data.I64ApplicationId,
            StrTitle = data.StrTitle ?? string.Empty,
            StrSubtitle = data.StrSubtitle ?? string.Empty,
            StrLargeImageName = data.StrLargeImageName ?? string.Empty,
            StrLargeImageText = data.StrLargeImageText ?? string.Empty,
            StrSmallImageName = data.StrSmallImageName ?? string.Empty,
            StrSmallImageText = data.StrSmallImageText ?? string.Empty
        };
    }
}