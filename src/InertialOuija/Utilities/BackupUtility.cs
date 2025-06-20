using System;
using System.IO;

namespace InertialOuija.Utilities;
internal static class BackupUtility
{
	private static string BackupDirectory => Path.Combine(FileUtility.ModDirectory, "backups");

	public static void CreateBackup(string name, byte[] data)
	{
		using var file = FileUtility.CreateUniqueFile(BackupDirectory, GenerateName(name), "");
		file.Write(data, 0, data.Length);
	}

	public static void CreateSteamBackup(string name)
	{
		var data = SteamUtility.ReadFile(name);
		if (data is null)
			return;
		CreateBackup(name, data);
	}

	private static string GenerateName(string name) => $"{FileUtility.Sanitize(name)} ({DateTime.Now:yyyy-MM-dd HH-mm-ss})";
}
