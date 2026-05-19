using System;
using GenLauncherGO.Core.Integrity.Models;
using GenLauncherGO.Core.Mods.Models;

namespace GenLauncherGO.Tests.Core.Mods.Models;

public sealed class LauncherContentTests
{
    [Fact]
    public void AddOrUpdate_KeepsOneCanonicalVersionAndCombinesLocalStateWithRemoteMetadata()
    {
        var localState = new LauncherContentInstallation
        {
            Installed = true,
            ContentSourceKind = ContentSourceKind.UnknownLegacy
        };
        var localVersion = new LauncherContentVersion(localState)
        {
            Name = "ShockWave",
            Version = "1.2",
            ModificationType = ModificationType.Mod,
            SimpleDownloadLink = "https://example.test/local-package.zip",
            ModDBLink = "https://example.test/local-moddb",
            S3BucketName = "local-mods"
        };
        var remoteState = new LauncherContentInstallation
        {
            IsSelected = true,
            ContentSourceKind = ContentSourceKind.ManagedSingleFile
        };
        var remoteVersion = new LauncherContentVersion(remoteState)
        {
            Name = "shockwave",
            Version = "1.2",
            ModificationType = ModificationType.Mod,
            SimpleDownloadLink = "https://example.test/package.zip",
            ModDBLink = "https://example.test/moddb",
            S3BucketName = "remote-mods"
        };

        LauncherContent content = TestLauncherContent.From(localVersion, remoteVersion);

        LauncherContentVersion merged = content.Versions.Should().ContainSingle().Which;
        merged.ContentKey.Should().Be(localVersion.ContentKey);
        merged.Name.Should().Be("ShockWave");
        merged.SimpleDownloadLink.Should().Be("https://example.test/local-package.zip");
        merged.ModDBLink.Should().Be("https://example.test/local-moddb");
        merged.S3BucketName.Should().Be("local-mods");
        merged.Installation.Should().BeSameAs(localState);
        merged.Installation.Installed.Should().BeTrue();
        merged.Installation.IsSelected.Should().BeTrue();
        merged.EffectiveContentSourceKind.Should().Be(ContentSourceKind.ManagedSingleFile);
        content.IsSelected.Should().BeTrue();
        content.Installed.Should().BeTrue();
    }

    [Fact]
    public void AddOrUpdate_WhenLocalStateIsUnclassified_AdoptsTheIncomingSourceKind()
    {
        LauncherContentVersion localVersion = TestLauncherContent.Version(
            version: "1.2",
            installed: true,
            sourceKind: ContentSourceKind.UnknownLegacy);
        LauncherContentVersion remoteVersion = TestLauncherContent.Version(
            version: "1.2",
            sourceKind: ContentSourceKind.Manual);

        LauncherContent content = TestLauncherContent.From(localVersion, remoteVersion);

        content.Versions.Should().ContainSingle()
            .Which.EffectiveContentSourceKind.Should().Be(ContentSourceKind.Manual);
    }

    [Fact]
    public void AddOrUpdate_WhenLocalStateIsAlreadyClassified_KeepsItOverAnUnclassifiedIncoming()
    {
        LauncherContentVersion localVersion = TestLauncherContent.Version(
            version: "1.2",
            installed: true,
            sourceKind: ContentSourceKind.Manual);
        LauncherContentVersion remoteVersion = TestLauncherContent.Version(
            version: "1.2",
            sourceKind: ContentSourceKind.UnknownLegacy);

        LauncherContent content = TestLauncherContent.From(localVersion, remoteVersion);

        content.Versions.Should().ContainSingle()
            .Which.EffectiveContentSourceKind.Should().Be(ContentSourceKind.Manual);
    }

    [Fact]
    public void AddOrUpdate_WhenBothStatesAreClassified_KeepsTheLocalClassification()
    {
        LauncherContentVersion localVersion = TestLauncherContent.Version(
            version: "1.2",
            installed: true,
            sourceKind: ContentSourceKind.Manual);
        LauncherContentVersion remoteVersion = TestLauncherContent.Version(
            version: "1.2",
            sourceKind: ContentSourceKind.ManagedSingleFile);

        LauncherContent content = TestLauncherContent.From(localVersion, remoteVersion);

        content.Versions.Should().ContainSingle()
            .Which.EffectiveContentSourceKind.Should().Be(ContentSourceKind.Manual);
    }

