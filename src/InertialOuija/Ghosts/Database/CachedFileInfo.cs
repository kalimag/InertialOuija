extern alias GameScripts;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using SQLite;

namespace InertialOuija.Ghosts.Database;

/// <summary>
/// Subset of <see cref="ExternalGhostFile"/> for query purposes
/// </summary>
[Table(ExternalGhostFile.Table, WithoutRowId = true)]
public class CachedFileInfo
{
	[PrimaryKey]
	public required string Path { get; init; }
	public required long Size { get; init; }
	public required DateTimeOffset LastWrite { get; init; }

	[SetsRequiredMembers]
	internal CachedFileInfo(FileInfo file)
	{
		Path = file.FullName;
		LastWrite = file.LastWriteTimeUtc;
		Size = file.Length;
	}

	[Obsolete("For serialization only", true)]
	public CachedFileInfo()
	{ }

#if DEBUG
	static CachedFileInfo()
	{
		if (nameof(Path) != nameof(ExternalGhostFile.Path) ||
			nameof(LastWrite) != nameof(ExternalGhostFile.LastWrite) ||
			nameof(Size) != nameof(ExternalGhostFile.Size))
			throw new Exception($"{nameof(CachedFileInfo)} members do not match {nameof(ExternalGhostFile)}");
	}
#endif
}

