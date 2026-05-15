namespace PresenceSharpUI.Models;

public sealed class PsuiUserData
{
    public long I64ApplicationId { get; init; }
    public string StrTitle { get; init; } = string.Empty;
    public string StrSubtitle { get; init; } = string.Empty;
    public string StrLargeImageName { get; init; } = string.Empty;
    public string StrLargeImageText { get; init; } = string.Empty;
    public string StrSmallImageName { get; init; } = string.Empty;
    public string StrSmallImageText { get; init; } = string.Empty;
}