    [Fact]
    public void AddOrUpdate_WhenLocalRecordHasNoCatalogMetadata_PreservesEveryIncomingFieldAndLocalState()
    {
        var localInstallation = new LauncherContentInstallation
        {
            Installed = true,
            ContentSourceKind = ContentSourceKind.UnknownLegacy
        };
        var localVersion = new LauncherContentVersion(localInstallation)
        {
            ModificationType = ModificationType.Patch,
            Name = "Patch",
            Version = "2.0",
            ParentContentName = "ShockWave"
        };
        var theme = new LauncherContentTheme { GenLauncherActiveColor = "#FF123456" };
        var incomingVersion = new LauncherContentVersion
        {
            ModificationType = ModificationType.Patch,
            Name = "Patch",
            Version = "2.0",
            ParentContentName = "ShockWave",
            SimpleDownloadLink = "https://example.test/package.zip",
            UIImageSourceLink = "https://example.test/image.png",
            DiscordLink = "https://example.test/discord",
            ModDBLink = "https://example.test/moddb",
            NewsLink = "https://example.test/news",
            S3HostLink = "https://s3.example.test",
            S3BucketName = "packages",
            S3FolderName = "shockwave/patch/2.0",
            S3HostPublicKey = "public-key",
            S3HostSecretKey = "secret-key",
            NetworkInfo = "Community service required.",
            Deprecated = true,
            SupportLink = "https://example.test/support",
            Theme = theme
        };

        LauncherContent content = TestLauncherContent.From(localVersion, incomingVersion);

        LauncherContentVersion merged = content.Versions.Should().ContainSingle().Which;
        merged.Should().BeEquivalentTo(
            incomingVersion,
            options => options.Excluding(version => version.Installation));
        merged.Installation.Should().BeSameAs(localInstallation);
        merged.Installation.Installed.Should().BeTrue();
        merged.EffectiveContentSourceKind.Should().Be(ContentSourceKind.ManagedS3);
    }

    [Fact]
    public void AddOrUpdate_WhenRemoteMetadataRefreshesExistingVersion_KeepsPublishedTheme()
    {
        LauncherContentVersion localVersion = TestLauncherContent.Version(version: "1.2", installed: true);
        var publishedTheme = new LauncherContentTheme { GenLauncherActiveColor = "#FF123456" };
        LauncherContentVersion remoteVersion = TestLauncherContent.Version(version: "1.2", theme: publishedTheme);

        LauncherContent content = TestLauncherContent.From(localVersion, remoteVersion);

        content.Versions.Should().ContainSingle().Which.Theme.Should().BeSameAs(publishedTheme);
    }

    [Fact]
    public void AddOrUpdate_WhenRemoteMetadataCarriesNoTheme_KeepsTheAlreadyPublishedTheme()
    {
        var publishedTheme = new LauncherContentTheme { GenLauncherActiveColor = "#FF123456" };
        LauncherContentVersion localVersion = TestLauncherContent.Version(
            version: "1.2",
            installed: true,
            theme: publishedTheme);
        LauncherContentVersion remoteVersion = TestLauncherContent.Version(version: "1.2");

        LauncherContent content = TestLauncherContent.From(localVersion, remoteVersion);

        content.Versions.Should().ContainSingle().Which.Theme.Should().BeSameAs(publishedTheme);
    }

    [Fact]
    public void AddOrUpdate_WhenBothVersionsPublishATheme_KeepsTheRemoteTheme()
    {
        var localTheme = new LauncherContentTheme { GenLauncherActiveColor = "#FF111111" };
        var remoteTheme = new LauncherContentTheme { GenLauncherActiveColor = "#FF222222" };
        LauncherContentVersion localVersion = TestLauncherContent.Version(
            version: "1.2",
            installed: true,
            theme: localTheme);
        LauncherContentVersion remoteVersion = TestLauncherContent.Version(version: "1.2", theme: remoteTheme);

        LauncherContent content = TestLauncherContent.From(localVersion, remoteVersion);

        content.Versions.Should().ContainSingle().Which.Theme.Should().BeSameAs(remoteTheme);
    }

