using System;
using System.Linq;
using GenLauncherGO.Core.Mods.Models;

namespace GenLauncherGO.UI.Features.Mods;

internal sealed class ModificationVersionSelection
{
    public ModificationVersionSelection(
        LauncherContentVersion selectedVersion,
        ModificationViewModel modificationViewModel)
    {
        ArgumentNullException.ThrowIfNull(selectedVersion);
        ArgumentNullException.ThrowIfNull(modificationViewModel);
        if (!modificationViewModel.ContainerModification.Versions.Any(version =>
                version.ContentKey == selectedVersion.ContentKey))
        {
            throw new ArgumentException(
                "A selected version must belong to its modification tile.",
                nameof(selectedVersion));
        }

        SelectedVersion = selectedVersion;
        ModificationViewModel = modificationViewModel;
    }

    public string VersionName => SelectedVersion.Version;

    public LauncherContentVersion SelectedVersion { get; }

    public ModificationViewModel ModificationViewModel { get; }
}
