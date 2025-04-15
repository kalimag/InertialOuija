extern alias GameScripts;
using System;

namespace InertialOuija.Ghosts;

public readonly record struct GhostTime(float TimeInSeconds)
{
	public TimeSpan Time => TimeSpan.FromSeconds(TimeInSeconds);

	/// <remarks>May be greater than 59</remarks>
	public int Minutes => (int)Time.TotalMinutes;
	public int Seconds => Time.Seconds;
	/// <see cref="GameScripts.Assets.Source.Tools.StringTools.SecondsToTimeString" />
	public int Hundreds => Time.Milliseconds / 10;
	public int Thousands => Time.Milliseconds;

	public string ToString(bool thousands, string separator)
	{
		if (thousands)
			return $"{Minutes:00}{separator}{Seconds:00}{separator}{Thousands:000}";
		else
			return $"{Minutes:00}{separator}{Seconds:00}{separator}{Hundreds:00}";
	}

	public string ToString(bool thousands) => ToString(thousands, ":");
	public override string ToString() => ToString(true);
}
