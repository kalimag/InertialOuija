extern alias GameScripts;

using Riok.Mapperly.Abstractions;
using SQLite;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace InertialOuija.Ghosts;

[Table(Table, WithoutRowId = true)]
public partial class ExternalGhostFile : ExternalGhostInfo
{
	internal const string Table = "Ghosts";

	[PrimaryKey]
	public required string Path { get; init; }
	public required long Size { get; init; }
	public required DateTimeOffset LastWrite { get; init; }

	[SetsRequiredMembers]
	private ExternalGhostFile(string path, DateTimeOffset lastWrite, long size)
	{
		Path = path;
		Size = size;
		LastWrite = lastWrite;
	}

	[Obsolete("For serialization only", true)]
	public ExternalGhostFile()
	{ }

	public static ExternalGhostFile FromInfo(string path, long size, DateTimeOffset lastWrite, ExternalGhostInfo info)
		=> Mapper.FromInfo(info, path, size, lastWrite);

	public static ExternalGhostFile FromInfo(FileInfo file, ExternalGhostInfo info)
		=> FromInfo(file.FullName, file.Length, file.LastWriteTimeUtc, info);



	public Task<ExternalGhost> LoadAsync()
	{
		return Task.Run(() => ExternalGhost.Load(Path));
	}



	[Mapper(PreferParameterlessConstructors = false)]
	private static partial class Mapper
	{
		public static partial ExternalGhostFile FromInfo(ExternalGhostInfo source, string path, long size, DateTimeOffset lastWrite);
	}
}
