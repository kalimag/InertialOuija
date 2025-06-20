extern alias GameScripts;

using GameScripts.Assets.Source.SaveData;
using InertialOuija.Utilities;
using Steamworks;
using UnityEngine;
using static InertialOuija.Configuration.ModConfig;

namespace InertialOuija.UI;

internal class DangerousOptionsWindow : Window
{
	const double QuotaThreshold = 0.1; // free space remaining

	protected override string Title => "DANGER ZONE";
	protected override Rect InitialPosition => new(200, 200, 500, 50);
	protected override Rect WindowPosition { get => _windowPosition; set => _windowPosition = value; }

	private static Rect _windowPosition;


	private readonly StringCache<(ulong Free, double Quota)> _steamCloudSpace = new(values => $"{FileUtility.FormatSize(values.Free)} free ({values.Quota * 100:0}%)");
	private readonly StringCache<int> _playerGhosts = new(value => FileUtility.FormatSize(value));

	private bool _confirmation;


	protected override void DrawWindow()
	{
		if (!GameScripts.SteamManager.Initialized)
			return;

		_confirmation = GUILayout.Toggle(_confirmation, "I promise I have a good reason for clicking these buttons");
		using var _ = Styles.Enable(_confirmation);

		GUILayout.Label("Player Ghost Database", Styles.Heading);
		GUILayout.Label("""
		Remove "PlayerGhosts" file from Steam Cloud synchronization to free up space.

		The file will still exist locally and will be added to the cloud again when playing the game without the DisablePlayerGhosts patch.
		""", Styles.MultilineWrapLabel);

		SteamRemoteStorage.GetQuota(out ulong totalSpace, out ulong freeSpace);
		int playerGhostSize = SteamRemoteStorage.FileExists(SaveFileKeys.PlayerGhostKey) ? SteamRemoteStorage.GetFileSize(SaveFileKeys.PlayerGhostKey) : 0;
		bool persisted = SteamRemoteStorage.FilePersisted(SaveFileKeys.PlayerGhostKey);
		double quota = (double)freeSpace / totalSpace;

		using (Styles.Row("Steam Cloud:"))
			GUILayout.Label(_steamCloudSpace.GetString((freeSpace, quota)));
		using (Styles.Row("Player Ghosts:"))
			GUILayout.Label(_playerGhosts.GetString(playerGhostSize));

		if (!SteamRemoteStorage.IsCloudEnabledForAccount() || !SteamRemoteStorage.IsCloudEnabledForApp())
			GUILayout.Label("Steam Cloud is not enabled", Styles.WarningLabel);
		if (!persisted)
			GUILayout.Label("Player Ghosts are not synced with Steam Cloud");
		else if (quota > QuotaThreshold)
			GUILayout.Label("Cloud isn't full", Styles.WarningLabel);
		else if (!Config.Patches.DisablePlayerGhosts)
			GUILayout.Label("DisablePlayerGhosts patch must be enabled", Styles.WarningLabel);

		using (Styles.Enable(persisted && quota <= QuotaThreshold && Config.Patches.DisablePlayerGhosts))
		{
			if (GUILayout.Button("Remove Player Ghosts from cloud"))
			{
				BackupUtility.CreateSteamBackup(SaveFileKeys.PlayerGhostKey);
				SteamRemoteStorage.FileForget(SaveFileKeys.PlayerGhostKey);
			}
		}

		Styles.Space();

		using (Styles.Horizontal())
		{
			GUILayout.FlexibleSpace();
			GUI.enabled = true;
			if (GUILayout.Button("Close", Styles.FixedButton))
				Close();
		}
	}
}