    [Fact]
    public void AddOrMergeVersion_WithVersionFromAnotherCard_Throws()
    {
        LauncherContent content = TestLauncherContent.From(TestLauncherContent.Version(version: "1.2"));
        LauncherContentVersion otherCardVersion = TestLauncherContent.Version("Rise Of The Reds", "1.2");

        Action act = () => content.AddOrMergeVersion(otherCardVersion);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("version");
    }

    [Fact]
    public void LatestVersion_IsTheCardPresentationMetadataAuthority()
    {
        LauncherContentVersion earliest = new()
        {
            Name = "ShockWave",
            Version = "1.0",
            ModificationType = ModificationType.Mod,
            SupportLink = "https://example.test/old"
        };
        LauncherContentVersion latest = new()
        {
            Name = "ShockWave",
            Version = "2.0",
            ModificationType = ModificationType.Mod,
            SupportLink = "https://example.test/current"
        };

        LauncherContent content = TestLauncherContent.From(earliest, latest);

        content.LatestVersion.Should().BeSameAs(latest);
        content.LatestVersion.SupportLink.Should().Be("https://example.test/current");
    }

    [Fact]
    public void SelectedVersion_UsesPersistedInstalledSelectionBeforeFallbacks()
    {
        LauncherContentVersion earliestInstalled = TestLauncherContent.Version(version: "1.0", installed: true);
        LauncherContentVersion selectedInstalled = TestLauncherContent.Version(
            version: "2.0",
            installed: true,
            isSelected: true);
        LauncherContentVersion latestRemote = TestLauncherContent.Version(version: "3.0", isSelected: true);
        LauncherContent content = TestLauncherContent.From(
            earliestInstalled,
            latestRemote,
            selectedInstalled);

        LauncherContentVersion? selectedVersion = content.GetSelectedVersion();

        selectedVersion.Should().BeSameAs(selectedInstalled);
    }

    [Fact]
    public void SelectedVersion_WithoutASavedSelection_PrefersInstalledOverEarlierCatalogVersions()
    {
        LauncherContentVersion earliestCatalogVersion = TestLauncherContent.Version(version: "1.0");
        LauncherContentVersion earliestInstalled = TestLauncherContent.Version(version: "2.0", installed: true);
        LauncherContentVersion laterInstalled = TestLauncherContent.Version(version: "3.0", installed: true);
        LauncherContent content = TestLauncherContent.From(
            earliestCatalogVersion,
            earliestInstalled,
            laterInstalled);

        LauncherContentVersion? selectedVersion = content.GetSelectedVersion();

        selectedVersion.Should().BeSameAs(earliestInstalled);
    }

    [Fact]
    public void SelectedVersion_FallsBackToEarliestInstalledThenEarliestKnownVersion()
    {
        LauncherContentVersion latestRemote = TestLauncherContent.Version(version: "3.0");
        LauncherContentVersion earliestInstalled = TestLauncherContent.Version(version: "1.0", installed: true);
        LauncherContentVersion laterInstalled = TestLauncherContent.Version(version: "2.5", installed: true);
        LauncherContentVersion middleRemote = TestLauncherContent.Version(version: "2.0");
        LauncherContent installedContent = TestLauncherContent.From(
            latestRemote,
            earliestInstalled,
            laterInstalled,
            middleRemote);
        LauncherContent remoteContent = TestLauncherContent.From(latestRemote, middleRemote);

        installedContent.GetSelectedVersion().Should().BeSameAs(earliestInstalled);
        remoteContent.GetSelectedVersion().Should().BeSameAs(middleRemote);
    }

    [Fact]
    public void LatestInstalledVersion_UsesCanonicalVersionOrdering()
    {
        LauncherContentVersion latestInstalled = TestLauncherContent.Version(version: "2.0", installed: true);
        LauncherContentVersion earliestInstalled = TestLauncherContent.Version(version: "1.0", installed: true);
        LauncherContentVersion remoteUpdate = TestLauncherContent.Version(version: "3.0");
        LauncherContent content = TestLauncherContent.From(
            latestInstalled,
            remoteUpdate,
            earliestInstalled);

        content.LatestInstalledVersion.Should().BeSameAs(latestInstalled);
    }
}
