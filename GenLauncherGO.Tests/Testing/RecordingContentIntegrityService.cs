using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GenLauncherGO.Core.Integrity.Models;
using GenLauncherGO.Core.Startup;
using GenLauncherGO.Infrastructure.Integrity.Contracts;

namespace GenLauncherGO.Tests.Testing;

internal sealed class RecordingContentIntegrityService : IContentIntegrityService
{
    public ContentIntegrityReport VerificationReport { get; set; } =
        new(Array.Empty<ContentIntegrityIssue>());

    public bool CaptureIfMatchesExpectedFileSetResult { get; set; }

    public int PruneSnapshotsResult { get; set; }

    public List<IReadOnlyList<ContentIntegrityTarget>> VerifiedTargetSets { get; } = [];

    public List<LauncherPaths> VerifiedPaths { get; } = [];

    public List<(
        ContentIntegrityTarget Target,
        IReadOnlySet<string> ExpectedRelativePaths)> ConditionalSnapshotRequests
    { get; } = [];

    public List<LauncherPaths> ConditionalSnapshotPaths { get; } = [];

    public List<ContentIntegrityTarget> CapturedTargets { get; } = [];

    public List<LauncherPaths> CapturedPaths { get; } = [];

    public List<IReadOnlySet<string>> RetainedTargetIdSets { get; } = [];

    public List<LauncherPaths> PrunedPaths { get; } = [];

    public List<(
        ContentIntegrityReport Report,
        IReadOnlyList<ContentIntegrityTarget> Targets)> CleanupRequests
    { get; } = [];

    /// <summary>
    ///     Names every observed call in order, so a test can pin that cleanup runs before a repair rewrites files.
    /// </summary>
    public List<string> Calls { get; } = [];

    public Task<ContentIntegrityReport> VerifyAsync(
        LauncherPaths paths,
        IReadOnlyList<ContentIntegrityTarget> targets,
        CancellationToken cancellationToken)
    {
        Calls.Add("verify");
        VerifiedPaths.Add(paths);
        VerifiedTargetSets.Add(targets);
        return Task.FromResult(VerificationReport);
    }

    public Task<bool> CaptureSnapshotIfMatchesExpectedFileSetAsync(
        LauncherPaths paths,
        ContentIntegrityTarget target,
        IReadOnlySet<string> expectedRelativePaths,
        CancellationToken cancellationToken)
    {
        Calls.Add("conditional-capture");
        ConditionalSnapshotPaths.Add(paths);
        ConditionalSnapshotRequests.Add((target, expectedRelativePaths));
        return Task.FromResult(CaptureIfMatchesExpectedFileSetResult);
    }

    public Task CaptureSnapshotAsync(
        LauncherPaths paths,
        ContentIntegrityTarget target,
        CancellationToken cancellationToken)
    {
        Calls.Add("capture");
        CapturedPaths.Add(paths);
        CapturedTargets.Add(target);
        return Task.CompletedTask;
    }

    public int PruneSnapshots(LauncherPaths paths, IReadOnlySet<string> retainedTargetIds)
    {
        Calls.Add("prune");
        PrunedPaths.Add(paths);
        RetainedTargetIdSets.Add(retainedTargetIds);
        return PruneSnapshotsResult;
    }

    public Task ApplyCleanupAsync(
        ContentIntegrityReport report,
        IReadOnlyList<ContentIntegrityTarget> targets,
        CancellationToken cancellationToken)
    {
        Calls.Add("cleanup");
        CleanupRequests.Add((report, targets));
        return Task.CompletedTask;
    }
}
