using System;
using System.Linq;
using GenLauncherGO.Core.Integrity.Models;
using GenLauncherGO.Core.Mods.Contracts;
using GenLauncherGO.Core.Mods.Models;
using GenLauncherGO.UI.Features.Launcher.Models;
using GenLauncherGO.UI.Features.Mods;

namespace GenLauncherGO.UI.Features.Launcher.Services;

/// <summary>
///     Builds and applies actions for launcher modification tiles.
/// </summary>
internal sealed class LauncherTileActionService
{
    private readonly ILauncherContentCatalog _catalog;

    public LauncherTileActionService(ILauncherContentCatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    public static LauncherTileLinkAction GetAdvertisingDownloadAction(LauncherContent modification)
    {
        ArgumentNullException.ThrowIfNull(modification);

        if (modification.ModificationType == ModificationType.Advertising &&
            !string.IsNullOrEmpty(modification.LatestVersion.SimpleDownloadLink))
        {
            return new LauncherTileLinkAction(
                modification.LatestVersion.SimpleDownloadLink,
                true);
        }

        return new LauncherTileLinkAction(null, false);
    }

    /// <summary>
    ///     Builds the action for one of the external links on a modification tile.
    /// </summary>
    /// <remarks>
    ///     Advertising tiles thank the user for every link they follow, because those tiles exist to send traffic to
    ///     another project. Support links thank regardless of tile type, since they lead to the author's donation page.
    /// </remarks>
    public static LauncherTileLinkAction GetLinkAction(LauncherContent modification, LauncherTileLinkKind kind)
    {
        ArgumentNullException.ThrowIfNull(modification);

        bool isAdvertising = modification.ModificationType == ModificationType.Advertising;
        (string? Uri, bool ShowThankYouMessage) link = kind switch
        {
            LauncherTileLinkKind.ChangeLog => (modification.LatestVersion.NewsLink, isAdvertising),
            LauncherTileLinkKind.NetworkInfo => (modification.LatestVersion.NetworkInfo, isAdvertising),
            LauncherTileLinkKind.Support => (modification.LatestVersion.SupportLink, true),
            LauncherTileLinkKind.ModDb => (modification.LatestVersion.ModDBLink, false),
            LauncherTileLinkKind.Discord => (modification.LatestVersion.DiscordLink, false),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "A tile link kind is required.")
        };

        return new LauncherTileLinkAction(
            string.IsNullOrEmpty(link.Uri) ? null : link.Uri,
            link.ShowThankYouMessage);
    }

    /// <summary>
    ///     Deletes a selected installed version from the catalog and refreshes local modification data.
    /// </summary>
    public bool DeleteVersion(ModificationVersionSelection versionData)
    {
        ArgumentNullException.ThrowIfNull(versionData);

        LauncherContentKey contentKey = versionData.SelectedVersion.ContentKey;

        if (ShouldRemoveContentCard(versionData))
        {
            _catalog.DiscardContent(contentKey);
            return true;
        }

        _catalog.UninstallVersion(contentKey);
        return false;
    }

    /// <summary>
    ///     Deletes local files for a content tile and removes its latest version from the catalog.
    /// </summary>
    public void DiscardContentVersion(ModificationViewModel modification)
    {
        ArgumentNullException.ThrowIfNull(modification);

        _catalog.DiscardVersion(modification.LatestVersion.ContentKey);
    }

    /// <summary>
    ///     Deletes local files for a content tile while keeping its catalog entry.
    /// </summary>
    public void UninstallContentVersion(ModificationViewModel modification)
    {
        ArgumentNullException.ThrowIfNull(modification);

        _catalog.UninstallVersion(modification.LatestVersion.ContentKey);
    }

    /// <summary>
    ///     Determines whether deleting the selected version should remove the whole visible content card.
    /// </summary>
    /// <remarks>
    ///     Removing the card deletes every version installed beneath it, so a mod or manual import only takes its card
    ///     with it once the selected version is the last one installed. While another installed version remains, the
    ///     selected version is uninstalled on its own and the card keeps the rest.
    /// </remarks>
    private static bool ShouldRemoveContentCard(ModificationVersionSelection versionData)
    {
        LauncherContentVersion selectedVersion = versionData.SelectedVersion;
        if (selectedVersion.ModificationType != ModificationType.Mod &&
            selectedVersion.EffectiveContentSourceKind != ContentSourceKind.Manual)
        {
            return false;
        }

        return !versionData.ModificationViewModel.ContainerModification.Versions.Any(version =>
            version.ContentKey != selectedVersion.ContentKey &&
            version.Installation.Installed);
    }
}
