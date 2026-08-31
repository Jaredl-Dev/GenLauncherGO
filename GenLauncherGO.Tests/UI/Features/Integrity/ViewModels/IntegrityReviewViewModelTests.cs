using System;
using GenLauncherGO.Core.Integrity.Models;
using GenLauncherGO.UI.Features.Integrity.ViewModels;

namespace GenLauncherGO.Tests.UI.Features.Integrity.ViewModels;

public sealed class IntegrityReviewViewModelTests
{
    [Fact]
    public void Constructor_GroupsIssuesByTargetAndMapsEntries()
    {
        ContentIntegrityIssue modifiedIssue = new(
            "mod:one",
            "Demo Mod",
            ContentSourceKind.ManagedS3,
            IntegrityIssueKind.ModifiedFile,
            IntegrityIssueAction.Repair,
            "Data/file.big",
            "Hash mismatch",
            2048);
        ContentIntegrityIssue addedIssue = new(
            "mod:one",
            "Demo Mod",
            ContentSourceKind.ManagedS3,
            IntegrityIssueKind.UnexpectedFile,
            IntegrityIssueAction.Delete,
            "Data/debug.txt",
            ExpectedSizeBytes: 4096);
        ContentIntegrityIssue unrepairedMissingIssue = new(
            "mod:one",
            "Demo Mod",
            ContentSourceKind.ManagedS3,
            IntegrityIssueKind.MissingFile,
            IntegrityIssueAction.Delete,
            "Data/stale.big",
            ExpectedSizeBytes: 4096);

        IntegrityReviewViewModel viewModel = new(
            new ContentIntegrityReport(new[] { modifiedIssue, addedIssue, unrepairedMissingIssue }),
            CreateLocalizer());

        viewModel.Description.Should().Be("Repair Demo Mod");
        viewModel.PrimaryActionText.Should().Be("Fix and launch");
        viewModel.IssueGroups.Should().ContainSingle();
        IntegrityReviewGroup group = viewModel.IssueGroups[0];
        group.TargetDisplayName.Should().Be("Demo Mod");
        group.Summary.Should().Be("3 changes");
        group.Entries.Should().Equal(
            new IntegrityReviewEntry(
                "Modified",
                "Data/file.big (2 KB) - Hash mismatch",
                "Repair"),
            new IntegrityReviewEntry(
                "Added",
                "Data/debug.txt",
                "Delete"),
            new IntegrityReviewEntry(
                "Missing",
                "Data/stale.big",
                "Delete"));
    }

    /// <summary>
    ///     An untracked package is an issue about the whole target, which the report marks with "." rather than a file
    ///     name. The row has to read as the absence of a path, not as a file called ".".
    /// </summary>
    [Fact]
    public void Constructor_WholeTargetIssue_RendersNoFileNameInTheRow()
    {
        ContentIntegrityIssue untrackedIssue = new(
            "mod:one",
            "Demo Mod",
            ContentSourceKind.ManagedS3,
            IntegrityIssueKind.Untracked,
            IntegrityIssueAction.Repair,
            ".");

        IntegrityReviewViewModel viewModel = new(
            new ContentIntegrityReport(new[] { untrackedIssue }),
            CreateLocalizer());

        viewModel.IssueGroups.Should().ContainSingle().Which.Entries.Should().Equal(
            new IntegrityReviewEntry("Untracked", string.Empty, "Repair"));
    }

    /// <summary>
    ///     A whole-target failure still has to show why it failed, so a message survives even when the path does not.
    /// </summary>
    [Fact]
    public void Constructor_WholeTargetIssueWithMessage_ShowsTheMessageAlone()
    {
        ContentIntegrityIssue verificationErrorIssue = new(
            "mod:one",
            "Demo Mod",
            ContentSourceKind.ManagedS3,
            IntegrityIssueKind.VerificationError,
            IntegrityIssueAction.Block,
            ".",
            "The snapshot could not be read.");

        IntegrityReviewViewModel viewModel = new(
            new ContentIntegrityReport(new[] { verificationErrorIssue }),
            CreateLocalizer());

        viewModel.IssueGroups.Should().ContainSingle().Which.Entries.Should().ContainSingle()
            .Which.RelativePath.Should().Be("The snapshot could not be read.");
    }

