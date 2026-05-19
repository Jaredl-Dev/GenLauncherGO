using System.Collections.Generic;
using GenLauncherGO.Core.Mods.Contracts;
using GenLauncherGO.Core.Mods.Models;

namespace GenLauncherGO.Tests.Testing;

/// <summary>
///     Keeps cached palettes in memory so tests can assert what was cached without touching disk.
/// </summary>
internal sealed class FakeModificationThemeCache : IModificationThemeCache
{
    private readonly Dictionary<LauncherContentKey, LauncherContentTheme> _entries = [];

    public IReadOnlyDictionary<LauncherContentKey, LauncherContentTheme> Entries => _entries;

    public void Save(LauncherContentKey contentKey, LauncherContentTheme theme)
    {
        _entries[contentKey] = theme;
    }

    public LauncherContentTheme? Load(LauncherContentKey contentKey)
    {
        return _entries.TryGetValue(contentKey, out LauncherContentTheme? theme) ? theme : null;
    }
}
