using System.Collections.Generic;
using System.Linq;

namespace InertialOuija;

internal static class UserData
{
	// Users with ghosts saved by a pre-release version that didn't include the SteamUserId
	private static readonly Dictionary<ulong, string> UserIdFallbacks = new()
	{
		[76561198065943770ul] = "Bonz",
		[76561198041430874ul] = "Sean A",
		[76561198014309406ul] = "kalimag",
	};
	private static readonly Dictionary<string, ulong> UsernameFallbacks = UserIdFallbacks.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

	public static string GetFallbackUsername(ulong userId)
	{
		if (UserIdFallbacks.TryGetValue(userId, out string username))
			return username;
		else
			return null;
	}

	public static ulong? GetFallbackUserId(string username)
	{
		if (UsernameFallbacks.TryGetValue(username, out ulong userId))
			return userId;
		else
			return null;
	}
}
