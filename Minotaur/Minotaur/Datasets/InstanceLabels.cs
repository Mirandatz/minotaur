namespace Minotaur.Datasets {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using ExtensionMethods.SystemArray;

	public sealed class InstanceLabels: IEquatable<InstanceLabels>, IReadOnlyList<bool> {

		private readonly bool[] _values;
		private readonly int _precomputedHashCode;

		public InstanceLabels(ReadOnlySpan<bool> values) {

			var storage = new bool[values.Length];
			var hash = new HashCode();

			for (int i = 0; i < values.Length; i++) {
				var v = values[i];
				storage[i] = v;
				hash.Add(v);
			}

			_values = storage;
			_precomputedHashCode = hash.ToHashCode();
		}

		// Views
		public ReadOnlySpan<bool> AsSpan() => _values.AsSpan();

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((InstanceLabels) obj!);

		// IEquatable
		public bool Equals([AllowNull] InstanceLabels other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (ReferenceEquals(this, other))
				return true;

			var lhs = this._values.AsSpan();
			var rhs = other._values.AsSpan();

			if (lhs.Length != rhs.Length)
				throw new InvalidOperationException();

			return lhs.SequenceEqual(rhs);
		}

		// IReadOnlyList
		public int Count => _values.Length;

		public bool this[int index] => _values[index];

		public IEnumerator<bool> GetEnumerator() => _values.GetGenericEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
	}
}
