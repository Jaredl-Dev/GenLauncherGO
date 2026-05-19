using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GenLauncherGO.Core.Updating.Models;
using GenLauncherGO.Infrastructure.Updating.Contracts;
using GenLauncherGO.Infrastructure.Updating.Models;

namespace GenLauncherGO.Tests.Testing;

/// <summary>
///     Records single-file package updates, including the pause controller the caller forwarded, which is the only
///     evidence that a suspended download can be resumed.
/// </summary>
internal sealed class RecordingSingleFilePackageUpdater : ISingleFilePackageUpdater
{
    public List<(DownloadFileMetadata Metadata, PackageUpdatePathSet Paths)> Requests { get; } = [];

    public List<PackageDownloadPauseController?> PauseControllers { get; } = [];

    /// <summary>
    ///     Reported once from each update when set.
    /// </summary>
    public PackageUpdateProgress? ProgressToReport { get; init; }

    /// <summary>
    ///     Takes over the update body when set, for the cancellation and failure paths.
    /// </summary>
    public Func<DownloadFileMetadata, IProgress<PackageUpdateProgress>?, CancellationToken, Task>? Update { get; init; }

    public Task UpdateAsync(
        DownloadFileMetadata metadata,
        PackageUpdatePathSet paths,
        IProgress<PackageUpdateProgress>? progress,
        CancellationToken cancellationToken,
        PackageDownloadPauseController? pauseController = null)
    {
        Requests.Add((metadata, paths));
        PauseControllers.Add(pauseController);
        if (ProgressToReport is not null)
        {
            progress?.Report(ProgressToReport);
        }

        return Update?.Invoke(metadata, progress, cancellationToken) ?? Task.CompletedTask;
    }
}
