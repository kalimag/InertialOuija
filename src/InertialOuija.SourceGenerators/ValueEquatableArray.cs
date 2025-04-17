using System.Collections;
using System.Collections.Immutable;

namespace InertialOuija.SourceGenerators;
internal readonly struct ValueEquatableArray<T>(ImmutableArray<T> values) : IReadOnlyList<T>, IEquatable<ValueEquatableArray<T>>
{
	public readonly ImmutableArray<T> Values = values;

	public int Count => Values.Length;

	public T this[int index] => Values[index];

	public bool Equals(ValueEquatableArray<T> other)
	{
		if (Count != other.Count)
			return false;

		for (int i = 0; i < Count; i++)
			if (!EqualityComparer<T>.Default.Equals(this[i], other[i]))
				return false;

		return true;
	}

	public override bool Equals(object obj) => obj is ValueEquatableArray<T> array && Equals(array);

	public override int GetHashCode() => -1521134295 * Count;

	public static implicit operator ValueEquatableArray<T>(ImmutableArray<T> values) => new(values);
	public static implicit operator ImmutableArray<T>(ValueEquatableArray<T> array) => array.Values;

	public ImmutableArray<T>.Enumerator GetEnumerator() => Values.GetEnumerator();
	IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)Values).GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Values).GetEnumerator();
}
