namespace Minotaur.Classification.Rules {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.ExtensionMethods.SystemArray;

	public sealed class Consequent: IEquatable<Consequent>, IReadOnlyList<bool> {

		private readonly bool[] _labels;
		private readonly int _precomputedHashCode;

		public Consequent(ReadOnlySpan<bool> labels) {

			var storage = new bool[labels.Length];
			var hash = new HashCode();

			for (int i = 0; i < labels.Length; i++) {
				var v = labels[i];
				storage[i] = v;
				hash.Add(v);
			}

			_labels = storage;
			_precomputedHashCode = hash.ToHashCode();
		}

		// Views
		public ReadOnlySpan<bool> AsSpan() => _labels;

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((Consequent) obj!);

		// IEquatable
		public bool Equals([AllowNull] Consequent other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (ReferenceEquals(this, other))
				return true;

			var lhs = _labels.AsSpan();
			var rhs = other._labels.AsSpan();

			if (lhs.Length != rhs.Length)
				throw new InvalidOperationException();

			return lhs.SequenceEqual(rhs);
		}

		// IReadOnlyList
		public int Count => _labels.Length;

		public bool this[int index] => _labels[index];

		public IEnumerator<bool> GetEnumerator() => _labels.GetGenericEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _labels.GetEnumerator();
	}
}
