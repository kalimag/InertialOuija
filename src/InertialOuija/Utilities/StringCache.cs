using System;
using System.Collections.Generic;
using System.Globalization;

namespace InertialOuija.Utilities;

internal sealed class StringCache<T> where T : IEquatable<T>
{
	private readonly Func<T, string> _formatString;
	private string _cachedString;
	private T _previousValue;

	public StringCache(Func<T, string> formatString) => _formatString = formatString;
	public StringCache() => _formatString = static value => value?.ToString() ?? "";
	public StringCache(string format) => _formatString = value => ((IFormattable)value)?.ToString(format, CultureInfo.CurrentCulture) ?? "";

	public string GetString(T value)
	{
		if (_cachedString == null || !EqualityComparer<T>.Default.Equals(value, _previousValue))
		{
			_cachedString = _formatString(value) ?? "";
			_previousValue = value;
		}
		return _cachedString;
	}
}