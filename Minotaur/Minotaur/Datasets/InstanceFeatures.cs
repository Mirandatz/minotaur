namespace Minotaur.Datasets {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.ExtensionMethods.SystemArray;

	public sealed class InstanceFeatures: IEquatable<InstanceFeatures>, IReadOnlyList<float> {

		private readonly float[] _values;
		private readonly int _precomputedHashCode;

		public InstanceFeatures(ReadOnlySpan<float> values) {

			var storage = new float[values.Length];
			var hash = new HashCode();

			for (int i = 0; i < values.Length; i++) {
				var v = values[i];

				if (float.IsNaN(v))
					throw new ArgumentException(nameof(values) + " can't contain NaNs.");
				if (float.IsInfinity(v))
					throw new ArgumentException(nameof(values) + " can't contain Infinity.");

				storage[i] = v;
				hash.Add(v);
			}

			_values = storage;
			_precomputedHashCode = hash.ToHashCode();
		}

		// Views
		public ReadOnlySpan<float> AsSpan() => _values.AsSpan();

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((InstanceFeatures) obj!);

		// IEquatable
		public bool Equals([AllowNull] InstanceFeatures other) {
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
		public float this[int index] => _values[index];

		public int Count => _values.Length;

		public IEnumerator<float> GetEnumerator() => _values.GetGenericEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
	}
}
