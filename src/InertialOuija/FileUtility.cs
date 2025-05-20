using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Doorstop;

namespace InertialOuija;

internal static class FileUtility
{
	public static string ModDirectory { get; } = Path.GetDirectoryName(Path.GetFullPath(typeof(Entrypoint).Assembly.Location));
	public static string GameDirectory { get; } = Path.GetDirectoryName(GameData.DataPath ?? ModDirectory);

	public static FileStream CreateUniqueFile(string directory, string fileName, string extension, bool overwriteFirst = false, uint maxTries = 100)
	{
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);

		string fullPath = Path.Combine(directory, fileName + extension);
		for (uint i = 1; i <= maxTries; i++)
		{
			if (overwriteFirst || !File.Exists(fullPath))
			{
				try
				{
					var stream = new FileStream(PrefixLongPath(fullPath), overwriteFirst ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None);
					return stream;
				}
				catch (IOException ex)
				{
					// try to only catch file exists/file in use exceptions
					if (ex.GetType() != typeof(IOException) || !File.Exists(fullPath))
						throw;
				}
			}
			overwriteFirst = false;
			fullPath = Path.Combine(directory, $"{fileName} ({i + 1}){extension}");
		}
		throw new IOException($"Unable to find unused name for \"{directory}\\{fileName}{extension}\"");
	}


	private static readonly Regex InvalidPathCharsRegex = new(@"[\p{C}" +
		Regex.Escape(string.Concat(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).Distinct()))
		+ "]");
	private static readonly Regex WhitespaceRegex = new(@"[\s]+");
	public static string Sanitize(string name, int? maxLength = null)
	{
		if (maxLength < 3)
			throw new ArgumentOutOfRangeException(nameof(maxLength));

		name = name.Trim();
		name = WhitespaceRegex.Replace(name, " ");
		name = InvalidPathCharsRegex.Replace(name, "-");

		if (name.Length > maxLength)
			name = name.Substring(0, (int)maxLength - 3) + "...";

		return name;
	}

	public static string PrefixLongPath(string path)
	{
		const int MaxPath = 260 - 1; // plus null terminator
		const string Prefix = @"\\?\";
		path = Path.GetFullPath(path);
		if (path.Length <= MaxPath || path.StartsWith(Prefix) || Environment.OSVersion.Platform != PlatformID.Win32NT)
			return path;
		else
			return Prefix + path;
	}
}