    [Fact]
    public void Constructor_CreatesSeparateGroupsForSeparateTargets()
    {
        ContentIntegrityIssue firstIssue = new(
            "mod:one",
            "Demo Mod",
            ContentSourceKind.ManagedS3,
            IntegrityIssueKind.MissingFile,
            IntegrityIssueAction.Repair,
            "Data/file.big");
        ContentIntegrityIssue secondIssue = new(
            "addon:one",
            "Demo Addon",
            ContentSourceKind.ManagedSingleFile,
            IntegrityIssueKind.UnexpectedFile,
            IntegrityIssueAction.Delete,
            "Data/addon.gib");

        IntegrityReviewViewModel viewModel = new(
            new ContentIntegrityReport(new[] { firstIssue, secondIssue }),
            CreateLocalizer());

        viewModel.IssueGroups.Should().HaveCount(2);
        viewModel.Description.Should().Be("Repair Demo Addon, Demo Mod");
        viewModel.PrimaryActionText.Should().Be("Fix and launch");
        viewModel.IssueGroups[0].TargetDisplayName.Should().Be("Demo Mod");
        viewModel.IssueGroups[0].Summary.Should().Be("1 change");
        viewModel.IssueGroups[1].TargetDisplayName.Should().Be("Demo Addon");
        viewModel.IssueGroups[1].Entries.Should().ContainSingle().Which.ActionLabel.Should().Be("Delete");
    }

    [Fact]
    public void ConfirmResolutionCommand_RaisesCloseRequestAndMarksResolutionConfirmed()
    {
        IntegrityReviewViewModel viewModel = new(
            new ContentIntegrityReport(Array.Empty<ContentIntegrityIssue>()),
            CreateLocalizer());
        bool closeRequested = false;
        viewModel.CloseRequested += (_, _) => closeRequested = true;

        viewModel.ConfirmResolutionCommand.Execute(null);

        closeRequested.Should().BeTrue();
        viewModel.ResolutionConfirmed.Should().BeTrue();
    }

    [Fact]
    public void CancelCommand_RaisesCloseRequestWithoutConfirmingResolution()
    {
        IntegrityReviewViewModel viewModel = new(
            new ContentIntegrityReport(Array.Empty<ContentIntegrityIssue>()),
            CreateLocalizer());
        bool closeRequested = false;
        viewModel.CloseRequested += (_, _) => closeRequested = true;

        viewModel.CancelCommand.Execute(null);

        closeRequested.Should().BeTrue();
        viewModel.ResolutionConfirmed.Should().BeFalse();
    }

    [Fact]
    public void Constructor_HidesPrimaryActionForBlockingReport()
    {
        ContentIntegrityIssue blockingIssue = new(
            "blocked",
            "Blocked Mod",
            ContentSourceKind.UnknownLegacy,
            IntegrityIssueKind.VerificationError,
            IntegrityIssueAction.Block,
            ".");
        IntegrityReviewViewModel viewModel = new(
            new ContentIntegrityReport(new[] { blockingIssue }),
            CreateLocalizer());

        viewModel.IsPrimaryActionVisible.Should().BeFalse();
        viewModel.Description.Should().Be("Remove blocked entries or restore content");
        viewModel.PrimaryActionText.Should().Be("Cancel launch");
        viewModel.CancelActionText.Should().Be("Cancel launch");
    }

    private static FakeStringLocalizer CreateLocalizer()
    {
        return FakeStringLocalizer.Create(TestLocalizedStrings.Integrity);
    }
}
